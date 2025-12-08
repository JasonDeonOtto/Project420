namespace Project420.Management.BLL.Sales.SalesCommon.DTOs;

/// <summary>
/// Data Transfer Object for displaying debtor (customer) category information.
/// Used to transfer category data from business logic layer to UI.
/// </summary>
public class DebtorCategoryDto
{
    /// <summary>
    /// Category unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name (e.g., "Retail Customer", "VIP Customer", "Medical Patient", "Wholesale Client").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Debtor category code for quick reference and reporting.
    /// </summary>
    public string DebtorCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this category has special compliance rules.
    /// Cannabis Compliance: Medical patients have Section 21 permit requirements.
    /// POPIA Compliance: Different data protection rules may apply.
    /// </summary>
    public bool SpecialRules { get; set; }

    /// <summary>
    /// Whether this category is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of customers (debtors) in this category.
    /// Useful for displaying category statistics and segmentation reports.
    /// </summary>
    public int CustomerCount { get; set; }

    /// <summary>
    /// When this category was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this category.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// When this category was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this category.
    /// </summary>
    public string? UpdatedBy { get; set; }
}
