using FluentValidation;
using Project420.Management.BLL.Sales.Retail.DTOs;
using Project420.Management.DAL.Repositories.Sales.Retail;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.BLL.Sales.Retail.Services;

/// <summary>
/// Service for pricelist business logic.
/// Handles pricelist management, product pricing, and pricelist item operations.
/// </summary>
public class PricelistService : IPricelistService
{
    private readonly IRetailPricelistRepository _pricelistRepository;
    private readonly IRetailPricelistItemRepository _pricelistItemRepository;
    private readonly IValidator<CreatePricelistDto> _createPricelistValidator;
    private readonly IValidator<UpdatePricelistDto> _updatePricelistValidator;
    private readonly IValidator<CreatePricelistItemDto> _createItemValidator;
    private readonly IValidator<UpdatePricelistItemDto> _updateItemValidator;

    public PricelistService(
        IRetailPricelistRepository pricelistRepository,
        IRetailPricelistItemRepository pricelistItemRepository,
        IValidator<CreatePricelistDto> createPricelistValidator,
        IValidator<UpdatePricelistDto> updatePricelistValidator,
        IValidator<CreatePricelistItemDto> createItemValidator,
        IValidator<UpdatePricelistItemDto> updateItemValidator)
    {
        _pricelistRepository = pricelistRepository;
        _pricelistItemRepository = pricelistItemRepository;
        _createPricelistValidator = createPricelistValidator;
        _updatePricelistValidator = updatePricelistValidator;
        _createItemValidator = createItemValidator;
        _updateItemValidator = updateItemValidator;
    }

    // ============================================================
    // PRICELIST CRUD OPERATIONS
    // ============================================================

    public async Task<RetailPricelist> CreatePricelistAsync(CreatePricelistDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _createPricelistValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check for duplicate name
        if (!await IsPricelistNameUniqueAsync(dto.Name))
        {
            throw new InvalidOperationException($"Pricelist with name '{dto.Name}' already exists");
        }

        // STEP 3: Check for duplicate code (if provided)
        if (!string.IsNullOrWhiteSpace(dto.Code) && !await IsPricelistCodeUniqueAsync(dto.Code))
        {
            throw new InvalidOperationException($"Pricelist with code '{dto.Code}' already exists");
        }

        // STEP 4: If this is marked as default, unset other defaults
        if (dto.IsDefault)
        {
            await UnsetAllDefaultPricelistsAsync();
        }

        // STEP 5: Map DTO to Entity
        var pricelist = new RetailPricelist
        {
            Name = dto.Name,
            Description = dto.Description,
            Code = dto.Code?.ToUpperInvariant(), // Normalize code to uppercase
            IsActive = dto.IsActive,
            IsDefault = dto.IsDefault,
            EffectiveFrom = dto.EffectiveFrom,
            EffectiveTo = dto.EffectiveTo,
            PricingStrategy = dto.PricingStrategy,
            PercentageAdjustment = dto.PercentageAdjustment,
            Priority = dto.Priority
        };

        // STEP 6: Save to database
        var createdPricelist = await _pricelistRepository.AddAsync(pricelist);

        return createdPricelist;
    }

    public async Task<RetailPricelist> UpdatePricelistAsync(UpdatePricelistDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _updatePricelistValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check if pricelist exists
        var existingPricelist = await _pricelistRepository.GetByIdAsync(dto.Id);
        if (existingPricelist == null)
        {
            throw new InvalidOperationException($"Pricelist with ID {dto.Id} not found");
        }

        // STEP 3: Check for duplicate name (excluding current pricelist)
        if (!await IsPricelistNameUniqueAsync(dto.Name, dto.Id))
        {
            throw new InvalidOperationException($"Pricelist with name '{dto.Name}' already exists");
        }

        // STEP 4: Check for duplicate code (if provided, excluding current pricelist)
        if (!string.IsNullOrWhiteSpace(dto.Code) && !await IsPricelistCodeUniqueAsync(dto.Code, dto.Id))
        {
            throw new InvalidOperationException($"Pricelist with code '{dto.Code}' already exists");
        }

        // STEP 5: If this is marked as default, unset other defaults
        if (dto.IsDefault && !existingPricelist.IsDefault)
        {
            await UnsetAllDefaultPricelistsAsync();
        }

        // STEP 6: Update entity properties
        existingPricelist.Name = dto.Name;
        existingPricelist.Description = dto.Description;
        existingPricelist.Code = dto.Code?.ToUpperInvariant();
        existingPricelist.IsActive = dto.IsActive;
        existingPricelist.IsDefault = dto.IsDefault;
        existingPricelist.EffectiveFrom = dto.EffectiveFrom;
        existingPricelist.EffectiveTo = dto.EffectiveTo;
        existingPricelist.PricingStrategy = dto.PricingStrategy;
        existingPricelist.PercentageAdjustment = dto.PercentageAdjustment;
        existingPricelist.Priority = dto.Priority;

        // STEP 7: Save changes
        await _pricelistRepository.UpdateAsync(existingPricelist);

        return existingPricelist;
    }

