using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service interface for stock transfer business logic with integrated Movement generation.
/// </summary>
/// <remarks>
/// Stock Transfer Workflow:
/// 1. Create: Draft transfer with line items (no movements yet)
/// 2. Approve: Generate TransferOut movements at source location
/// 3. Complete: Generate TransferIn movements at destination location
///
/// SAHPRA Compliance: Stock transfers tracked for seed-to-sale traceability.
/// </remarks>
public interface IStockTransferService
{
    /// <summary>
    /// Create a new stock transfer (draft status, no movements generated).
    /// </summary>
    Task<StockTransferResultDto> CreateStockTransferAsync(CreateStockTransferDto dto);

    /// <summary>
    /// Update transfer details (only allowed in Draft status).
    /// </summary>
    Task UpdateStockTransferAsync(UpdateStockTransferDto dto);

    /// <summary>
    /// Get stock transfer by ID.
    /// </summary>
    Task<StockTransferDto?> GetStockTransferByIdAsync(int id);

    /// <summary>
    /// Get all stock transfers.
    /// </summary>
    Task<IEnumerable<StockTransferDto>> GetAllStockTransfersAsync();

    /// <summary>
    /// Soft-delete a stock transfer (only allowed in Draft status).
    /// </summary>
    Task DeactivateStockTransferAsync(int id);

    /// <summary>
    /// Get stock transfers by location (source or destination).
    /// </summary>
    Task<IEnumerable<StockTransferDto>> GetStockTransfersByLocationAsync(string fromLocation, string toLocation);

    /// <summary>
    /// Get pending transfers (Draft, Approved, In Transit).
    /// </summary>
    Task<IEnumerable<StockTransferDto>> GetPendingTransfersAsync();

    /// <summary>
    /// Approve a transfer - generates TransferOut movements at source location.
    /// </summary>
    /// <param name="dto">Approval details with line items to ship</param>
    /// <returns>Result with number of movements created</returns>
    Task<StockTransferResultDto> ApproveTransferAsync(ApproveStockTransferDto dto);

    /// <summary>
    /// Complete a transfer - generates TransferIn movements at destination location.
    /// </summary>
    /// <param name="dto">Completion details with line items received</param>
    /// <returns>Result with number of movements created</returns>
    Task<StockTransferResultDto> CompleteTransferAsync(CompleteStockTransferDto dto);

    /// <summary>
    /// Cancel a transfer - reverses any movements if already approved.
    /// </summary>
    Task<StockTransferResultDto> CancelTransferAsync(int transferId, string reason);
}
