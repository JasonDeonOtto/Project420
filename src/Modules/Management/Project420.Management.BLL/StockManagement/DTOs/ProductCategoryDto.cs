namespace Project420.Management.BLL.StockManagement.DTOs;

/// <summary>
/// Data Transfer Object for displaying product category information.
/// Used to transfer category data from business logic layer to UI.
/// </summary>
public class ProductCategoryDto
{
    /// <summary>
    /// Category unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Category name (e.g., "Flower", "Edibles", "Accessories").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category code for quick reference and reporting.
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this category has special compliance rules.
    /// Cannabis Compliance: Some categories may have additional restrictions.
    /// </summary>
    public bool SpecialRules { get; set; }

    /// <summary>
    /// Whether this category is currently active and available for use.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Number of products in this category.
    /// Useful for displaying category statistics.
    /// </summary>
    public int ProductCount { get; set; }

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
