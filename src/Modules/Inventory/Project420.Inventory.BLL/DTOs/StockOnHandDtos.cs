using System.ComponentModel.DataAnnotations;

namespace Project420.Inventory.BLL.DTOs;

// ============================================================
// STOCK ON HAND DTOs (Phase 11 - Inventory SOH Engine)
// ============================================================

/// <summary>
/// Current stock on hand for a product.
/// </summary>
public class StockOnHandDto
{
    /// <summary>Product ID.</summary>
    public int ProductId { get; set; }

    /// <summary>Product SKU.</summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Current quantity on hand.</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Unit of measure (e.g., "g", "units", "kg").</summary>
    public string UnitOfMeasure { get; set; } = "units";

    /// <summary>Current total weight in grams (for cannabis products).</summary>
    public decimal TotalWeightGrams { get; set; }

    /// <summary>Weighted average cost per unit.</summary>
    public decimal AverageCostPerUnit { get; set; }

    /// <summary>Total inventory value (quantity × avg cost).</summary>
    public decimal TotalValue { get; set; }

    /// <summary>Minimum stock level (reorder point).</summary>
    public decimal? MinimumStockLevel { get; set; }

    /// <summary>Maximum stock level.</summary>
    public decimal? MaximumStockLevel { get; set; }

    /// <summary>Whether stock is below minimum level.</summary>
    public bool IsBelowMinimum => MinimumStockLevel.HasValue && QuantityOnHand < MinimumStockLevel.Value;

    /// <summary>Whether stock is at zero or negative.</summary>
    public bool IsOutOfStock => QuantityOnHand <= 0;

    /// <summary>Location (if location-specific).</summary>
    public string? Location { get; set; }

    /// <summary>Last movement date for this product.</summary>
    public DateTime? LastMovementDate { get; set; }
}

/// <summary>
/// Stock on hand by batch (for traceability).
/// </summary>
public class BatchStockOnHandDto
{
    /// <summary>Product ID.</summary>
    public int ProductId { get; set; }

    /// <summary>Product SKU.</summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Batch number.</summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Quantity on hand for this batch.</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Weight in grams for this batch.</summary>
    public decimal WeightGrams { get; set; }

    /// <summary>Batch expiry date (if applicable).</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Is batch expired?</summary>
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value < DateTime.UtcNow;

    /// <summary>Days until expiry (null if no expiry).</summary>
    public int? DaysUntilExpiry => ExpiryDate.HasValue
        ? (int)(ExpiryDate.Value - DateTime.UtcNow).TotalDays
        : null;

    /// <summary>THC percentage (cannabis compliance).</summary>
    public string? THCPercentage { get; set; }

    /// <summary>CBD percentage (cannabis compliance).</summary>
    public string? CBDPercentage { get; set; }
}

/// <summary>
/// Stock level alert for low stock notifications.
/// </summary>
public class StockAlertDto
{
    /// <summary>Product ID.</summary>
    public int ProductId { get; set; }

    /// <summary>Product SKU.</summary>
    public string ProductSKU { get; set; } = string.Empty;

    /// <summary>Product name.</summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>Current quantity on hand.</summary>
    public decimal CurrentQuantity { get; set; }

    /// <summary>Minimum stock level.</summary>
    public decimal MinimumStockLevel { get; set; }

    /// <summary>Recommended reorder quantity.</summary>
    public decimal ReorderQuantity { get; set; }

    /// <summary>Alert type (LowStock, OutOfStock, Expiring, Expired).</summary>
    public StockAlertType AlertType { get; set; }

    /// <summary>Alert severity (Info, Warning, Critical).</summary>
    public AlertSeverity Severity { get; set; }

    /// <summary>Alert message.</summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>Location (if applicable).</summary>
    public string? Location { get; set; }

