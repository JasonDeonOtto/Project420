namespace Project420.Management.BLL.StockManagement.DTOs;

/// <summary>
/// Data Transfer Object for creating a new product.
/// Used to transfer product creation data from UI to business logic layer.
/// </summary>
public class CreateProductDto
{
    // ============================================================
    // BASIC INFORMATION (Required)
    // ============================================================

    /// <summary>
    /// Stock Keeping Unit - Unique product identifier/barcode (Required).
    /// Format examples: "CBD-OIL-001", "FLOWER-IND-002", "EDIBLE-GUMMY-003".
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Product name/title (Required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed product description (Optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this product is currently active and available for sale.
    /// Defaults to true.
    /// </summary>
    public bool IsActive { get; set; } = true;

    // ============================================================
    // CANNABIS COMPLIANCE FIELDS (SA Cannabis Act 2024)
    // ============================================================

    /// <summary>
    /// THC (Tetrahydrocannabinol) percentage or content.
    /// Cannabis Compliance: REQUIRED on all product labels.
    /// Format examples: "15%", "150mg", "15-18%".
    /// </summary>
    public string? THCPercentage { get; set; }

    /// <summary>
    /// CBD (Cannabidiol) percentage or content.
    /// Cannabis Compliance: REQUIRED on all product labels.
    /// Format examples: "0.5%", "50mg", "1-2%".
    /// </summary>
    public string? CBDPercentage { get; set; }

    /// <summary>
    /// Batch or lot number for seed-to-sale traceability.
    /// Cannabis Compliance: REQUIRED for SAHPRA reporting.
    /// Example: "BATCH-2024-11-001".
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Cannabis strain name (e.g., "Blue Dream", "OG Kush", "Charlotte's Web").
    /// </summary>
    public string? StrainName { get; set; }

    /// <summary>
    /// Date when lab testing was performed on this batch.
    /// Cannabis Compliance: Certificate of Analysis (COA) required.
    /// </summary>
    public DateTime? LabTestDate { get; set; }

    /// <summary>
    /// Product expiry or best-before date.
    /// Important for edibles and cannabis oils.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    // ============================================================
    // PRICING (Required)
    // ============================================================

    /// <summary>
    /// Default selling price (includes VAT 15%) (Required).
    /// Must be between R0.01 and R999,999.99.
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cost price - what you paid for this product (excluding VAT) (Required).
    /// Must be between R0.00 and R999,999.99.
    /// </summary>
    public decimal CostPrice { get; set; }

    // ============================================================
    // INVENTORY/STOCK (Required)
    // ============================================================

    /// <summary>
    /// Initial quantity in stock (on hand).
    /// Defaults to 0.
    /// </summary>
    public int StockOnHand { get; set; } = 0;

    /// <summary>
    /// Minimum stock level before reorder alert triggers.
    /// Defaults to 0.
    /// </summary>
    public int ReorderLevel { get; set; } = 0;

    // ============================================================
    // CATEGORY (Future Enhancement)
    // ============================================================

    /// <summary>
    /// Foreign key to Product Category (if/when implemented).
    /// Examples: "Flower", "Edibles", "Oils", "Concentrates", "Accessories".
    /// </summary>
    public int? CategoryId { get; set; }
}
