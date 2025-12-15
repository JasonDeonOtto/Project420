using Microsoft.Extensions.Logging;
using Project420.Inventory.BLL.DTOs;
using Project420.Inventory.DAL.Repositories;
using Project420.Inventory.Models.Entities;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service for stock transfer business logic with integrated Movement ledger generation.
/// </summary>
/// <remarks>
/// Stock Transfer Workflow:
/// 1. Create: Draft transfer with line items (no movements yet)
/// 2. Approve: Generate TransferOut movements at source location (SOH decreases)
/// 3. Complete: Generate TransferIn movements at destination location (SOH increases)
///
/// SAHPRA/SARS Compliance:
/// - All transfers tracked for audit trail
/// - Batch/serial numbers for seed-to-sale traceability
/// - Two-phase movement generation (out then in) for accurate location tracking
/// </remarks>
public class StockTransferService : IStockTransferService
{
    private readonly IStockTransferRepository _repository;
    private readonly IMovementService _movementService;
    private readonly ILogger<StockTransferService> _logger;

    // Transfer status constants
    private const string StatusDraft = "Draft";
    private const string StatusApproved = "Approved";
    private const string StatusInTransit = "In Transit";
    private const string StatusReceived = "Received";
    private const string StatusCancelled = "Cancelled";

    public StockTransferService(
        IStockTransferRepository repository,
        IMovementService movementService,
        ILogger<StockTransferService> logger)
    {
        _repository = repository;
        _movementService = movementService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new stock transfer (draft status, no movements generated).
    /// </summary>
    public async Task<StockTransferResultDto> CreateStockTransferAsync(CreateStockTransferDto dto)
    {
        _logger.LogInformation(
            "Creating stock transfer: {TransferNumber} from {FromLocation} to {ToLocation}",
            dto.TransferNumber, dto.FromLocation, dto.ToLocation);

        try
        {
            var transfer = new StockTransfer
            {
                TransferNumber = dto.TransferNumber,
                TransferDate = dto.TransferDate,
                FromLocation = dto.FromLocation,
                ToLocation = dto.ToLocation,
                Status = StatusDraft,
                RequestedBy = dto.RequestedBy,
                Notes = dto.Notes
            };

            await _repository.AddAsync(transfer);

            _logger.LogInformation(
                "Stock transfer created: {TransferId} - {TransferNumber}",
                transfer.Id, transfer.TransferNumber);

            return new StockTransferResultDto
            {
                TransferId = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                MovementsCreated = 0, // No movements in draft
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create stock transfer: {TransferNumber}", dto.TransferNumber);
            return new StockTransferResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Update transfer details (only allowed in Draft status).
    /// </summary>
    public async Task UpdateStockTransferAsync(UpdateStockTransferDto dto)
    {
        var transfer = await _repository.GetByIdAsync(dto.Id);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer {dto.Id} not found");

        if (transfer.Status != StatusDraft)
            throw new InvalidOperationException($"Cannot update transfer in {transfer.Status} status. Only Draft transfers can be modified.");

        transfer.TransferNumber = dto.TransferNumber;
        transfer.TransferDate = dto.TransferDate;
        transfer.FromLocation = dto.FromLocation;
        transfer.ToLocation = dto.ToLocation;
        transfer.Notes = dto.Notes;

        await _repository.UpdateAsync(transfer);
        _logger.LogInformation("Stock transfer {Id} updated", dto.Id);
    }

    /// <summary>
    /// Get stock transfer by ID.
    /// </summary>
    public async Task<StockTransferDto?> GetStockTransferByIdAsync(int id)
    {
        var transfer = await _repository.GetByIdAsync(id);
        return transfer == null ? null : MapToDto(transfer);
    }

    /// <summary>
    /// Get all stock transfers.
    /// </summary>
    public async Task<IEnumerable<StockTransferDto>> GetAllStockTransfersAsync()
    {
        var transfers = await _repository.GetAllAsync();
        return transfers.Select(MapToDto);
    }

    /// <summary>
    /// Soft-delete a stock transfer (only allowed in Draft status).
    /// </summary>
    public async Task DeactivateStockTransferAsync(int id)
    {
        var transfer = await _repository.GetByIdAsync(id);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer {id} not found");

        if (transfer.Status != StatusDraft)
            throw new InvalidOperationException($"Cannot delete transfer in {transfer.Status} status. Only Draft transfers can be deleted.");

        await _repository.DeleteAsync(id);
        _logger.LogInformation("Stock transfer {Id} deactivated", id);
    }

    /// <summary>
    /// Get stock transfers by location (source or destination).
    /// </summary>
    public async Task<IEnumerable<StockTransferDto>> GetStockTransfersByLocationAsync(string fromLocation, string toLocation)
    {
        var transfers = await _repository.FindAsync(t =>
            (string.IsNullOrEmpty(fromLocation) || t.FromLocation == fromLocation) &&
            (string.IsNullOrEmpty(toLocation) || t.ToLocation == toLocation));
        return transfers.Select(MapToDto);
    }

    /// <summary>
    /// Get pending transfers (Draft, Approved, In Transit).
    /// </summary>
    public async Task<IEnumerable<StockTransferDto>> GetPendingTransfersAsync()
    {
        var pending = await _repository.FindAsync(t =>
            t.Status == StatusDraft ||
            t.Status == StatusApproved ||
            t.Status == StatusInTransit);
        return pending.Select(MapToDto);
    }

    /// <summary>
    /// Approve a transfer - generates TransferOut movements at source location.
    /// </summary>
    public async Task<StockTransferResultDto> ApproveTransferAsync(ApproveStockTransferDto dto)
    {
        var transfer = await _repository.GetByIdAsync(dto.TransferId);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer {dto.TransferId} not found");

        if (transfer.Status != StatusDraft)
            throw new InvalidOperationException($"Cannot approve transfer in {transfer.Status} status. Only Draft transfers can be approved.");

        if (dto.Lines == null || !dto.Lines.Any())
            throw new InvalidOperationException("Transfer must have at least one line item to approve.");

        _logger.LogInformation(
            "Approving stock transfer {TransferId} with {LineCount} lines",
            dto.TransferId, dto.Lines.Count);

        try
        {
            var movementsCreated = 0;

            // Generate TransferOut movements for each line item
            foreach (var line in dto.Lines)
            {
                var movement = new Movement
                {
                    ProductId = line.ProductId,
                    ProductSKU = line.ProductSKU,
                    ProductName = line.ProductName,
                    MovementType = _movementService.GetMovementTypeName(TransactionType.TransferOut),
                    Direction = MovementDirection.Out,
                    Quantity = line.Quantity,
                    Mass = line.WeightGrams ?? 0,
                    Value = line.UnitCost.HasValue ? line.UnitCost.Value * line.Quantity : 0,
                    BatchNumber = line.BatchNumber,
                    SerialNumber = line.SerialNumber,
                    TransactionType = TransactionType.TransferOut,
                    HeaderId = transfer.Id,
                    DetailId = 0, // No detail ID for header-level transfer
                    MovementReason = $"Transfer Out: {transfer.TransferNumber} from {transfer.FromLocation} to {transfer.ToLocation}",
                    TransactionDate = DateTime.UtcNow,
                    UserId = dto.ApprovedBy,
                    LocationId = null // Could be enhanced to use FromLocationId
                };

                await _movementService.CreateMovementAsync(movement);
                movementsCreated++;
            }

            // Update transfer status
            transfer.AuthorizedBy = dto.ApprovedBy;
            transfer.Status = StatusInTransit;
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                transfer.Notes = string.IsNullOrEmpty(transfer.Notes)
                    ? dto.Notes
                    : $"{transfer.Notes}\nApproval: {dto.Notes}";
            }

            await _repository.UpdateAsync(transfer);

            _logger.LogInformation(
                "Stock transfer {TransferId} approved. {MovementsCreated} TransferOut movements created.",
                dto.TransferId, movementsCreated);

            return new StockTransferResultDto
            {
                TransferId = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                MovementsCreated = movementsCreated,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve stock transfer {TransferId}", dto.TransferId);
            return new StockTransferResultDto
            {
                TransferId = dto.TransferId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Complete a transfer - generates TransferIn movements at destination location.
    /// </summary>
    public async Task<StockTransferResultDto> CompleteTransferAsync(CompleteStockTransferDto dto)
    {
        var transfer = await _repository.GetByIdAsync(dto.TransferId);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer {dto.TransferId} not found");

        if (transfer.Status != StatusApproved && transfer.Status != StatusInTransit)
            throw new InvalidOperationException($"Cannot complete transfer in {transfer.Status} status. Only Approved/In Transit transfers can be completed.");

        if (dto.Lines == null || !dto.Lines.Any())
            throw new InvalidOperationException("Transfer must have at least one line item to complete.");

        _logger.LogInformation(
            "Completing stock transfer {TransferId} with {LineCount} lines received",
            dto.TransferId, dto.Lines.Count);

        try
        {
            var movementsCreated = 0;

            // Generate TransferIn movements for each line item received
            foreach (var line in dto.Lines)
            {
                var movement = new Movement
                {
                    ProductId = line.ProductId,
                    ProductSKU = line.ProductSKU,
                    ProductName = line.ProductName,
                    MovementType = _movementService.GetMovementTypeName(TransactionType.TransferIn),
                    Direction = MovementDirection.In,
                    Quantity = line.Quantity,
                    Mass = line.WeightGrams ?? 0,
                    Value = line.UnitCost.HasValue ? line.UnitCost.Value * line.Quantity : 0,
                    BatchNumber = line.BatchNumber,
                    SerialNumber = line.SerialNumber,
                    TransactionType = TransactionType.TransferIn,
                    HeaderId = transfer.Id,
                    DetailId = 0, // No detail ID for header-level transfer
                    MovementReason = $"Transfer In: {transfer.TransferNumber} received at {transfer.ToLocation} from {transfer.FromLocation}",
                    TransactionDate = dto.CompletionDate,
                    UserId = dto.ReceivedBy,
                    LocationId = null // Could be enhanced to use ToLocationId
                };

                await _movementService.CreateMovementAsync(movement);
                movementsCreated++;
            }

            // Update transfer status
            transfer.Status = StatusReceived;
            if (!string.IsNullOrEmpty(dto.Notes))
            {
                transfer.Notes = string.IsNullOrEmpty(transfer.Notes)
                    ? dto.Notes
                    : $"{transfer.Notes}\nCompletion: {dto.Notes}";
            }

            await _repository.UpdateAsync(transfer);

            _logger.LogInformation(
                "Stock transfer {TransferId} completed. {MovementsCreated} TransferIn movements created.",
                dto.TransferId, movementsCreated);

            return new StockTransferResultDto
            {
                TransferId = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                MovementsCreated = movementsCreated,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to complete stock transfer {TransferId}", dto.TransferId);
            return new StockTransferResultDto
            {
                TransferId = dto.TransferId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Cancel a transfer - reverses any movements if already approved.
    /// </summary>
    public async Task<StockTransferResultDto> CancelTransferAsync(int transferId, string reason)
    {
        var transfer = await _repository.GetByIdAsync(transferId);
        if (transfer == null)
            throw new InvalidOperationException($"Stock transfer {transferId} not found");

        if (transfer.Status == StatusReceived)
            throw new InvalidOperationException("Cannot cancel a completed transfer. Use a return transfer instead.");

        _logger.LogInformation("Cancelling stock transfer {TransferId}. Reason: {Reason}", transferId, reason);

        try
        {
            var movementsReversed = 0;

            // If transfer was approved/in transit, reverse the TransferOut movements
            if (transfer.Status == StatusApproved || transfer.Status == StatusInTransit)
            {
                movementsReversed = await _movementService.ReverseMovementsAsync(
                    TransactionType.TransferOut,
                    transferId,
                    $"Transfer cancelled: {reason}");

                _logger.LogInformation(
                    "Reversed {Count} TransferOut movements for transfer {TransferId}",
                    movementsReversed, transferId);
            }

            // Update transfer status
            transfer.Status = StatusCancelled;
            transfer.Notes = string.IsNullOrEmpty(transfer.Notes)
                ? $"Cancelled: {reason}"
                : $"{transfer.Notes}\nCancelled: {reason}";

            await _repository.UpdateAsync(transfer);

            return new StockTransferResultDto
            {
                TransferId = transfer.Id,
                TransferNumber = transfer.TransferNumber,
                MovementsCreated = movementsReversed, // Actually reversed
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cancel stock transfer {TransferId}", transferId);
            return new StockTransferResultDto
            {
                TransferId = transferId,
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    private static StockTransferDto MapToDto(StockTransfer entity)
    {
        return new StockTransferDto
        {
            Id = entity.Id,
            TransferNumber = entity.TransferNumber,
            TransferDate = entity.TransferDate,
            FromLocation = entity.FromLocation,
            ToLocation = entity.ToLocation,
            Status = entity.Status,
            RequestedBy = entity.RequestedBy,
            AuthorizedBy = entity.AuthorizedBy,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
    }
}
