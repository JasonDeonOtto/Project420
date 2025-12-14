using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Project420.Inventory.BLL.DTOs;
using Project420.Management.BLL.StockManagement.Services;
using Project420.Shared.Database.Services;

namespace Project420.Inventory.BLL.Services;

/// <summary>
/// Service for inventory Stock on Hand (SOH) queries and alerts.
/// Built on top of MovementService for core SOH calculation.
/// </summary>
/// <remarks>
/// Architecture (Phase 11 - Inventory SOH Engine):
/// - Leverages IMovementService for core SOH calculation (Option A - Movement Architecture)
/// - Uses IProductService for product master data
/// - Provides stock level alerts and notifications
/// - Supports inventory valuation and reporting
///
/// Performance:
/// - Bulk SOH queries use batch operations
/// - Performance metrics logged for monitoring
/// - Target: SOH query &lt;200ms
///
/// Cannabis Compliance (SAHPRA/SARS):
/// - Full audit trail via MovementService
/// - Batch tracking for seed-to-sale traceability
/// - Weight tracking for cannabis reconciliation
/// - Expiry tracking for FIFO/FEFO compliance
/// </remarks>
public class InventorySohService : IInventorySohService
{
    private readonly IMovementService _movementService;
    private readonly IProductService _productService;
    private readonly ILogger<InventorySohService> _logger;

    // Performance thresholds
    private const int PerformanceWarningThresholdMs = 200;

