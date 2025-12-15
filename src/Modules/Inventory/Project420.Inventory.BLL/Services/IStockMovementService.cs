using Project420.Inventory.BLL.DTOs;
using Project420.Inventory.Models.Entities;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service interface for stock movement business logic.
/// SARS/SAHPRA Compliance: Stock movement tracking for tax reconciliation and seed-to-sale traceability.
/// </summary>
/// <remarks>
/// This service:
/// 1. Creates module-level StockMovement records for detailed tracking
/// 2. Generates unified Movement records via MovementService for SOH calculation
/// </remarks>
public interface IStockMovementService
{
    /// <summary>
    /// Create a stock movement with integrated Movement record generation.
    /// </summary>
    /// <param name="dto">Stock movement details</param>
    /// <returns>ID of the created StockMovement</returns>
    Task<StockMovementResultDto> CreateStockMovementAsync(CreateStockMovementDto dto);

    /// <summary>
    /// Update a stock movement (limited fields - movements are immutable for audit).
    /// </summary>
    Task UpdateStockMovementAsync(UpdateStockMovementDto dto);

    /// <summary>
    /// Get stock movement by ID.
    /// </summary>
    Task<StockMovementDto?> GetStockMovementByIdAsync(int id);

    /// <summary>
    /// Get all stock movements.
    /// </summary>
    Task<IEnumerable<StockMovementDto>> GetAllStockMovementsAsync();

    /// <summary>
    /// Soft-delete a stock movement and reverse associated Movement record.
    /// </summary>
    Task DeactivateStockMovementAsync(int id, string reason);

    /// <summary>
    /// Get stock movements by product SKU.
    /// </summary>
    Task<IEnumerable<StockMovementDto>> GetStockMovementsByProductAsync(string productSku);

    /// <summary>
    /// Get stock movements within a date range.
    /// </summary>
    Task<IEnumerable<StockMovementDto>> GetStockMovementsByDateRangeAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get stock movements by type.
    /// </summary>
    Task<IEnumerable<StockMovementDto>> GetStockMovementsByTypeAsync(string movementType);

    /// <summary>
    /// Get stock movements by batch number (seed-to-sale traceability).
    /// </summary>
    Task<IEnumerable<StockMovementDto>> GetStockMovementsByBatchAsync(string batchNumber);
}
