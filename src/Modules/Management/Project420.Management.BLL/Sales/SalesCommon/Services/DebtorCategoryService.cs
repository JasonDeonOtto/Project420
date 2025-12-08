using FluentValidation;
using Project420.Management.BLL.Sales.SalesCommon.DTOs;
using Project420.Management.DAL.Repositories.Sales.SalesCommon;
using Project420.Management.Models.Entities.Sales.Common;

namespace Project420.Management.BLL.Sales.SalesCommon.Services;

/// <summary>
/// Service for customer (debtor) category business logic.
/// Handles category management, customer segmentation, and compliance validation.
/// Cannabis Act: Special rules for medical patients (Section 21 permits).
/// POPIA: Enhanced data protection for sensitive customer categories.
/// </summary>
public class DebtorCategoryService : IDebtorCategoryService
{
    private readonly IDebtorCategoryRepository _categoryRepository;
    private readonly IDebtorRepository _debtorRepository;
    private readonly IValidator<CreateDebtorCategoryDto> _createValidator;
    private readonly IValidator<UpdateDebtorCategoryDto> _updateValidator;

    public DebtorCategoryService(
        IDebtorCategoryRepository categoryRepository,
        IDebtorRepository debtorRepository,
        IValidator<CreateDebtorCategoryDto> createValidator,
        IValidator<UpdateDebtorCategoryDto> updateValidator)
    {
        _categoryRepository = categoryRepository;
        _debtorRepository = debtorRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new customer category with full validation.
    /// </summary>
    public async Task<DebtorCategory> CreateCategoryAsync(CreateDebtorCategoryDto dto)
    {
        // Normalize category code to uppercase before validation
        dto.DebtorCode = dto.DebtorCode?.ToUpperInvariant() ?? string.Empty;

        // STEP 1: Validate input data
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check for duplicate name
        if (!await IsCategoryNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Customer category with name '{dto.Name}' already exists");
        }

        // STEP 3: Check for duplicate code
        if (!await IsCategoryCodeUniqueAsync(dto.DebtorCode))
        {
            throw new InvalidOperationException($"Customer category with code '{dto.DebtorCode}' already exists");
        }

        // STEP 4: Map DTO to Entity
        var category = new DebtorCategory
        {
            Name = dto.Name,
            DebtorCode = dto.DebtorCode, // Already uppercased before validation
            SpecialRules = dto.SpecialRules,
            IsActive = dto.IsActive
        };

        // STEP 5: Save to database
        var createdCategory = await _categoryRepository.AddAsync(category);

        return createdCategory;
    }

    /// <summary>
    /// Updates an existing customer category with validation.
    /// </summary>
    public async Task<DebtorCategory> UpdateCategoryAsync(UpdateDebtorCategoryDto dto)
    {
        // Normalize category code to uppercase before validation
        dto.DebtorCode = dto.DebtorCode?.ToUpperInvariant() ?? string.Empty;

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
            throw new InvalidOperationException($"Customer category with ID {dto.Id} not found");
        }

        // STEP 3: Check for duplicate name (excluding current category)
        if (!await IsCategoryNameUniqueAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Customer category with name '{dto.Name}' already exists");
        }

        // STEP 4: Check for duplicate code (excluding current category)
        if (!await IsCategoryCodeUniqueAsync(dto.DebtorCode, dto.Id))
        {
            throw new InvalidOperationException($"Customer category with code '{dto.DebtorCode}' already exists");
        }

        // STEP 5: Update entity properties
        existingCategory.Name = dto.Name;
        existingCategory.DebtorCode = dto.DebtorCode; // Already uppercased before validation
        existingCategory.SpecialRules = dto.SpecialRules;
        existingCategory.IsActive = dto.IsActive;

        // STEP 6: Save changes
        await _categoryRepository.UpdateAsync(existingCategory);

        return existingCategory;
    }

    /// <summary>
    /// Gets a category by ID with customer count.
    /// </summary>
    public async Task<DebtorCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category != null ? await MapToDtoAsync(category) : null;
    }

    /// <summary>
    /// Gets all categories with customer counts.
    /// </summary>
    public async Task<IEnumerable<DebtorCategoryDto>> GetAllCategoriesAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        var dtos = new List<DebtorCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets paginated categories.
    /// </summary>
    public async Task<IEnumerable<DebtorCategoryDto>> GetPagedCategoriesAsync(int pageNumber, int pageSize)
    {
        var categories = await _categoryRepository.GetPagedAsync(pageNumber, pageSize);
        var dtos = new List<DebtorCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Deactivates a category (soft delete).
    /// Note: Customers in this category are not affected.
    /// Consider moving customers to another category before deactivating.
    /// </summary>
    public async Task DeactivateCategoryAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new InvalidOperationException($"Customer category with ID {id} not found");
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
    public async Task<IEnumerable<DebtorCategoryDto>> SearchCategoriesAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<DebtorCategoryDto>();

        var normalizedSearch = searchTerm.ToUpperInvariant();

        var categories = await _categoryRepository.FindAsync(c =>
            c.Name!.ToUpper().Contains(normalizedSearch) ||
            c.DebtorCode.ToUpper().Contains(normalizedSearch));

        var dtos = new List<DebtorCategoryDto>();
        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets a category by its unique code.
    /// </summary>
    public async Task<DebtorCategoryDto?> GetCategoryByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var normalizedCode = code.ToUpperInvariant();
        var categories = await _categoryRepository.FindAsync(c =>
            c.DebtorCode.ToUpper() == normalizedCode);

        var category = categories.FirstOrDefault();
        return category != null ? await MapToDtoAsync(category) : null;
    }

    /// <summary>
    /// Gets all active categories only.
    /// </summary>
    public async Task<IEnumerable<DebtorCategoryDto>> GetActiveCategoriesAsync()
    {
        var categories = await _categoryRepository.FindAsync(c => c.IsActive);
        var dtos = new List<DebtorCategoryDto>();

        foreach (var category in categories)
        {
            dtos.Add(await MapToDtoAsync(category));
        }

        return dtos;
    }

    /// <summary>
    /// Gets all categories with special compliance rules.
    /// Cannabis Act: Medical patients require Section 21 permits and additional documentation.
    /// POPIA: Enhanced data protection and consent requirements for sensitive categories.
    /// </summary>
    public async Task<IEnumerable<DebtorCategoryDto>> GetCategoriesWithSpecialRulesAsync()
    {
        var categories = await _categoryRepository.FindAsync(c => c.SpecialRules);
        var dtos = new List<DebtorCategoryDto>();

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
            c.DebtorCode.ToUpper() == normalizedCode &&
            (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));

        return !categories.Any();
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Maps a DebtorCategory entity to a DebtorCategoryDto.
    /// Includes customer count for the category.
    /// </summary>
    private async Task<DebtorCategoryDto> MapToDtoAsync(DebtorCategory category)
    {
        // Get customer count for this category
        // Note: Assuming Debtor entity has a CategoryId property
        // If not, this will need to be adjusted based on actual schema
        var customers = await _debtorRepository.FindAsync(d => d.Id == category.Id); // TODO: Update with correct property
        var customerCount = customers.Count();

        return new DebtorCategoryDto
        {
            Id = category.Id,
            Name = category.Name ?? string.Empty,
            DebtorCode = category.DebtorCode,
            SpecialRules = category.SpecialRules,
            IsActive = category.IsActive,
            CustomerCount = customerCount,
            CreatedAt = category.CreatedAt,
            CreatedBy = category.CreatedBy,
            UpdatedAt = category.ModifiedAt,
            UpdatedBy = category.ModifiedBy
        };
    }
}