    /// <summary>Alert created date.</summary>
    public DateTime AlertDate { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Inventory valuation summary.
/// </summary>
public class InventoryValuationDto
{
    /// <summary>Total number of products with stock.</summary>
    public int TotalProducts { get; set; }

    /// <summary>Total quantity across all products.</summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>Total weight in grams (for cannabis products).</summary>
    public decimal TotalWeightGrams { get; set; }

    /// <summary>Total inventory value at cost.</summary>
    public decimal TotalValueAtCost { get; set; }

    /// <summary>Total inventory value at retail.</summary>
    public decimal TotalValueAtRetail { get; set; }

    /// <summary>Potential gross margin.</summary>
    public decimal PotentialGrossMargin => TotalValueAtRetail - TotalValueAtCost;

    /// <summary>Gross margin percentage.</summary>
    public decimal GrossMarginPercentage => TotalValueAtRetail > 0
        ? (PotentialGrossMargin / TotalValueAtRetail) * 100
        : 0;

    /// <summary>Valuation date.</summary>
    public DateTime ValuationDate { get; set; } = DateTime.UtcNow;

    /// <summary>Products at or below minimum stock.</summary>
    public int LowStockProductCount { get; set; }

    /// <summary>Products with zero stock.</summary>
    public int OutOfStockProductCount { get; set; }

    /// <summary>Breakdown by category (if available).</summary>
    public List<CategoryValuationDto> CategoryBreakdown { get; set; } = new();
}

/// <summary>
/// Valuation by product category.
/// </summary>
public class CategoryValuationDto
{
    /// <summary>Category name.</summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>Number of products in category.</summary>
    public int ProductCount { get; set; }

    /// <summary>Total quantity.</summary>
    public decimal TotalQuantity { get; set; }

    /// <summary>Total value at cost.</summary>
    public decimal TotalValueAtCost { get; set; }

    /// <summary>Percentage of total inventory value.</summary>
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// Stock movement history for a product.
/// </summary>
public class StockMovementHistoryDto
{
    /// <summary>Movement ID.</summary>
    public int MovementId { get; set; }

    /// <summary>Movement date.</summary>
    public DateTime MovementDate { get; set; }

    /// <summary>Movement type.</summary>
    public string MovementType { get; set; } = string.Empty;

    /// <summary>Direction (IN or OUT).</summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>Quantity moved.</summary>
    public decimal Quantity { get; set; }

    /// <summary>Running balance after this movement.</summary>
    public decimal RunningBalance { get; set; }

    /// <summary>Batch number.</summary>
    public string? BatchNumber { get; set; }

    /// <summary>Reference number (transaction, PO, etc.).</summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>Movement reason.</summary>
    public string? Reason { get; set; }

    /// <summary>User who created the movement.</summary>
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Request for setting stock levels.
/// </summary>
public class SetStockLevelsDto
{
    /// <summary>Product ID.</summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>Minimum stock level (reorder point).</summary>
    [Range(0, 1000000)]
    public decimal MinimumStockLevel { get; set; }

    /// <summary>Maximum stock level.</summary>
    [Range(0, 1000000)]
    public decimal? MaximumStockLevel { get; set; }

    /// <summary>Reorder quantity (how much to order when below minimum).</summary>
    [Range(0, 1000000)]
    public decimal ReorderQuantity { get; set; }

    /// <summary>Location (if location-specific levels).</summary>
    public string? Location { get; set; }
}

// ============================================================
// ENUMERATIONS
// ============================================================

/// <summary>
/// Type of stock alert.
/// </summary>
public enum StockAlertType
{
    /// <summary>Stock is below minimum level.</summary>
    LowStock = 1,

    /// <summary>Stock is at zero.</summary>
    OutOfStock = 2,

    /// <summary>Batch is expiring soon.</summary>
    ExpiringSoon = 3,

    /// <summary>Batch has expired.</summary>
    Expired = 4,

    /// <summary>Stock is above maximum level.</summary>
    Overstock = 5
}

/// <summary>
/// Alert severity level.
/// </summary>
public enum AlertSeverity
{
    /// <summary>Informational alert.</summary>
    Info = 1,

    /// <summary>Warning - action recommended.</summary>
    Warning = 2,

    /// <summary>Critical - immediate action required.</summary>
    Critical = 3
}
