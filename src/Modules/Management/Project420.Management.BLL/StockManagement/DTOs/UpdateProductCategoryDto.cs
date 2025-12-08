namespace Project420.Management.BLL.StockManagement.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing product category.
/// Contains the ID of the category to update plus the fields that can be changed.
/// </summary>
public class UpdateProductCategoryDto
{
    /// <summary>
    /// Category ID to update.
    /// Required.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Updated category name.
    /// Required, max 50 characters.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Updated category code.
    /// Required, should be short and unique.
    /// </summary>
    public string CategoryCode { get; set; } = string.Empty;

    /// <summary>
    /// Whether this category has special compliance rules.
    /// </summary>
    public bool SpecialRules { get; set; }

    /// <summary>
    /// Whether this category is active.
    /// Setting to false will deactivate the category without deleting it.
    /// </summary>
    public bool IsActive { get; set; }
}
