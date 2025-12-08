using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.Models.Entities.ProductManagement;

namespace Project420.Management.BLL.StockManagement.Services;

/// <summary>
/// Service interface for product business logic.
/// Handles product management, cannabis compliance validation, and stock operations.
/// </summary>
public interface IProductService
{
    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new product with full validation and compliance checks.
    /// </summary>
    /// <param name="dto">Product creation data</param>
    /// <returns>Created product with generated ID</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If duplicate SKU exists</exception>
    Task<Product> CreateProductAsync(CreateProductDto dto);

    /// <summary>
    /// Updates an existing product with validation and compliance checks.
    /// </summary>
    /// <param name="dto">Product update data</param>
    /// <returns>Updated product</returns>
    /// <exception cref="ValidationException">If validation fails</exception>
    /// <exception cref="InvalidOperationException">If product not found or duplicate SKU</exception>
    Task<Product> UpdateProductAsync(UpdateProductDto dto);

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<ProductDto?> GetProductByIdAsync(int id);

    /// <summary>
    /// Gets all active products.
    /// </summary>
    /// <returns>Collection of all active products</returns>
    Task<IEnumerable<ProductDto>> GetAllProductsAsync();

    /// <summary>
    /// Gets paginated products.
    /// </summary>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <returns>Page of products</returns>
    Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize);

    /// <summary>
    /// Deactivates a product (soft delete).
    /// POPIA requires data retention, so we use soft delete.
    /// </summary>
    /// <param name="id">Product ID</param>
    Task DeactivateProductAsync(int id);

    // ============================================================
    // SEARCH & FILTER OPERATIONS
    // ============================================================

    /// <summary>
    /// Searches for products by SKU, name, or strain.
    /// </summary>
    /// <param name="searchTerm">Search term</param>
    /// <returns>Matching products</returns>
    Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    /// <param name="sku">Stock Keeping Unit</param>
    /// <returns>Product if found, null otherwise</returns>
    Task<ProductDto?> GetProductBySkuAsync(string sku);

    /// <summary>
    /// Gets products by strain name.
    /// </summary>
    /// <param name="strainName">Cannabis strain name</param>
    /// <returns>Products with matching strain</returns>
    Task<IEnumerable<ProductDto>> GetProductsByStrainAsync(string strainName);

    /// <summary>
    /// Gets products by batch number.
    /// Cannabis compliance: Required for seed-to-sale traceability.
    /// </summary>
    /// <param name="batchNumber">Batch or lot number</param>
    /// <returns>Products with matching batch</returns>
    Task<IEnumerable<ProductDto>> GetProductsByBatchAsync(string batchNumber);

    // ============================================================
    // STOCK MANAGEMENT
    // ============================================================

    /// <summary>
    /// Adds stock to a product.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to add</param>
    /// <param name="reason">Reason for stock increase</param>
    /// <returns>Updated product</returns>
    Task<Product> AddStockAsync(int productId, int quantity, string reason);

    /// <summary>
    /// Removes stock from a product.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">Quantity to remove</param>
    /// <param name="reason">Reason for stock decrease</param>
    /// <returns>Updated product</returns>
    /// <exception cref="InvalidOperationException">If insufficient stock</exception>
    Task<Product> RemoveStockAsync(int productId, int quantity, string reason);

    /// <summary>
    /// Adjusts stock to a specific quantity.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="newQuantity">New stock quantity</param>
    /// <param name="reason">Reason for stock adjustment</param>
    /// <returns>Updated product</returns>
    Task<Product> AdjustStockAsync(int productId, int newQuantity, string reason);

    // ============================================================
    // INVENTORY ALERTS
    // ============================================================

    /// <summary>
    /// Gets all products with low stock (at or below reorder level).
    /// Used for inventory management and purchasing decisions.
    /// </summary>
    /// <returns>Products with low stock</returns>
    Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();

    /// <summary>
    /// Gets all products that are out of stock.
    /// </summary>
    /// <returns>Out of stock products</returns>
    Task<IEnumerable<ProductDto>> GetOutOfStockProductsAsync();

    /// <summary>
    /// Gets products with expiring dates.
    /// Important for edibles and cannabis oils.
    /// </summary>
    /// <param name="daysUntilExpiry">Number of days until expiry</param>
    /// <returns>Products expiring soon</returns>
    Task<IEnumerable<ProductDto>> GetExpiringProductsAsync(int daysUntilExpiry = 30);

    // ============================================================
    // CANNABIS COMPLIANCE
    // ============================================================

    /// <summary>
    /// Checks if a SKU is unique (not already in use).
    /// Required for preventing duplicate products.
    /// </summary>
    /// <param name="sku">SKU to check</param>
    /// <param name="excludeProductId">Product ID to exclude from check (for updates)</param>
    /// <returns>True if SKU is unique, false otherwise</returns>
    Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null);

    /// <summary>
    /// Validates if a product meets age verification requirements.
    /// Cannabis Act requires 18+ years for all cannabis products.
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>True if age verification required</returns>
    Task<bool> RequiresAgeVerificationAsync(int productId);
}
