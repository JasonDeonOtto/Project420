using Project420.Management.BLL.Sales.Retail.DTOs;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.BLL.Sales.Retail.Services;

/// <summary>
/// Service interface for pricelist business logic.
/// Handles pricelist management, product pricing, and pricelist item operations.
/// </summary>
public interface IPricelistService
{
    // ============================================================
    // PRICELIST CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new pricelist with full validation.
    /// </summary>
    /// <param name="dto">Pricelist creation data</param>
    /// <returns>Created pricelist with generated ID</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If duplicate pricelist name or code exists</exception>
    Task<RetailPricelist> CreatePricelistAsync(CreatePricelistDto dto);

    /// <summary>
    /// Updates an existing pricelist with validation.
    /// </summary>
    /// <param name="dto">Pricelist update data</param>
    /// <returns>Updated pricelist</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If pricelist not found or duplicate name/code</exception>
    Task<RetailPricelist> UpdatePricelistAsync(UpdatePricelistDto dto);

    /// <summary>
    /// Gets a pricelist by ID.
    /// </summary>
    /// <param name="id">Pricelist ID</param>
    /// <returns>Pricelist if found, null otherwise</returns>
    Task<PricelistDto?> GetPricelistByIdAsync(int id);

    /// <summary>
    /// Gets all active pricelists.
    /// </summary>
    /// <returns>Collection of all active pricelists</returns>
    Task<IEnumerable<PricelistDto>> GetAllPricelistsAsync();

    /// <summary>
    /// Gets paginated pricelists.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Page of pricelists</returns>
    Task<IEnumerable<PricelistDto>> GetPagedPricelistsAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Deactivates a pricelist (soft delete).
    /// </summary>
    /// <param name="id">Pricelist ID</param>
    Task DeactivatePricelistAsync(int id);

    // ============================================================
    // PRICELIST SEARCH & FILTER
    // ============================================================

    /// <summary>
    /// Searches for pricelists by name or code.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Matching pricelists</returns>
    Task<IEnumerable<PricelistDto>> SearchPricelistsAsync(string searchTerm);

    /// <summary>
    /// Gets a pricelist by name.
    /// </summary>
    /// <param name="name">Pricelist name</param>
    /// <returns>Pricelist if found, null otherwise</returns>
    Task<PricelistDto?> GetPricelistByNameAsync(string name);

    /// <summary>
    /// Gets a pricelist by code.
    /// </summary>
    /// <param name="code">Pricelist code</param>
    /// <returns>Pricelist if found, null otherwise</returns>
    Task<PricelistDto?> GetPricelistByCodeAsync(string code);

    /// <summary>
    /// Gets the default pricelist (for walk-in customers).
    /// </summary>
    /// <returns>Default pricelist if set, null otherwise</returns>
    Task<PricelistDto?> GetDefaultPricelistAsync();

    /// <summary>
    /// Gets currently effective pricelists (within date range).
    /// </summary>
    /// <returns>Collection of effective pricelists</returns>
    Task<IEnumerable<PricelistDto>> GetEffectivePricelistsAsync();

    // ============================================================
    // PRICELIST MANAGEMENT
    // ============================================================

    /// <summary>
    /// Sets a pricelist as the default.
    /// Automatically unsets any other default pricelist.
    /// </summary>
    /// <param name="pricelistId">Pricelist ID to set as default</param>
    Task SetAsDefaultPricelistAsync(int pricelistId);

    /// <summary>
    /// Checks if a pricelist name is unique.
    /// </summary>
    /// <param name="name">Pricelist name</param>
    /// <param name="excludePricelistId">Pricelist ID to exclude from check (for updates)</param>
    /// <returns>True if name is unique, false otherwise</returns>
    Task<bool> IsPricelistNameUniqueAsync(string name, int? excludePricelistId = null);

    /// <summary>
    /// Checks if a pricelist code is unique.
    /// </summary>
    /// <param name="code">Pricelist code</param>
    /// <param name="excludePricelistId">Pricelist ID to exclude from check (for updates)</param>
    /// <returns>True if code is unique, false otherwise</returns>
    Task<bool> IsPricelistCodeUniqueAsync(string code, int? excludePricelistId = null);

    // ============================================================
    // PRICELIST ITEM CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Adds a product to a pricelist with a specific price.
    /// </summary>
    /// <param name="dto">Pricelist item creation data</param>
    /// <returns>Created pricelist item with generated ID</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If product already in pricelist or product/pricelist not found</exception>
    Task<RetailPricelistItem> AddProductToPricelistAsync(CreatePricelistItemDto dto);

    /// <summary>
    /// Updates a pricelist item (change price or quantity tiers).
    /// </summary>
    /// <param name="dto">Pricelist item update data</param>
    /// <returns>Updated pricelist item</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If pricelist item not found</exception>
    Task<RetailPricelistItem> UpdatePricelistItemAsync(UpdatePricelistItemDto dto);

    /// <summary>
    /// Removes a product from a pricelist.
    /// </summary>
    /// <param name="pricelistItemId">Pricelist item ID</param>
    Task RemoveProductFromPricelistAsync(int pricelistItemId);

    /// <summary>
    /// Gets all products in a pricelist.
    /// </summary>
    /// <param name="pricelistId">Pricelist ID</param>
    /// <returns>Collection of pricelist items</returns>
    Task<IEnumerable<PricelistItemDto>> GetPricelistItemsAsync(int pricelistId);

    /// <summary>
    /// Gets a product's price in a specific pricelist.
    /// </summary>
    /// <param name="pricelistId">Pricelist ID</param>
    /// <param name="productId">Product ID</param>
    /// <returns>Pricelist item if product in pricelist, null otherwise</returns>
    Task<PricelistItemDto?> GetProductPriceInPricelistAsync(int pricelistId, int productId);

    /// <summary>
    /// Checks if a product is already in a pricelist.
    /// </summary>
    /// <param name="pricelistId">Pricelist ID</param>
    /// <param name="productId">Product ID</param>
    /// <returns>True if product in pricelist, false otherwise</returns>
    Task<bool> IsProductInPricelistAsync(int pricelistId, int productId);

    // ============================================================
    // BULK OPERATIONS
    // ============================================================

    /// <summary>
    /// Adds multiple products to a pricelist at once.
    /// </summary>
    /// <param name="items">Collection of pricelist items to add</param>
    /// <returns>Number of items successfully added</returns>
    Task<int> AddMultipleProductsToPricelistAsync(IEnumerable<CreatePricelistItemDto> items);

    /// <summary>
    /// Copies all products from one pricelist to another.
    /// </summary>
    /// <param name="sourcePricelistId">Source pricelist ID</param>
    /// <param name="targetPricelistId">Target pricelist ID</param>
    /// <param name="applyPercentageAdjustment">Optional percentage adjustment to apply</param>
    /// <returns>Number of products copied</returns>
    Task<int> CopyPricelistItemsAsync(int sourcePricelistId, int targetPricelistId, decimal? applyPercentageAdjustment = null);
}
