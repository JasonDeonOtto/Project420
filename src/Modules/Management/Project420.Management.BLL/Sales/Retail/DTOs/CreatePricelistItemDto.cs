namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for adding a product to a pricelist.
/// Used to transfer pricelist item creation data from UI to business logic layer.
/// </summary>
public class CreatePricelistItemDto
{
    // ============================================================
    // RELATIONSHIPS (Required)
    // ============================================================

    /// <summary>
    /// Pricelist ID to add product to (Required).
    /// </summary>
    public int PricelistId { get; set; }

    /// <summary>
    /// Product ID to add to pricelist (Required).
    /// </summary>
    public int ProductId { get; set; }

    // ============================================================
    // PRICING (Required)
    // ============================================================

    /// <summary>
    /// Price for this product in this pricelist (includes VAT 15%) (Required).
    /// Must be between R0.01 and R999,999.99.
    /// </summary>
    public decimal Price { get; set; }

    // ============================================================
    // QUANTITY-BASED PRICING (Optional - for tiered pricing)
    // ============================================================

    /// <summary>
    /// Minimum quantity required to get this price.
    /// Default: 1 (no minimum).
    /// Example: "Buy 10+ to get this price".
    /// </summary>
    public int MinimumQuantity { get; set; } = 1;

    /// <summary>
    /// Maximum quantity this price applies to.
    /// Null = no maximum (unlimited).
    /// Example: "Qty 1-9: R150, Qty 10-49: R135".
    /// </summary>
    public int? MaximumQuantity { get; set; }
}
