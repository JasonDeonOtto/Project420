namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing pricelist.
/// Used to transfer pricelist update data from UI to business logic layer.
/// </summary>
public class UpdatePricelistDto
{
    // ============================================================
    // IDENTITY (Required)
    // ============================================================

    /// <summary>
    /// Pricelist ID to update (Required).
    /// </summary>
    public int Id { get; set; }

    // ============================================================
    // BASIC INFORMATION
    // ============================================================

    /// <summary>
    /// Pricelist name/title.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this pricelist purpose and rules.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique code for this pricelist.
    /// </summary>
    public string? Code { get; set; }

    // ============================================================
    // STATUS AND ACTIVATION
    // ============================================================

    /// <summary>
    /// Whether this pricelist is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Whether this is the default pricelist for new customers.
    /// IMPORTANT: Service will ensure only ONE pricelist is default.
    /// </summary>
    public bool IsDefault { get; set; }

    // ============================================================
    // DATE RANGE
    // ============================================================

    /// <summary>
    /// Date when this pricelist becomes effective.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Date when this pricelist expires.
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    // ============================================================
    // PRICING STRATEGY
    // ============================================================

    /// <summary>
    /// Pricing strategy type.
    /// Values: "Fixed", "Percentage", "Tiered".
    /// </summary>
    public string PricingStrategy { get; set; } = "Fixed";

    /// <summary>
    /// Discount or markup percentage (if using percentage strategy).
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }

    // ============================================================
    // PRIORITY
    // ============================================================

    /// <summary>
    /// Priority level when multiple pricelists could apply.
    /// Lower number = higher priority.
    /// </summary>
    public int Priority { get; set; }
}
