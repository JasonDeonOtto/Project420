using Microsoft.Extensions.Logging;
using Project420.Inventory.BLL.DTOs;
using Project420.Inventory.DAL.Repositories;
using Project420.Inventory.Models.Entities;
using Project420.Inventory.Models.Enums;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;
using Project420.Shared.Database.Services;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service for stock movement business logic with integrated Movement ledger generation.
/// </summary>
/// <remarks>
/// This service:
/// 1. Creates module-level StockMovement records for detailed tracking
/// 2. Generates unified Movement records via MovementService for SOH calculation
///
/// SAHPRA/SARS Compliance:
/// - All stock movements tracked for audit trail
/// - Batch/serial numbers for seed-to-sale traceability
/// - Weight tracking for cannabis reconciliation
/// </remarks>
public class StockMovementService : IStockMovementService
{
    private readonly IStockMovementRepository _repository;
    private readonly IMovementService _movementService;
    private readonly ILogger<StockMovementService> _logger;

    public StockMovementService(
        IStockMovementRepository repository,
        IMovementService movementService,
        ILogger<StockMovementService> logger)
    {
        _repository = repository;
        _movementService = movementService;
        _logger = logger;
    }

    /// <summary>
    /// Create a stock movement with integrated Movement record generation.
    /// </summary>
    public async Task<StockMovementResultDto> CreateStockMovementAsync(CreateStockMovementDto dto)
    {
        _logger.LogInformation(
            "Creating stock movement: {MovementType} for {ProductSKU}, Qty: {Quantity}",
            dto.MovementType, dto.ProductSKU, dto.Quantity);

        try
        {
            // 1. Create the module-level StockMovement entity
            var stockMovement = new StockMovement
            {
                MovementNumber = dto.MovementNumber,
                MovementType = dto.MovementType,
                MovementDate = dto.MovementDate,
                ProductSKU = dto.ProductSKU,
                ProductName = dto.ProductName,
                BatchNumber = dto.BatchNumber,
                Quantity = dto.Quantity,
                WeightGrams = dto.WeightGrams,
                UnitCost = dto.UnitCost,
                TotalValue = dto.UnitCost.HasValue ? dto.UnitCost.Value * Math.Abs(dto.Quantity) : null,
                FromLocation = dto.FromLocation,
                ToLocation = dto.ToLocation,
                ReferenceNumber = dto.ReferenceNumber,
                ReferenceType = dto.ReferenceType,
                Reason = dto.Reason,
                Notes = dto.Notes
            };

            await _repository.AddAsync(stockMovement);
            // SaveChanges is handled internally by the repository

            // 2. Create the unified Movement record for SOH calculation
            var (transactionType, direction) = MapToTransactionType(dto.MovementType, dto.Quantity);

            var movement = new Movement
            {
                ProductId = dto.ProductId,
                ProductSKU = dto.ProductSKU,
                ProductName = dto.ProductName,
                MovementType = _movementService.GetMovementTypeName(transactionType),
                Direction = direction,
                Quantity = Math.Abs(dto.Quantity),
                Mass = dto.WeightGrams ?? 0,
                Value = dto.UnitCost.HasValue ? dto.UnitCost.Value * Math.Abs(dto.Quantity) : 0,
                BatchNumber = dto.BatchNumber,
                SerialNumber = dto.SerialNumber,
                TransactionType = transactionType,
                HeaderId = stockMovement.Id, // Link to StockMovement
                DetailId = stockMovement.Id, // Same as header for direct movements
                MovementReason = dto.Reason ?? $"Stock movement: {dto.MovementType}",
                TransactionDate = dto.MovementDate,
                UserId = dto.UserId,
                LocationId = dto.LocationId
            };

            var createdMovement = await _movementService.CreateMovementAsync(movement);

            _logger.LogInformation(
                "Stock movement created successfully. StockMovementId: {StockMovementId}, MovementId: {MovementId}",
                stockMovement.Id, createdMovement.Id);

            return new StockMovementResultDto
            {
                StockMovementId = stockMovement.Id,
                MovementId = createdMovement.Id,
                MovementNumber = stockMovement.MovementNumber,
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create stock movement: {MovementType} for {ProductSKU}",
                dto.MovementType, dto.ProductSKU);

            return new StockMovementResultDto
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Update a stock movement (limited fields - core data is immutable for audit).
    /// </summary>
    public async Task UpdateStockMovementAsync(UpdateStockMovementDto dto)
    {
        var stockMovement = await _repository.GetByIdAsync(dto.Id);
        if (stockMovement == null)
            throw new InvalidOperationException($"Stock movement {dto.Id} not found");

        // Only allow updating non-critical fields (for compliance - quantity and type are immutable)
        stockMovement.Notes = dto.Notes;
        stockMovement.ReferenceNumber = dto.ReferenceNumber;

        await _repository.UpdateAsync(stockMovement);
        // SaveChanges is handled internally by the repository

        _logger.LogInformation("Stock movement {Id} updated", dto.Id);
    }

    /// <summary>
    /// Get stock movement by ID.
    /// </summary>
    public async Task<StockMovementDto?> GetStockMovementByIdAsync(int id)
    {
        var stockMovement = await _repository.GetByIdAsync(id);
        return stockMovement == null ? null : MapToDto(stockMovement);
    }

    /// <summary>
    /// Get all stock movements.
    /// </summary>
    public async Task<IEnumerable<StockMovementDto>> GetAllStockMovementsAsync()
    {
        var movements = await _repository.GetAllAsync();
        return movements.Select(MapToDto);
    }

    /// <summary>
    /// Soft-delete a stock movement and reverse associated Movement record.
    /// </summary>
    public async Task DeactivateStockMovementAsync(int id, string reason)
    {
        var stockMovement = await _repository.GetByIdAsync(id);
        if (stockMovement == null)
            throw new InvalidOperationException($"Stock movement {id} not found");

        // 1. Soft-delete the module-level StockMovement
        await _repository.DeleteAsync(id);
        // SaveChanges is handled internally by the repository

        // 2. Reverse the associated Movement in the unified ledger
        var (transactionType, _) = MapToTransactionType(stockMovement.MovementType, stockMovement.Quantity);
        var reversedCount = await _movementService.ReverseMovementsAsync(
            transactionType,
            id, // HeaderId was set to StockMovement.Id
            reason);

        _logger.LogInformation(
            "Stock movement {Id} deactivated and {ReversedCount} movements reversed. Reason: {Reason}",
            id, reversedCount, reason);
    }

    /// <summary>
    /// Get stock movements by product SKU.
    /// </summary>
    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsByProductAsync(string productSku)
    {
        var movements = await _repository.FindAsync(m => m.ProductSKU == productSku);
        return movements.Select(MapToDto);
    }

    /// <summary>
    /// Get stock movements within a date range.
    /// </summary>
    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var movements = await _repository.FindAsync(m =>
            m.MovementDate >= startDate && m.MovementDate <= endDate);
        return movements.Select(MapToDto);
    }

    /// <summary>
    /// Get stock movements by type.
    /// </summary>
    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsByTypeAsync(string movementType)
    {
        if (Enum.TryParse<StockMovementType>(movementType, true, out var type))
        {
            var movements = await _repository.FindAsync(m => m.MovementType == type);
            return movements.Select(MapToDto);
        }
        return Enumerable.Empty<StockMovementDto>();
    }

    /// <summary>
    /// Get stock movements by batch number (seed-to-sale traceability).
    /// </summary>
    public async Task<IEnumerable<StockMovementDto>> GetStockMovementsByBatchAsync(string batchNumber)
    {
        var movements = await _repository.FindAsync(m => m.BatchNumber == batchNumber);
        return movements.Select(MapToDto);
    }

    // ============================================================
    // PRIVATE HELPERS
    // ============================================================

    /// <summary>
    /// Map StockMovementType to TransactionType and determine MovementDirection.
    /// </summary>
    private (TransactionType transactionType, MovementDirection direction) MapToTransactionType(
        StockMovementType stockMovementType,
        int quantity)
    {
        return stockMovementType switch
        {
            StockMovementType.GoodsReceived => (TransactionType.GRV, MovementDirection.In),
            StockMovementType.Sale => (TransactionType.Sale, MovementDirection.Out),
            StockMovementType.TransferOut => (TransactionType.TransferOut, MovementDirection.Out),
            StockMovementType.TransferIn => (TransactionType.TransferIn, MovementDirection.In),
            StockMovementType.Return => (TransactionType.Refund, MovementDirection.In),
            StockMovementType.Waste => (TransactionType.AdjustmentOut, MovementDirection.Out),
            StockMovementType.Adjustment => quantity >= 0
                ? (TransactionType.AdjustmentIn, MovementDirection.In)
                : (TransactionType.AdjustmentOut, MovementDirection.Out),
            _ => throw new ArgumentOutOfRangeException(nameof(stockMovementType),
                $"Unknown stock movement type: {stockMovementType}")
        };
    }

    /// <summary>
    /// Map StockMovement entity to DTO.
    /// </summary>
    private static StockMovementDto MapToDto(StockMovement entity)
    {
        return new StockMovementDto
        {
            Id = entity.Id,
            MovementNumber = entity.MovementNumber,
            MovementType = entity.MovementType,
            MovementDate = entity.MovementDate,
            ProductSKU = entity.ProductSKU,
            ProductName = entity.ProductName,
            BatchNumber = entity.BatchNumber,
            Quantity = entity.Quantity,
            UnitCost = entity.UnitCost,
            Location = entity.ToLocation ?? entity.FromLocation,
            ReferenceNumber = entity.ReferenceNumber,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            CreatedBy = entity.CreatedBy
        };
    }
}
