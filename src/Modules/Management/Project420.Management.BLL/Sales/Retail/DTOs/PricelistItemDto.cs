namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for pricelist item display/reading.
/// Represents a product's price within a specific pricelist.
/// </summary>
public class PricelistItemDto
{
    // ============================================================
    // IDENTITY
    // ============================================================

    /// <summary>
    /// Pricelist item unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Foreign key to Pricelist.
    /// </summary>
    public int PricelistId { get; set; }

    /// <summary>
    /// Foreign key to Product.
    /// </summary>
    public int ProductId { get; set; }

    // ============================================================
    // PRODUCT INFORMATION (Denormalized for display)
    // ============================================================

    /// <summary>
    /// Product SKU (Stock Keeping Unit).
    /// </summary>
    public string? ProductSKU { get; set; }

    /// <summary>
    /// Product name.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Product's base/default price (for comparison).
    /// </summary>
    public decimal? ProductBasePrice { get; set; }

    // ============================================================
    /// PRICING
    // ============================================================

    /// <summary>
    /// Price for this product in this pricelist (includes VAT).
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Price difference from base price.
    /// Positive = price increase, Negative = discount.
    /// </summary>
    public decimal? PriceDifference => ProductBasePrice.HasValue
        ? Price - ProductBasePrice.Value
        : null;

    /// <summary>
    /// Percentage difference from base price.
    /// Positive = markup %, Negative = discount %.
    /// </summary>
    public decimal? PercentageDifference => ProductBasePrice.HasValue && ProductBasePrice.Value > 0
        ? Math.Round(((Price - ProductBasePrice.Value) / ProductBasePrice.Value) * 100, 2)
        : null;

    // ============================================================
    // QUANTITY-BASED PRICING (Tiered Pricing)
    // ============================================================

    /// <summary>
    /// Minimum quantity required to get this price.
    /// Default: 1 (no minimum).
    /// </summary>
    public int MinimumQuantity { get; set; }

    /// <summary>
    /// Maximum quantity this price applies to.
    /// Null = no maximum (unlimited).
    /// </summary>
    public int? MaximumQuantity { get; set; }

    // ============================================================
    // AUDIT INFORMATION
    // ============================================================

    /// <summary>
    /// When this pricelist item was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this pricelist item.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When this pricelist item was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this pricelist item.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
