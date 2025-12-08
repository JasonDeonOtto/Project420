namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for creating a new pricelist.
/// Used to transfer pricelist creation data from UI to business logic layer.
/// </summary>
public class CreatePricelistDto
{
    // ============================================================
    // BASIC INFORMATION (Required)
    // ============================================================

    /// <summary>
    /// Pricelist name/title (Required).
    /// Examples: "Standard Retail", "VIP Pricing", "Medical Patient Rates", "December 2024 Special".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this pricelist purpose and rules (Optional).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique code for this pricelist (Optional).
    /// Example: "PL-STD-001", "PL-VIP-001", "PL-MED-001".
    /// </summary>
    public string? Code { get; set; }

    // ============================================================
    // STATUS AND ACTIVATION
    // ============================================================

    /// <summary>
    /// Whether this pricelist is currently active and available for use.
    /// Defaults to true.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default pricelist for new customers.
    /// Defaults to false.
    /// IMPORTANT: Only ONE pricelist should be default at a time.
    /// </summary>
    public bool IsDefault { get; set; } = false;

    // ============================================================
    // DATE RANGE (Optional - for time-based pricing)
    // ============================================================

    /// <summary>
    /// Date when this pricelist becomes effective (Optional).
    /// Use for: future price increases, promotional start dates, seasonal pricing.
    /// Null = effective immediately.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Date when this pricelist expires (Optional).
    /// Use for: limited-time promotions, seasonal pricing end dates.
    /// Null = no expiry (permanent).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    // ============================================================
    // PRICING STRATEGY
    // ============================================================

    /// <summary>
    /// Pricing strategy type.
    /// Values: "Fixed" (specific prices per product), "Percentage" (discount/markup %), "Tiered" (quantity-based).
    /// Defaults to "Fixed".
    /// </summary>
    public string PricingStrategy { get; set; } = "Fixed";

    /// <summary>
    /// Discount or markup percentage (if using percentage strategy).
    /// Positive = markup (increase price by %), Negative = discount (decrease price by %).
    /// Example: -10.00 = 10% discount, +15.00 = 15% markup.
    /// Only used if PricingStrategy = "Percentage".
    /// </summary>
    public decimal? PercentageAdjustment { get; set; }

    // ============================================================
    // PRIORITY
    // ============================================================

    /// <summary>
    /// Priority level when multiple pricelists could apply.
    /// Lower number = higher priority.
    /// Defaults to 100.
    /// Range: 1-999.
    /// </summary>
    public int Priority { get; set; } = 100;
}