    public async Task<PricelistDto?> GetPricelistByIdAsync(int id)
    {
        var pricelist = await _pricelistRepository.GetByIdAsync(id);
        return pricelist != null ? await MapToDtoAsync(pricelist) : null;
    }

    public async Task<IEnumerable<PricelistDto>> GetAllPricelistsAsync()
    {
        var pricelists = await _pricelistRepository.GetAllAsync();
        var dtos = new List<PricelistDto>();

        foreach (var pricelist in pricelists)
        {
            dtos.Add(await MapToDtoAsync(pricelist));
        }

        return dtos;
    }

    public async Task<IEnumerable<PricelistDto>> GetPagedPricelistsAsync(int pageNumber, int pageSize)
    {
        var pricelists = await _pricelistRepository.GetPagedAsync(pageNumber, pageSize);
        var dtos = new List<PricelistDto>();

        foreach (var pricelist in pricelists)
        {
            dtos.Add(await MapToDtoAsync(pricelist));
        }

        return dtos;
    }

    public async Task DeactivatePricelistAsync(int id)
    {
        await _pricelistRepository.DeleteAsync(id);
    }

    // ============================================================
    // PRICELIST SEARCH & FILTER
    // ============================================================

    public async Task<IEnumerable<PricelistDto>> SearchPricelistsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<PricelistDto>();

        var normalizedSearch = searchTerm.ToUpperInvariant();

        var pricelists = await _pricelistRepository.FindAsync(p =>
            p.Name.ToUpper().Contains(normalizedSearch) ||
            (p.Code != null && p.Code.ToUpper().Contains(normalizedSearch)));

        var dtos = new List<PricelistDto>();
        foreach (var pricelist in pricelists)
        {
            dtos.Add(await MapToDtoAsync(pricelist));
        }

