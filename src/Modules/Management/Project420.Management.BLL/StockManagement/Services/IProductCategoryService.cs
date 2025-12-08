using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.Models.Entities.ProductManagement;

namespace Project420.Management.BLL.StockManagement.Services;

/// <summary>
/// Interface for product category business logic operations.
/// Defines contract for category management, validation, and queries.
/// </summary>
public interface IProductCategoryService
{
    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new product category with validation.
    /// </summary>
    /// <param name="dto">Category creation data</param>
    /// <returns>Created category entity</returns>
    Task<ProductCategory> CreateCategoryAsync(CreateProductCategoryDto dto);

    /// <summary>
    /// Updates an existing product category with validation.
    /// </summary>
    /// <param name="dto">Category update data</param>
    /// <returns>Updated category entity</returns>
    Task<ProductCategory> UpdateCategoryAsync(UpdateProductCategoryDto dto);

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    /// <param name="id">Category ID</param>
    /// <returns>Category DTO or null if not found</returns>
    Task<ProductCategoryDto?> GetCategoryByIdAsync(int id);

    /// <summary>
    /// Gets all active categories.
    /// </summary>
    /// <returns>List of category DTOs</returns>
    Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync();

    /// <summary>
    /// Gets paginated categories.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Paginated list of category DTOs</returns>
    Task<IEnumerable<ProductCategoryDto>> GetPagedCategoriesAsync(int pageNumber, int pageSize);

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
    Task<IEnumerable<ProductCategoryDto>> SearchCategoriesAsync(string searchTerm);

    /// <summary>
    /// Gets a category by its code.
    /// </summary>
    /// <param name="code">Category code</param>
    /// <returns>Category DTO or null if not found</returns>
    Task<ProductCategoryDto?> GetCategoryByCodeAsync(string code);

    /// <summary>
    /// Gets all active categories.
    /// </summary>
    /// <returns>Active categories only</returns>
    Task<IEnumerable<ProductCategoryDto>> GetActiveCategoriesAsync();

    /// <summary>
    /// Gets all categories with special rules.
    /// </summary>
    /// <returns>Categories with special compliance rules</returns>
    Task<IEnumerable<ProductCategoryDto>> GetCategoriesWithSpecialRulesAsync();

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