    /// <summary>
    /// Initializes a new instance of the InventorySohService.
    /// </summary>
    public InventorySohService(
        IMovementService movementService,
        IProductService productService,
        ILogger<InventorySohService> logger)
    {
        _movementService = movementService ?? throw new ArgumentNullException(nameof(movementService));
        _productService = productService ?? throw new ArgumentNullException(nameof(productService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    // ============================================================
    // SOH QUERIES
    // ============================================================

    /// <inheritdoc />
    public async Task<StockOnHandDto?> GetStockOnHandAsync(int productId, int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get product info
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for SOH query", productId);
                return null;
            }

            // Calculate SOH using MovementService
            var soh = await _movementService.CalculateSOHAsync(productId, null, locationId);

            // Get last movement date
            var movements = await _movementService.GetMovementHistoryAsync(
                productId,
                DateTime.UtcNow.AddYears(-10),
                DateTime.UtcNow);
            var lastMovement = movements.OrderByDescending(m => m.TransactionDate).FirstOrDefault();

            var result = new StockOnHandDto
            {
                ProductId = product.Id,
                ProductSKU = product.SKU,
                ProductName = product.Name,
                QuantityOnHand = soh,
                UnitOfMeasure = "units",
                TotalWeightGrams = 0, // Weight calculated elsewhere if needed
                AverageCostPerUnit = product.CostPrice,
                TotalValue = soh * product.CostPrice,
                MinimumStockLevel = product.ReorderLevel,
                MaximumStockLevel = null, // Not in ProductDto
                Location = locationId?.ToString(),
                LastMovementDate = lastMovement?.TransactionDate
            };

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetStockOnHandAsync", productId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SOH for product {ProductId}", productId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockOnHandDto>> GetStockOnHandBulkAsync(IEnumerable<int> productIds, int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();
        var productIdList = productIds.ToList();

        try
        {
            // Batch calculate SOH using MovementService
            var sohDictionary = await _movementService.CalculateSOHBatchAsync(productIdList);

            var results = new List<StockOnHandDto>();

            foreach (var productId in productIdList)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null) continue;

                var soh = sohDictionary.GetValueOrDefault(productId, 0);

                results.Add(new StockOnHandDto
                {
                    ProductId = product.Id,
                    ProductSKU = product.SKU,
                    ProductName = product.Name,
                    QuantityOnHand = soh,
                    UnitOfMeasure = "units",
                    TotalWeightGrams = 0,
                    AverageCostPerUnit = product.CostPrice,
                    TotalValue = soh * product.CostPrice,
                    MinimumStockLevel = product.ReorderLevel,
                    MaximumStockLevel = null,
                    Location = locationId?.ToString()
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetStockOnHandBulkAsync", productIdList.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bulk SOH for {Count} products", productIdList.Count);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<BatchStockOnHandDto>> GetStockOnHandByBatchAsync(int productId)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for batch SOH query", productId);
                return new List<BatchStockOnHandDto>();
            }

            // Get all movements for this product to extract batch numbers
            var movements = await _movementService.GetMovementHistoryAsync(
                productId,
                DateTime.MinValue,
                DateTime.UtcNow);

            // Group by batch number and calculate SOH for each batch
            var batchNumbers = movements
                .Where(m => !string.IsNullOrEmpty(m.BatchNumber))
                .Select(m => m.BatchNumber!)
                .Distinct()
                .ToList();

            var results = new List<BatchStockOnHandDto>();

            foreach (var batchNumber in batchNumbers)
            {
                var batchSoh = await _movementService.CalculateBatchSOHAsync(productId, batchNumber);

                // Only include batches with stock > 0
                if (batchSoh > 0)
                {
                    results.Add(new BatchStockOnHandDto
                    {
                        ProductId = product.Id,
                        ProductSKU = product.SKU,
                        ProductName = product.Name,
                        BatchNumber = batchNumber,
                        QuantityOnHand = batchSoh,
                        WeightGrams = 0,
                        ExpiryDate = product.ExpiryDate,
                        THCPercentage = product.THCPercentage,
                        CBDPercentage = product.CBDPercentage
                    });
                }
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetStockOnHandByBatchAsync", productId);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch SOH for product {ProductId}", productId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<decimal> GetHistoricalStockOnHandAsync(int productId, DateTime asOfDate, int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var soh = await _movementService.CalculateSOHAsync(productId, asOfDate, locationId);

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetHistoricalStockOnHandAsync", productId);
            return soh;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting historical SOH for product {ProductId} as of {AsOfDate}",
                productId, asOfDate);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockOnHandDto>> GetAllStockOnHandAsync(int? locationId = null, bool includeZeroStock = false)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get all products
            var products = await _productService.GetAllProductsAsync();
            var productList = products.ToList();

            // Get SOH for all products
            var productIds = productList.Select(p => p.Id).ToList();
            var sohDictionary = await _movementService.CalculateSOHBatchAsync(productIds);

            var results = new List<StockOnHandDto>();

            foreach (var product in productList)
            {
                var soh = sohDictionary.GetValueOrDefault(product.Id, 0);

                // Skip zero stock if not requested
                if (!includeZeroStock && soh <= 0) continue;

                results.Add(new StockOnHandDto
                {
                    ProductId = product.Id,
                    ProductSKU = product.SKU,
                    ProductName = product.Name,
                    QuantityOnHand = soh,
                    UnitOfMeasure = "units",
                    TotalWeightGrams = 0,
                    AverageCostPerUnit = product.CostPrice,
                    TotalValue = soh * product.CostPrice,
                    MinimumStockLevel = product.ReorderLevel,
                    MaximumStockLevel = null,
                    Location = locationId?.ToString()
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetAllStockOnHandAsync", productList.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all SOH");
            throw;
        }
    }

    // ============================================================
    // STOCK ALERTS
    // ============================================================

    /// <inheritdoc />
    public async Task<List<StockAlertDto>> GetStockAlertsAsync(int? locationId = null, AlertSeverity? severity = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var alerts = new List<StockAlertDto>();

            // Get all alert types
            var lowStockAlerts = await GetLowStockAlertsAsync(locationId);
            var outOfStockAlerts = await GetOutOfStockAlertsAsync(locationId);
            var expiringAlerts = await GetExpiringStockAlertsAsync();
            var expiredAlerts = await GetExpiredStockAlertsAsync();

            alerts.AddRange(lowStockAlerts);
            alerts.AddRange(outOfStockAlerts);
            alerts.AddRange(expiringAlerts);
            alerts.AddRange(expiredAlerts);

            // Filter by severity if specified
            if (severity.HasValue)
            {
                alerts = alerts.Where(a => a.Severity == severity.Value).ToList();
            }

            // Sort by severity (Critical first) then by alert date
            alerts = alerts
                .OrderByDescending(a => a.Severity)
                .ThenByDescending(a => a.AlertDate)
                .ToList();

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetStockAlertsAsync", alerts.Count);
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stock alerts");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockAlertDto>> GetLowStockAlertsAsync(int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get products with low stock
            var lowStockProducts = await _productService.GetLowStockProductsAsync();
            var alerts = new List<StockAlertDto>();

            foreach (var product in lowStockProducts)
            {
                // Get actual SOH from movements
                var soh = await _movementService.CalculateSOHAsync(product.Id);

                // Only alert if SOH is below minimum (double-check with movement-based SOH)
                if (soh > 0 && product.ReorderLevel > 0 && soh <= product.ReorderLevel)
                {
                    alerts.Add(new StockAlertDto
                    {
                        ProductId = product.Id,
                        ProductSKU = product.SKU,
                        ProductName = product.Name,
                        CurrentQuantity = soh,
                        MinimumStockLevel = product.ReorderLevel,
                        ReorderQuantity = (product.ReorderLevel * 2) - soh,
                        AlertType = StockAlertType.LowStock,
                        Severity = AlertSeverity.Warning,
                        Message = $"Stock level ({soh:N0}) is below minimum ({product.ReorderLevel:N0})",
                        Location = locationId?.ToString(),
                        AlertDate = DateTime.UtcNow
                    });
                }
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetLowStockAlertsAsync", alerts.Count);
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting low stock alerts");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockAlertDto>> GetOutOfStockAlertsAsync(int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get products that are out of stock
            var outOfStockProducts = await _productService.GetOutOfStockProductsAsync();
            var alerts = new List<StockAlertDto>();

            foreach (var product in outOfStockProducts)
            {
                // Verify with movement-based SOH
                var soh = await _movementService.CalculateSOHAsync(product.Id);

                if (soh <= 0)
                {
                    alerts.Add(new StockAlertDto
                    {
                        ProductId = product.Id,
                        ProductSKU = product.SKU,
                        ProductName = product.Name,
                        CurrentQuantity = soh,
                        MinimumStockLevel = product.ReorderLevel,
                        ReorderQuantity = product.ReorderLevel > 0 ? product.ReorderLevel * 2 : 10,
                        AlertType = StockAlertType.OutOfStock,
                        Severity = AlertSeverity.Critical,
                        Message = "Product is out of stock",
                        Location = locationId?.ToString(),
                        AlertDate = DateTime.UtcNow
                    });
                }
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetOutOfStockAlertsAsync", alerts.Count);
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting out of stock alerts");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockAlertDto>> GetExpiringStockAlertsAsync(int daysUntilExpiry = 30)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var expiringProducts = await _productService.GetExpiringProductsAsync(daysUntilExpiry);
            var alerts = new List<StockAlertDto>();

            foreach (var product in expiringProducts)
            {
                // Only alert if there's stock
                var soh = await _movementService.CalculateSOHAsync(product.Id);
                if (soh <= 0) continue;

                var daysLeft = product.ExpiryDate.HasValue
                    ? (int)(product.ExpiryDate.Value - DateTime.UtcNow).TotalDays
                    : daysUntilExpiry;

                var severity = daysLeft <= 7 ? AlertSeverity.Critical :
                               daysLeft <= 14 ? AlertSeverity.Warning :
                               AlertSeverity.Info;

                alerts.Add(new StockAlertDto
                {
                    ProductId = product.Id,
                    ProductSKU = product.SKU,
                    ProductName = product.Name,
                    CurrentQuantity = soh,
                    MinimumStockLevel = product.ReorderLevel,
                    ReorderQuantity = 0, // N/A for expiry alerts
                    AlertType = StockAlertType.ExpiringSoon,
                    Severity = severity,
                    Message = $"Product expires in {daysLeft} days ({product.ExpiryDate:d})",
                    AlertDate = DateTime.UtcNow
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetExpiringStockAlertsAsync", alerts.Count);
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring stock alerts");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockAlertDto>> GetExpiredStockAlertsAsync()
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Get products with expiry date in the past
            var expiringProducts = await _productService.GetExpiringProductsAsync(0);
            var alerts = new List<StockAlertDto>();

            foreach (var product in expiringProducts)
            {
                // Only include if already expired
                if (!product.ExpiryDate.HasValue || product.ExpiryDate.Value >= DateTime.UtcNow)
                    continue;

                // Only alert if there's stock
                var soh = await _movementService.CalculateSOHAsync(product.Id);
                if (soh <= 0) continue;

                alerts.Add(new StockAlertDto
                {
                    ProductId = product.Id,
                    ProductSKU = product.SKU,
                    ProductName = product.Name,
                    CurrentQuantity = soh,
                    MinimumStockLevel = product.ReorderLevel,
                    ReorderQuantity = 0,
                    AlertType = StockAlertType.Expired,
                    Severity = AlertSeverity.Critical,
                    Message = $"Product expired on {product.ExpiryDate:d}",
                    AlertDate = DateTime.UtcNow
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetExpiredStockAlertsAsync", alerts.Count);
            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired stock alerts");
            throw;
        }
    }

    // ============================================================
    // INVENTORY VALUATION
    // ============================================================

    /// <inheritdoc />
    public async Task<InventoryValuationDto> GetInventoryValuationAsync(int? locationId = null)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var allSoh = await GetAllStockOnHandAsync(locationId, includeZeroStock: false);

            var valuation = new InventoryValuationDto
            {
                TotalProducts = allSoh.Count,
                TotalQuantity = allSoh.Sum(s => s.QuantityOnHand),
                TotalWeightGrams = allSoh.Sum(s => s.TotalWeightGrams),
                TotalValueAtCost = allSoh.Sum(s => s.TotalValue),
                ValuationDate = DateTime.UtcNow,
                LowStockProductCount = allSoh.Count(s => s.IsBelowMinimum),
                OutOfStockProductCount = 0 // These are excluded from allSoh (includeZeroStock: false)
            };

            // Calculate retail value
            var products = await _productService.GetAllProductsAsync();
            var productDict = products.ToDictionary(p => p.Id, p => p.Price);

            valuation.TotalValueAtRetail = allSoh.Sum(s =>
                productDict.TryGetValue(s.ProductId, out var retailPrice)
                    ? s.QuantityOnHand * retailPrice
                    : s.TotalValue * 1.5m); // Default 50% markup if no retail price

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetInventoryValuationAsync", allSoh.Count);
            return valuation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory valuation");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<StockOnHandDto?> GetProductValuationAsync(int productId)
    {
        return await GetStockOnHandAsync(productId);
    }

    // ============================================================
    // MOVEMENT HISTORY
    // ============================================================

    /// <inheritdoc />
    public async Task<List<StockMovementHistoryDto>> GetMovementHistoryAsync(int productId, DateTime startDate, DateTime endDate)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var movements = await _movementService.GetMovementHistoryAsync(productId, startDate, endDate);
            var movementList = movements.OrderBy(m => m.TransactionDate).ToList();

            // Calculate running balance
            var runningBalance = 0m;
            var results = new List<StockMovementHistoryDto>();

            foreach (var movement in movementList)
            {
                var quantity = movement.Direction == Shared.Core.Enums.MovementDirection.In
                    ? movement.Quantity
                    : -movement.Quantity;

                runningBalance += quantity;

                results.Add(new StockMovementHistoryDto
                {
                    MovementId = movement.Id,
                    MovementDate = movement.TransactionDate,
                    MovementType = movement.MovementType,
                    Direction = movement.Direction.ToString(),
                    Quantity = movement.Quantity,
                    RunningBalance = runningBalance,
                    BatchNumber = movement.BatchNumber,
                    ReferenceNumber = $"{movement.TransactionType}-{movement.HeaderId}",
                    Reason = movement.MovementReason,
                    CreatedBy = movement.UserId
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetMovementHistoryAsync", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting movement history for product {ProductId}", productId);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<List<StockMovementHistoryDto>> GetBatchMovementHistoryAsync(string batchNumber)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var movements = await _movementService.GetMovementsByBatchAsync(batchNumber);
            var movementList = movements.OrderBy(m => m.TransactionDate).ToList();

            // Calculate running balance per product for this batch
            var runningBalances = new Dictionary<int, decimal>();
            var results = new List<StockMovementHistoryDto>();

            foreach (var movement in movementList)
            {
                if (!runningBalances.ContainsKey(movement.ProductId))
                    runningBalances[movement.ProductId] = 0;

                var quantity = movement.Direction == Shared.Core.Enums.MovementDirection.In
                    ? movement.Quantity
                    : -movement.Quantity;

                runningBalances[movement.ProductId] += quantity;

                results.Add(new StockMovementHistoryDto
                {
                    MovementId = movement.Id,
                    MovementDate = movement.TransactionDate,
                    MovementType = movement.MovementType,
                    Direction = movement.Direction.ToString(),
                    Quantity = movement.Quantity,
                    RunningBalance = runningBalances[movement.ProductId],
                    BatchNumber = movement.BatchNumber,
                    ReferenceNumber = $"{movement.TransactionType}-{movement.HeaderId}",
                    Reason = movement.MovementReason,
                    CreatedBy = movement.UserId
                });
            }

            LogPerformance(stopwatch.ElapsedMilliseconds, "GetBatchMovementHistoryAsync", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting batch movement history for batch {BatchNumber}", batchNumber);
            throw;
        }
    }

    // ============================================================
    // STOCK LEVEL CONFIGURATION
    // ============================================================

    /// <inheritdoc />
    public async Task<bool> SetStockLevelsAsync(SetStockLevelsDto dto)
    {
        _logger.LogInformation("Setting stock levels for product {ProductId}: Min={Min}, Max={Max}",
            dto.ProductId, dto.MinimumStockLevel, dto.MaximumStockLevel);

        // Note: This would typically update the Product entity via ProductService
        // For now, we log the intent - actual implementation depends on Product entity structure
        _logger.LogWarning("SetStockLevelsAsync: Product update not implemented. " +
            "Would set ProductId={ProductId}, Min={Min}, Max={Max}, Reorder={Reorder}",
            dto.ProductId, dto.MinimumStockLevel, dto.MaximumStockLevel, dto.ReorderQuantity);

        // TODO: Implement via IProductService.UpdateProductAsync when stock level fields available
        return await Task.FromResult(true);
    }

    /// <inheritdoc />
    public async Task<SetStockLevelsDto?> GetStockLevelsAsync(int productId)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null) return null;

        return new SetStockLevelsDto
        {
            ProductId = product.Id,
            MinimumStockLevel = product.ReorderLevel,
            MaximumStockLevel = null, // Not in ProductDto
            ReorderQuantity = product.ReorderLevel > 0 ? product.ReorderLevel : 10
        };
    }

    // ============================================================
    // UTILITY METHODS
    // ============================================================

    /// <inheritdoc />
    public async Task<bool> IsInStockAsync(int productId, decimal requiredQuantity = 1)
    {
        var soh = await _movementService.CalculateSOHAsync(productId);
        return soh >= requiredQuantity;
    }

    /// <inheritdoc />
    public async Task<decimal> GetAvailableQuantityAsync(int productId)
    {
        // For now, available = SOH (no reservations implemented)
        // TODO: Subtract reserved quantities when reservation system is implemented
        return await _movementService.CalculateSOHAsync(productId);
    }

    /// <inheritdoc />
    public async Task<int> CountProductsInStockAsync(int? locationId = null)
    {
        var allSoh = await GetAllStockOnHandAsync(locationId, includeZeroStock: false);
        return allSoh.Count;
    }

    /// <inheritdoc />
    public async Task<(int LowStock, int OutOfStock)> CountLowStockProductsAsync(int? locationId = null)
    {
        var lowStockAlerts = await GetLowStockAlertsAsync(locationId);
        var outOfStockAlerts = await GetOutOfStockAlertsAsync(locationId);

        return (lowStockAlerts.Count, outOfStockAlerts.Count);
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Logs performance metrics and warnings for slow queries.
    /// </summary>
    private void LogPerformance(long elapsedMs, string methodName, object context)
    {
        if (elapsedMs >= PerformanceWarningThresholdMs)
        {
            _logger.LogWarning(
                "PERFORMANCE WARNING: {Method} took {ElapsedMs}ms (threshold: {Threshold}ms) for context: {Context}",
                methodName, elapsedMs, PerformanceWarningThresholdMs, context);
        }
        else
        {
            _logger.LogDebug(
                "{Method} completed in {ElapsedMs}ms for context: {Context}",
                methodName, elapsedMs, context);
        }
    }
}