        return dtos;
    }

    public async Task<PricelistDto?> GetPricelistByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var pricelist = await _pricelistRepository.GetByNameAsync(name);
        return pricelist != null ? await MapToDtoAsync(pricelist) : null;
    }

    public async Task<PricelistDto?> GetPricelistByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var pricelist = await _pricelistRepository.GetByCodeAsync(code);
        return pricelist != null ? await MapToDtoAsync(pricelist) : null;
    }

    public async Task<PricelistDto?> GetDefaultPricelistAsync()
    {
        var pricelists = await _pricelistRepository.FindAsync(p => p.IsDefault);
        var defaultPricelist = pricelists.FirstOrDefault();

        return defaultPricelist != null ? await MapToDtoAsync(defaultPricelist) : null;
    }

    public async Task<IEnumerable<PricelistDto>> GetEffectivePricelistsAsync()
    {
        var today = DateTime.Today;

        var pricelists = await _pricelistRepository.FindAsync(p =>
            p.IsActive &&
            (!p.EffectiveFrom.HasValue || p.EffectiveFrom.Value.Date <= today) &&
            (!p.EffectiveTo.HasValue || p.EffectiveTo.Value.Date >= today));

        var dtos = new List<PricelistDto>();
        foreach (var pricelist in pricelists)
        {
            dtos.Add(await MapToDtoAsync(pricelist));
        }

        return dtos;
    }

    // ============================================================
    // PRICELIST MANAGEMENT
    // ============================================================

    public async Task SetAsDefaultPricelistAsync(int pricelistId)
    {
        var pricelist = await _pricelistRepository.GetByIdAsync(pricelistId);
        if (pricelist == null)
        {
            throw new InvalidOperationException($"Pricelist with ID {pricelistId} not found");
        }

        // Unset all other defaults
        await UnsetAllDefaultPricelistsAsync();

        // Set this one as default
        pricelist.IsDefault = true;
        await _pricelistRepository.UpdateAsync(pricelist);
    }

    public async Task<bool> IsPricelistNameUniqueAsync(string name, int? excludePricelistId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.ToUpperInvariant();

        var pricelists = await _pricelistRepository.FindAsync(p =>
            p.Name.ToUpper() == normalizedName &&
            (!excludePricelistId.HasValue || p.Id != excludePricelistId.Value));

        return !pricelists.Any();
    }

    public async Task<bool> IsPricelistCodeUniqueAsync(string code, int? excludePricelistId = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            return true; // Code is optional

        var normalizedCode = code.ToUpperInvariant();

        var pricelists = await _pricelistRepository.FindAsync(p =>
            p.Code != null &&
            p.Code.ToUpper() == normalizedCode &&
            (!excludePricelistId.HasValue || p.Id != excludePricelistId.Value));

        return !pricelists.Any();
    }

    // ============================================================
    // PRICELIST ITEM CRUD OPERATIONS
    // ============================================================

    public async Task<RetailPricelistItem> AddProductToPricelistAsync(CreatePricelistItemDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _createItemValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check if product already in pricelist
        if (await IsProductInPricelistAsync(dto.PricelistId, dto.ProductId))
        {
            throw new InvalidOperationException($"Product with ID {dto.ProductId} is already in pricelist {dto.PricelistId}");
        }

        // STEP 3: Verify pricelist exists
        var pricelist = await _pricelistRepository.GetByIdAsync(dto.PricelistId);
        if (pricelist == null)
        {
            throw new InvalidOperationException($"Pricelist with ID {dto.PricelistId} not found");
        }

        // TODO: Verify product exists (need IProductRepository reference)

        // STEP 4: Map DTO to Entity
        var pricelistItem = new RetailPricelistItem
        {
            RetailPricelistId = dto.PricelistId,
            ProductId = dto.ProductId,
            Price = dto.Price,
            MinimumQuantity = dto.MinimumQuantity,
            MaximumQuantity = dto.MaximumQuantity
        };

        // STEP 5: Save to database
        var createdItem = await _pricelistItemRepository.AddAsync(pricelistItem);

        return createdItem;
    }

    public async Task<RetailPricelistItem> UpdatePricelistItemAsync(UpdatePricelistItemDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _updateItemValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check if pricelist item exists
        var existingItem = await _pricelistItemRepository.GetByIdAsync(dto.Id);
        if (existingItem == null)
        {
            throw new InvalidOperationException($"Pricelist item with ID {dto.Id} not found");
        }

        // STEP 3: Update entity properties
        existingItem.Price = dto.Price;
        existingItem.MinimumQuantity = dto.MinimumQuantity;
        existingItem.MaximumQuantity = dto.MaximumQuantity;

        // STEP 4: Save changes
        await _pricelistItemRepository.UpdateAsync(existingItem);

        return existingItem;
    }

    public async Task RemoveProductFromPricelistAsync(int pricelistItemId)
    {
        await _pricelistItemRepository.DeleteAsync(pricelistItemId);
    }

    public async Task<IEnumerable<PricelistItemDto>> GetPricelistItemsAsync(int pricelistId)
    {
        var items = await _pricelistItemRepository.FindAsync(i => i.RetailPricelistId == pricelistId);

        return items.Select(MapItemToDto);
    }

    public async Task<PricelistItemDto?> GetProductPriceInPricelistAsync(int pricelistId, int productId)
    {
        var items = await _pricelistItemRepository.FindAsync(i =>
            i.RetailPricelistId == pricelistId && i.ProductId == productId);

        var item = items.FirstOrDefault();

        return item != null ? MapItemToDto(item) : null;
    }

    public async Task<bool> IsProductInPricelistAsync(int pricelistId, int productId)
    {
        var items = await _pricelistItemRepository.FindAsync(i =>
            i.RetailPricelistId == pricelistId && i.ProductId == productId);

        return items.Any();
    }

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    public async Task<int> AddMultipleProductsToPricelistAsync(IEnumerable<CreatePricelistItemDto> items)
    {
        int count = 0;

        foreach (var item in items)
        {
            try
            {
                await AddProductToPricelistAsync(item);
                count++;
            }
            catch
            {
                // Continue processing other items even if one fails
                // TODO: Consider logging the failure
            }
        }

        return count;
    }

    public async Task<int> CopyPricelistItemsAsync(int sourcePricelistId, int targetPricelistId, decimal? applyPercentageAdjustment = null)
    {
        // Verify both pricelists exist
        var sourcePricelist = await _pricelistRepository.GetByIdAsync(sourcePricelistId);
        if (sourcePricelist == null)
        {
            throw new InvalidOperationException($"Source pricelist with ID {sourcePricelistId} not found");
        }

        var targetPricelist = await _pricelistRepository.GetByIdAsync(targetPricelistId);
        if (targetPricelist == null)
        {
            throw new InvalidOperationException($"Target pricelist with ID {targetPricelistId} not found");
        }

        // Get all items from source pricelist
        var sourceItems = await _pricelistItemRepository.FindAsync(i => i.RetailPricelistId == sourcePricelistId);

        int count = 0;

        foreach (var sourceItem in sourceItems)
        {
            // Skip if product already in target pricelist
            if (await IsProductInPricelistAsync(targetPricelistId, sourceItem.ProductId))
                continue;

            // Calculate adjusted price if percentage adjustment provided
            var price = sourceItem.Price;
            if (applyPercentageAdjustment.HasValue)
            {
                price = price * (1 + (applyPercentageAdjustment.Value / 100));
            }

            var newItem = new CreatePricelistItemDto
            {
                PricelistId = targetPricelistId,
                ProductId = sourceItem.ProductId,
                Price = price,
                MinimumQuantity = sourceItem.MinimumQuantity,
                MaximumQuantity = sourceItem.MaximumQuantity
            };

            try
            {
                await AddProductToPricelistAsync(newItem);
                count++;
            }
            catch
            {
                // Continue processing other items even if one fails
                // TODO: Consider logging the failure
            }
        }

        return count;
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    private async Task UnsetAllDefaultPricelistsAsync()
    {
        var defaultPricelists = await _pricelistRepository.FindAsync(p => p.IsDefault);

        foreach (var pricelist in defaultPricelists)
        {
            pricelist.IsDefault = false;
            await _pricelistRepository.UpdateAsync(pricelist);
        }
    }

    private async Task<PricelistDto> MapToDtoAsync(RetailPricelist pricelist)
    {
        // Get product count for this pricelist
        var items = await _pricelistItemRepository.FindAsync(i => i.RetailPricelistId == pricelist.Id);
        var productCount = items.Count();

        return new PricelistDto
        {
            Id = pricelist.Id,
            Name = pricelist.Name,
            Description = pricelist.Description,
            Code = pricelist.Code,
            IsActive = pricelist.IsActive,
            IsDefault = pricelist.IsDefault,
            EffectiveFrom = pricelist.EffectiveFrom,
            EffectiveTo = pricelist.EffectiveTo,
            PricingStrategy = pricelist.PricingStrategy,
            PercentageAdjustment = pricelist.PercentageAdjustment,
            Priority = pricelist.Priority,
            ProductCount = productCount,
            CreatedAt = pricelist.CreatedAt,
            CreatedBy = pricelist.CreatedBy,
            UpdatedAt = pricelist.ModifiedAt,
            UpdatedBy = pricelist.ModifiedBy
        };
    }

    private PricelistItemDto MapItemToDto(RetailPricelistItem item)
    {
        return new PricelistItemDto
        {
            Id = item.Id,
            PricelistId = item.RetailPricelistId,
            ProductId = item.ProductId,
            Price = item.Price,
            MinimumQuantity = item.MinimumQuantity,
            MaximumQuantity = item.MaximumQuantity,
            CreatedAt = item.CreatedAt,
            CreatedBy = item.CreatedBy,
            UpdatedAt = item.ModifiedAt,
            UpdatedBy = item.ModifiedBy
        };
    }
}
