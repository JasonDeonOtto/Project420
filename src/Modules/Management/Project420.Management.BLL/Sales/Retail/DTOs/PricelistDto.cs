namespace Project420.Management.BLL.Sales.Retail.DTOs;

/// <summary>
/// Data Transfer Object for pricelist display/reading.
/// Used to transfer pricelist data from business logic layer to UI.
/// </summary>
public class PricelistDto
{
    // ============================================================
    // IDENTITY
    // ============================================================

    /// <summary>
    /// Pricelist unique identifier.
    /// </summary>
    public int Id { get; set; }

    // ============================================================
    // BASIC INFORMATION
    // ============================================================

    /// <summary>
    /// Pricelist name/title.
    /// Examples: "Standard Retail", "VIP Pricing", "Medical Patient Rates".
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this pricelist purpose and rules.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Unique code for this pricelist (optional).
    /// Example: "PL-STD-001", "PL-VIP-001".
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
    /// Only ONE pricelist should be default at a time.
    /// </summary>
    public bool IsDefault { get; set; }

    // ============================================================
    // DATE RANGE (Time-based pricing)
    // ============================================================

    /// <summary>
    /// Date when this pricelist becomes effective (optional).
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>
    /// Date when this pricelist expires (optional).
    /// </summary>
    public DateTime? EffectiveTo { get; set; }

    /// <summary>
    /// Whether this pricelist is currently within its effective date range.
    /// </summary>
    public bool IsCurrentlyEffective
    {
        get
        {
            var now = DateTime.Today;
            var afterStart = !EffectiveFrom.HasValue || EffectiveFrom.Value.Date <= now;
            var beforeEnd = !EffectiveTo.HasValue || EffectiveTo.Value.Date >= now;
            return afterStart && beforeEnd;
        }
    }

    /// <summary>
    /// Whether this pricelist has expired.
    /// </summary>
    public bool IsExpired => EffectiveTo.HasValue && EffectiveTo.Value.Date < DateTime.Today;

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
    /// Positive = markup, Negative = discount.
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

    // ============================================================
    // STATISTICS
    // ============================================================

    /// <summary>
    /// Number of products in this pricelist.
    /// </summary>
    public int ProductCount { get; set; }

    // ============================================================
    // AUDIT INFORMATION
    // ============================================================

    /// <summary>
    /// When this pricelist was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this pricelist.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When this pricelist was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this pricelist.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
