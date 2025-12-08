namespace Project420.Management.BLL.Sales.SalesCommon.DTOs;

/// <summary>
/// Data Transfer Object for updating an existing customer (debtor) category.
/// Contains the ID of the category to update plus the fields that can be changed.
/// </summary>
public class UpdateDebtorCategoryDto
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
    /// Updated debtor category code.
    /// Required, should be short and unique.
    /// </summary>
    public string DebtorCode { get; set; } = string.Empty;

    /// <summary>
    /// Whether this category has special compliance rules.
    /// Cannabis Act: Medical patients have different requirements.
    /// POPIA: Enhanced data protection may be required.
    /// </summary>
    public bool SpecialRules { get; set; }

    /// <summary>
    /// Whether this category is active.
    /// Setting to false will deactivate the category without deleting it.
    /// </summary>
    public bool IsActive { get; set; }
}
