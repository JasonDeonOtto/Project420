using FluentValidation;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.DAL.Repositories.ProductManagement;
using Project420.Management.DAL.Repositories.StockManagement;
using Project420.Management.Models.Entities.ProductManagement;

namespace Project420.Management.BLL.StockManagement.Services;

/// <summary>
/// Service for product category business logic.
/// Handles category management, validation, and organization.
/// </summary>
public class ProductCategoryService : IProductCategoryService
{
    private readonly IProductCategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductCategoryDto> _createValidator;
    private readonly IValidator<UpdateProductCategoryDto> _updateValidator;

    public ProductCategoryService(
        IProductCategoryRepository categoryRepository,
        IProductRepository productRepository,
        IValidator<CreateProductCategoryDto> createValidator,
        IValidator<UpdateProductCategoryDto> updateValidator)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new product category with full validation.
    /// </summary>
    public async Task<ProductCategory> CreateCategoryAsync(CreateProductCategoryDto dto)
    {
        // Normalize category code to uppercase before validation
        dto.CategoryCode = dto.CategoryCode?.ToUpperInvariant() ?? string.Empty;

        // STEP 1: Validate input data
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check for duplicate name
        if (!await IsCategoryNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists");
        }

        // STEP 3: Check for duplicate code
        if (!await IsCategoryCodeUniqueAsync(dto.CategoryCode))
        {
            throw new InvalidOperationException($"Category with code '{dto.CategoryCode}' already exists");
        }

        // STEP 4: Map DTO to Entity
        var category = new ProductCategory
        {
            Name = dto.Name,
            CategoryCode = dto.CategoryCode, // Already uppercased before validation
            SpecialRules = dto.SpecialRules,
            IsActive = dto.IsActive
        };

        // STEP 5: Save to database
        var createdCategory = await _categoryRepository.AddAsync(category);

        return createdCategory;
    }

    /// <summary>
    /// Updates an existing product category with validation.
    /// </summary>
    public async Task<ProductCategory> UpdateCategoryAsync(UpdateProductCategoryDto dto)
    {
        // Normalize category code to uppercase before validation
        dto.CategoryCode = dto.CategoryCode?.ToUpperInvariant() ?? string.Empty;

        // STEP 1: Validate input data
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check if category exists
        var existingCategory = await _categoryRepository.GetByIdAsync(dto.Id);
        if (existingCategory == null)
        {
            throw new InvalidOperationException($"Category with ID {dto.Id} not found");
        }

        // STEP 3: Check for duplicate name (excluding current category)
        if (!await IsCategoryNameUniqueAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Category with name '{dto.Name}' already exists");
        }

        // STEP 4: Check for duplicate code (excluding current category)
        if (!await IsCategoryCodeUniqueAsync(dto.CategoryCode, dto.Id))
        {
            throw new InvalidOperationException($"Category with code '{dto.CategoryCode}' already exists");
        }

        // STEP 5: Update entity properties
        existingCategory.Name = dto.Name;
        existingCategory.CategoryCode = dto.CategoryCode; // Already uppercased before validation
        existingCategory.SpecialRules = dto.SpecialRules;
        existingCategory.IsActive = dto.IsActive;

        // STEP 6: Save changes
        await _categoryRepository.UpdateAsync(existingCategory);

        return existingCategory;
    }

    /// <summary>
    /// Gets a category by ID with product count.
    /// </summary>
    public async Task<ProductCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? await MapToDtoAsync(category) : null;
    }

    /// <summary>
    /// Gets all categories with product counts.
    /// </summary>
    public async Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = new List<ProductCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets paginated categories.
    /// </summary>
    public async Task<IEnumerable<ProductCategoryDto>> GetPagedCategoriesAsync(int pageNumber, int pageSize)
    {
        var categories = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);
        var dtos = new List<ProductCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Deactivates a category (soft delete).
    /// Note: Products in this category are not affected.
    /// </summary>
    public async Task DeactivateCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException($"Category with ID {id} not found");
        }

        category.IsActive = false;
        await _categoryRepository.UpdateAsync(category);
    }

    // ============================================================
    // SEARCH & FILTER OPERATIONS
    // ============================================================

    /// <summary>
    /// Searches for categories by name or code.
    /// </summary>
    public async Task<IEnumerable<ProductCategoryDto>> SearchCategoriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ProductCategoryDto>();

        var normalizedSearch = searchTerm.ToUpperInvariant();

        var categories = await _categoryRepository.FindAsync(c =>
            c.Name!.ToUpper().Contains(normalizedSearch) ||
            c.CategoryCode.ToUpper().Contains(normalizedSearch));

        var dtos = new List<ProductCategoryDto>();
        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets a category by its unique code.
    /// </summary>
    public async Task<ProductCategoryDto?> GetCategoryByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalizedCode = code.ToUpperInvariant();
        var categories = await _categoryRepository.FindAsync(c =>
            c.CategoryCode.ToUpper() == normalizedCode);

        var category = categories.FirstOrDefault();
        return category != null ? await MapToDtoAsync(category) : null;
    }

    /// <summary>
    /// Gets all active categories only.
    /// </summary>
    public async Task<IEnumerable<ProductCategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.FindAsync(c => c.IsActive);
        var dtos = new List<ProductCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets all categories with special compliance rules.
    /// Cannabis Compliance: Some categories may require additional checks or restrictions.
    /// </summary>
    public async Task<IEnumerable<ProductCategoryDto>> GetCategoriesWithSpecialRulesAsync()
    {
        var categories = await _categoryRepository.FindAsync(c => c.SpecialRules);
        var dtos = new List<ProductCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    // ============================================================
    // VALIDATION OPERATIONS
    // ============================================================

    /// <summary>
    /// Checks if a category name is unique.
    /// </summary>
    public async Task<bool> IsCategoryNameUniqueAsync(string name, int? excludeCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.ToUpperInvariant();

        var categories = await _categoryRepository.FindAsync(c =>
            c.Name!.ToUpper() == normalizedName &&
            (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));

        return !categories.Any();
    }

    /// <summary>
    /// Checks if a category code is unique.
    /// </summary>
    public async Task<bool> IsCategoryCodeUniqueAsync(string code, int? excludeCategoryId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return false;

        var normalizedCode = code.ToUpperInvariant();

        var categories = await _categoryRepository.FindAsync(c =>
            c.CategoryCode.ToUpper() == normalizedCode &&
            (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));

        return !categories.Any();
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Maps a ProductCategory entity to a ProductCategoryDto.
    /// Includes product count for the category.
    /// </summary>
    private async Task<ProductCategoryDto> MapToDtoAsync(ProductCategory category)
    {
        // Get product count for this category
        var products = await _productRepository.FindAsync(p => p.CategoryId == category.Id);
        var productCount = products.Count();

        return new ProductCategoryDto
        {
            Id = category.Id,
            Name = category.Name ?? string.Empty,
            CategoryCode = category.CategoryCode,
            SpecialRules = category.SpecialRules,
            IsActive = category.IsActive,
            ProductCount = productCount,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.ModifiedAt,
            UpdatedBy = category.ModifiedBy
        };
    }
}
