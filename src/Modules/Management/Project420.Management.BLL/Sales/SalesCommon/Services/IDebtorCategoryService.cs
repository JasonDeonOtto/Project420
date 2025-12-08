using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.BLL.Sales.SalesCommon.Services;

/// <summary>
/// Interface for customer (debtor) category business logic operations.
/// Defines contract for category management, customer segmentation, and compliance validation.
/// </summary>
public interface IDebtorCategoryService
{
    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new customer category with validation.
    /// </summary>
    /// <param name="dto">Category creation data</param>
    /// <returns>Created category entity</returns>
    Task<DebtorCategory> CreateCategoryAsync(CreateDebtorCategoryDto dto);

    /// <summary>
    /// Updates an existing customer category with validation.
    /// </summary>
    /// <param name="dto">Category update data</param>
    /// <returns>Updated category entity</returns>
    Task<DebtorCategory> UpdateCategoryAsync(UpdateDebtorCategoryDto dto);

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category DTO or null if not found</returns>
    Task<DebtorCategoryDto?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Gets all customer categories.
    /// </summary>
    /// <returns>List of category DTOs</returns>
    Task<IEnumerable<DebtorCategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets paginated categories.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of category DTOs</returns>
    Task<IEnumerable<DebtorCategoryDto>> GetPagedCategoriesAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Deactivates a category (soft delete).
    /// </summary>
    /// <param name="id">Category ID to deactivate</param>
    Task DeactivateCategoryAsync(int id);

    // ============================================================
    // SEARCH & FILTER OPERATIONS
    // ============================================================

    /// <summary>
    /// Searches for categories by name or code.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Matching categories</returns>
    Task<IEnumerable<DebtorCategoryDto>> SearchCategoriesAsync(string searchTerm);

    /// <summary>
    /// Gets a category by its code.
    /// </summary>
    /// <param name="code">Category code</param>
    /// <returns>Category DTO or null if not found</returns>
    Task<DebtorCategoryDto?> GetCategoryByCodeAsync(string code);

    /// <summary>
    /// Gets all active categories.
    /// </summary>
    /// <returns>Active categories only</returns>
    Task<IEnumerable<DebtorCategoryDto>> GetActiveCategoriesAsync();

    /// <summary>
    /// Gets all categories with special compliance rules.
    /// Cannabis Act: Medical patients require Section 21 permits.
    /// POPIA: Enhanced data protection requirements.
    /// </summary>
    /// <returns>Categories with special compliance rules</returns>
    Task<IEnumerable<DebtorCategoryDto>> GetCategoriesWithSpecialRulesAsync();

    // ============================================================
    // VALIDATION OPERATIONS
    // ============================================================

    /// <summary>
    /// Checks if a category name is unique.
    /// </summary>
    /// <param name="name">Category name to check</param>
    /// <param name="excludeCategoryId">Category ID to exclude (for updates)</param>
    /// <returns>True if name is unique</returns>
    Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeCategoryId = null);

    /// <summary>
    /// Checks if a category code is unique.
    /// </summary>
    /// <param name="code">Category code to check</param>
    /// <param name="excludeCategoryId">Category ID to exclude (for updates)</param>
    /// <returns>True if code is unique</returns>
    Task<bool> IsCategoryCodeUniqueAsync(string code, int? excludeCategoryId = null);
}
