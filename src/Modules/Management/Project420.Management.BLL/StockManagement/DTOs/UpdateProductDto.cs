namespace Project420.Management.BLL.StockManagement.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing product.
/// Used to transfer product update data from UI to business logic layer.
/// </summary>
public class UpdateProductDto
{
    // ============================================================
    // IDENTITY (Required)
    // ============================================================

    /// <summary>
    /// Product ID to update (Required).
    /// </summary>
    public int Id { get; set; }

    // ============================================================
    // BASIC INFORMATION
    // ============================================================

    /// <summary>
    /// Stock Keeping Unit - Unique product identifier/barcode.
    /// </summary>
    public string SKU { get; set; } = string.Empty;

    /// <summary>
    /// Product name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed product description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this product is currently active and available for sale.
    /// </summary>
    public bool IsActive { get; set; }

    // ============================================================
    // CANNABIS COMPLIANCE FIELDS (SA Cannabis Act 2024)
    // ============================================================

    /// <summary>
    /// THC (Tetrahydrocannabinol) percentage or content.
    /// Cannabis Compliance: REQUIRED on all product labels.
    /// </summary>
    public string? THCPercentage { get; set; }

    /// <summary>
    /// CBD (Cannabidiol) percentage or content.
    /// Cannabis Compliance: REQUIRED on all product labels.
    /// </summary>
    public string? CBDPercentage { get; set; }

    /// <summary>
    /// Batch or lot number for seed-to-sale traceability.
    /// Cannabis Compliance: REQUIRED for SAHPRA reporting.
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Cannabis strain name (e.g., "Blue Dream", "OG Kush").
    /// </summary>
    public string? StrainName { get; set; }

    /// <summary>
    /// Date when lab testing was performed on this batch.
    /// Cannabis Compliance: Certificate of Analysis (COA) required.
    /// </summary>
    public DateTime? LabTestDate { get; set; }

    /// <summary>
    /// Product expiry or best-before date.
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    // ============================================================
    // PRICING
    // ============================================================

    /// <summary>
    /// Default selling price (includes VAT 15%).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Cost price - what you paid for this product (excluding VAT).
    /// </summary>
    public decimal CostPrice { get; set; }

    // ============================================================
    // INVENTORY/STOCK
    // ============================================================

    /// <summary>
    /// Current quantity in stock (on hand).
    /// NOTE: Use stock adjustment methods in service for proper tracking.
    /// </summary>
    public int StockOnHand { get; set; }

    /// <summary>
    /// Minimum stock level before reorder alert triggers.
    /// </summary>
    public int ReorderLevel { get; set; }

    // ============================================================
    // CATEGORY (Future Enhancement)
    // ============================================================

    /// <summary>
    /// Foreign key to Product Category (if/when implemented).
    /// </summary>
    public int? CategoryId { get; set; }
}
