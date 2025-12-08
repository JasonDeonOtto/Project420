namespace Project420.Management.BLL.StockManagement.DTOs;

/// <summary>
/// Data Transfer Object for creating a new product category.
/// Contains only the fields needed to create a category.
/// </summary>
public class CreateProductCategoryDto
{
    /// <summary>
    /// Category name (e.g., "Flower", "Edibles", "Accessories").
    /// Required, max 50 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category code for quick reference and reporting.
    /// Required, should be short and unique (e.g., "FLWR", "EDBL", "ACCS").
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Indicates if this category has special compliance rules.
    /// Cannabis Compliance: Some categories may have additional age verification or storage requirements.
    /// Default: false
    /// </summary>
    public bool SpecialRules { get; set; } = false;

    /// <summary>
    /// Whether this category should be active immediately upon creation.
    /// Default: true
    /// </summary>
    public bool IsActive { get; set; } = true;
}
