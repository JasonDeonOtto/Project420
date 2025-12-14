using Project420.Inventory.BLL.DTOs;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service for inventory Stock on Hand (SOH) queries and alerts.
/// Provides a high-level API for inventory management built on top of MovementService.
/// </summary>
/// <remarks>
/// Architecture (Phase 11 - Inventory SOH Engine):
/// - Leverages MovementService for core SOH calculation (Option A - Movement Architecture)
/// - Adds caching layer for performance optimization
/// - Provides stock level alerts and notifications
/// - Supports inventory valuation and reporting
///
/// SOH Calculation:
/// - SOH is NEVER stored directly (calculated from movements)
/// - SOH = SUM(IN movements) - SUM(OUT movements)
/// - Historical SOH available via asOfDate parameter
/// - Batch-level SOH for FIFO/FEFO compliance
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Full audit trail via MovementService
/// - Batch tracking for seed-to-sale traceability
/// - Weight tracking for cannabis reconciliation
/// - Expiry tracking for FIFO/FEFO compliance
/// </remarks>
public interface IInventorySohService
{
    // ============================================================
    // SOH QUERIES (Leverage MovementService)
    // ============================================================

    /// <summary>
    /// Get current stock on hand for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="locationId">Optional: Location ID for location-specific SOH</param>
    /// <returns>Stock on hand DTO with full product details</returns>
    Task<StockOnHandDto?> GetStockOnHandAsync(int productId, int? locationId = null);

    /// <summary>
    /// Get stock on hand for multiple products efficiently.
    /// Uses batch query for performance.
    /// </summary>
    /// <param name="productIds">List of product IDs</param>
    /// <param name="locationId">Optional: Location ID for location-specific SOH</param>
    /// <returns>List of stock on hand DTOs</returns>
    Task<List<StockOnHandDto>> GetStockOnHandBulkAsync(IEnumerable<int> productIds, int? locationId = null);

    /// <summary>
    /// Get stock on hand by batch number (for FIFO/FEFO and traceability).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>List of batch-level SOH DTOs</returns>
    Task<List<BatchStockOnHandDto>> GetStockOnHandByBatchAsync(int productId);

    /// <summary>
    /// Get historical stock on hand as of a specific date.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="asOfDate">Date to calculate SOH as of</param>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>SOH as of the specified date</returns>
    Task<decimal> GetHistoricalStockOnHandAsync(int productId, DateTime asOfDate, int? locationId = null);

    /// <summary>
    /// Get all products with their current SOH.
    /// </summary>
    /// <param name="locationId">Optional: Location ID for location-specific SOH</param>
    /// <param name="includeZeroStock">Include products with zero stock</param>
    /// <returns>List of all products with SOH</returns>
    Task<List<StockOnHandDto>> GetAllStockOnHandAsync(int? locationId = null, bool includeZeroStock = false);

    // ============================================================
    // STOCK ALERTS
    // ============================================================

    /// <summary>
    /// Get all stock alerts (low stock, out of stock, expiring, expired).
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <param name="severity">Optional: Filter by severity</param>
    /// <returns>List of stock alerts</returns>
    Task<List<StockAlertDto>> GetStockAlertsAsync(int? locationId = null, AlertSeverity? severity = null);

    /// <summary>
    /// Get low stock alerts (products below minimum level).
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>List of low stock alerts</returns>
    Task<List<StockAlertDto>> GetLowStockAlertsAsync(int? locationId = null);

    /// <summary>
    /// Get out of stock alerts (products with zero stock).
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>List of out of stock alerts</returns>
    Task<List<StockAlertDto>> GetOutOfStockAlertsAsync(int? locationId = null);

    /// <summary>
    /// Get expiring stock alerts (batches expiring within specified days).
    /// </summary>
    /// <param name="daysUntilExpiry">Number of days to look ahead (default: 30)</param>
    /// <returns>List of expiring stock alerts</returns>
    Task<List<StockAlertDto>> GetExpiringStockAlertsAsync(int daysUntilExpiry = 30);

    /// <summary>
    /// Get expired stock alerts (batches past expiry date).
    /// </summary>
    /// <returns>List of expired stock alerts</returns>
    Task<List<StockAlertDto>> GetExpiredStockAlertsAsync();

    // ============================================================
    // INVENTORY VALUATION
    // ============================================================

    /// <summary>
    /// Get inventory valuation summary.
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>Inventory valuation DTO</returns>
    Task<InventoryValuationDto> GetInventoryValuationAsync(int? locationId = null);

    /// <summary>
    /// Get inventory valuation for a specific product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Product-specific valuation</returns>
    Task<StockOnHandDto?> GetProductValuationAsync(int productId);

    // ============================================================
    // MOVEMENT HISTORY
    // ============================================================

    /// <summary>
    /// Get movement history for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of movement history DTOs with running balance</returns>
    Task<List<StockMovementHistoryDto>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get movement history for a batch (seed-to-sale traceability).
    /// </summary>
    /// <param name="batchNumber">Batch number</param>
    /// <returns>List of movement history for the batch</returns>
    Task<List<StockMovementHistoryDto>> GetBatchMovementHistoryAsync(string batchNumber);

    // ============================================================
    // STOCK LEVEL CONFIGURATION
    // ============================================================

    /// <summary>
    /// Set stock levels for a product (minimum, maximum, reorder quantity).
    /// </summary>
    /// <param name="dto">Stock level configuration</param>
    /// <returns>True if successful</returns>
    Task<bool> SetStockLevelsAsync(SetStockLevelsDto dto);

    /// <summary>
    /// Get stock level configuration for a product.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Stock level configuration</returns>
    Task<SetStockLevelsDto?> GetStockLevelsAsync(int productId);

    // ============================================================
    // UTILITY METHODS
    // ============================================================

    /// <summary>
    /// Check if a product is in stock.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="requiredQuantity">Quantity needed (default: 1)</param>
    /// <returns>True if sufficient stock available</returns>
    Task<bool> IsInStockAsync(int productId, decimal requiredQuantity = 1);

    /// <summary>
    /// Get available quantity for a product (considering reservations if implemented).
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Available quantity</returns>
    Task<decimal> GetAvailableQuantityAsync(int productId);

    /// <summary>
    /// Count total products with stock.
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>Number of products with stock > 0</returns>
    Task<int> CountProductsInStockAsync(int? locationId = null);

    /// <summary>
    /// Count products with low or zero stock.
    /// </summary>
    /// <param name="locationId">Optional: Location ID</param>
    /// <returns>Tuple of (low stock count, out of stock count)</returns>
    Task<(int LowStock, int OutOfStock)> CountLowStockProductsAsync(int? locationId = null);
}
