namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for updating a pricelist item.
/// Used to transfer pricelist item update data from UI to business logic layer.
/// </summary>
public class UpdatePricelistItemDto
{
    // ============================================================
    // IDENTITY (Required)
    // ============================================================

    /// <summary>
    /// Pricelist item ID to update (Required).
    /// </summary>
    public int Id { get; set; }

    // ============================================================
    // PRICING
    // ============================================================

    /// <summary>
    /// Price for this product in this pricelist (includes VAT 15%).
    /// </summary>
    public decimal Price { get; set; }

    // ============================================================
    // QUANTITY-BASED PRICING
    // ============================================================

    /// <summary>
    /// Minimum quantity required to get this price.
    /// </summary>
    public int MinimumQuantity { get; set; }

    /// <summary>
    /// Maximum quantity this price applies to.
    /// Null = no maximum (unlimited).
    /// </summary>
    public int? MaximumQuantity { get; set; }
}
