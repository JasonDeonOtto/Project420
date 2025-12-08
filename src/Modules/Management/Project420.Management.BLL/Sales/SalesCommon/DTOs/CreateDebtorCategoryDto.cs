namespace Project420.Management.BLL.Sales.SalesCommon.DTOs;

/// <summary>
/// Data Transfer Object for creating a new customer (debtor) category.
/// Contains only the fields needed to create a category.
/// </summary>
public class CreateDebtorCategoryDto
{
    /// <summary>
    /// Category name (e.g., "Retail Customer", "VIP Customer", "Medical Patient", "Wholesale Client").
    /// Required, max 50 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Debtor category code for quick reference and reporting.
    /// Required, should be short and unique (e.g., "RETAIL", "VIP", "MEDICAL", "WHOLESALE").
    /// </summary>
    public string DebtorCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this category has special compliance rules.
    /// Cannabis Compliance: Medical patients require Section 21 permits.
    /// POPIA Compliance: Enhanced data protection for medical records.
    /// Default: false
    /// </summary>
    public bool SpecialRules { get; set; } = false;

    /// <summary>
    /// Whether this category should be active immediately upon creation.
    /// Default: true
    /// </summary>
    public bool IsActive { get; set; } = true;
}
