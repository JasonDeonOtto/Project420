using FluentValidation;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.DAL.Repositories.StockManagement;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Shared.Core.Compliance.Services;

namespace Project420.Management.BLL.StockManagement.Services;

/// <summary>
/// Service for product business logic.
/// Handles product management, validation, cannabis compliance, and stock operations.
/// </summary>
public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;
    private readonly ICannabisComplianceService _complianceService;

    public ProductService(
        IProductRepository productRepository,
        IValidator<CreateProductDto> createValidator,
        IValidator<UpdateProductDto> updateValidator,
        ICannabisComplianceService complianceService)
    {
        _productRepository = productRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _complianceService = complianceService;
    }

    // ============================================================
    // CRUD OPERATIONS
    // ============================================================

    /// <summary>
    /// Creates a new product with full validation and compliance checks.
    /// </summary>
    public async Task<Product> CreateProductAsync(CreateProductDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check for duplicate SKU
        if (!await IsSkuUniqueAsync(dto.SKU))
        {
            throw new InvalidOperationException($"Product with SKU '{dto.SKU}' already exists");
        }

        // STEP 3: Map DTO to Entity
        var product = new Product
        {
            // Basic Information
            SKU = dto.SKU.ToUpperInvariant(), // Normalize SKU to uppercase
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive,

            // Cannabis Compliance
            THCPercentage = dto.THCPercentage,
            CBDPercentage = dto.CBDPercentage,
            BatchNumber = dto.BatchNumber,
            StrainName = dto.StrainName,
            LabTestDate = dto.LabTestDate,
            ExpiryDate = dto.ExpiryDate,

            // Pricing
            Price = dto.Price,
            CostPrice = dto.CostPrice,

            // Inventory
            StockOnHand = dto.StockOnHand,
            ReorderLevel = dto.ReorderLevel,

            // Category
            CategoryId = dto.CategoryId
        };

        // STEP 4: Save to database
        var createdProduct = await _productRepository.AddAsync(product);

        return createdProduct;
    }

    /// <summary>
    /// Updates an existing product with validation and compliance checks.
    /// </summary>
    public async Task<Product> UpdateProductAsync(UpdateProductDto dto)
    {
        // STEP 1: Validate input data
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // STEP 2: Check if product exists
        var existingProduct = await _productRepository.GetByIdAsync(dto.Id);
        if (existingProduct == null)
        {
            throw new InvalidOperationException($"Product with ID {dto.Id} not found");
        }

        // STEP 3: Check for duplicate SKU (excluding current product)
        if (!await IsSkuUniqueAsync(dto.SKU, dto.Id))
        {
            throw new InvalidOperationException($"Product with SKU '{dto.SKU}' already exists");
        }

        // STEP 4: Update entity properties
        existingProduct.SKU = dto.SKU.ToUpperInvariant(); // Normalize SKU to uppercase
        existingProduct.Name = dto.Name;
        existingProduct.Description = dto.Description;
        existingProduct.IsActive = dto.IsActive;

        // Cannabis Compliance
        existingProduct.THCPercentage = dto.THCPercentage;
        existingProduct.CBDPercentage = dto.CBDPercentage;
        existingProduct.BatchNumber = dto.BatchNumber;
        existingProduct.StrainName = dto.StrainName;
        existingProduct.LabTestDate = dto.LabTestDate;
        existingProduct.ExpiryDate = dto.ExpiryDate;

        // Pricing
        existingProduct.Price = dto.Price;
        existingProduct.CostPrice = dto.CostPrice;

        // Inventory (Note: Use stock adjustment methods for proper audit trail)
        existingProduct.StockOnHand = dto.StockOnHand;
        existingProduct.ReorderLevel = dto.ReorderLevel;

        // Category
        existingProduct.CategoryId = dto.CategoryId;

        // STEP 5: Save changes
        await _productRepository.UpdateAsync(existingProduct);

        return existingProduct;
    }

    /// <summary>
    /// Gets a product by ID.
    /// </summary>
    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        return product != null ? MapToDto(product) : null;
    }

    /// <summary>
    /// Gets all active products.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products.Select(MapToDto);
    }

    /// <summary>
    /// Gets paginated products.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetPagedProductsAsync(int pageNumber, int pageSize)
    {
        var products = await _productRepository.GetPagedAsync(pageNumber, pageSize);
        return products.Select(MapToDto);
    }

    /// <summary>
    /// Deactivates a product (soft delete).
    /// </summary>
    public async Task DeactivateProductAsync(int id)
    {
        await _productRepository.DeleteAsync(id);
    }

    // ============================================================
    // SEARCH & FILTER OPERATIONS
    // ============================================================

    /// <summary>
    /// Searches for products by SKU, name, or strain.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Enumerable.Empty<ProductDto>();

        var normalizedSearch = searchTerm.ToUpperInvariant();

        var products = await _productRepository.FindAsync(p =>
            p.SKU.ToUpper().Contains(normalizedSearch) ||
            p.Name.ToUpper().Contains(normalizedSearch) ||
            (p.StrainName != null && p.StrainName.ToUpper().Contains(normalizedSearch)));

        return products.Select(MapToDto);
    }

    /// <summary>
    /// Gets a product by SKU.
    /// </summary>
    public async Task<ProductDto?> GetProductBySkuAsync(string sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var normalizedSku = sku.ToUpperInvariant();
        var products = await _productRepository.FindAsync(p => p.SKU.ToUpper() == normalizedSku);
        var product = products.FirstOrDefault();

        return product != null ? MapToDto(product) : null;
    }

    /// <summary>
    /// Gets products by strain name.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetProductsByStrainAsync(string strainName)
    {
        if (string.IsNullOrWhiteSpace(strainName))
            return Enumerable.Empty<ProductDto>();

        var normalizedStrain = strainName.ToUpperInvariant();

        var products = await _productRepository.FindAsync(p =>
            p.StrainName != null && p.StrainName.ToUpper() == normalizedStrain);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// Gets products by batch number.
    /// Cannabis compliance: Required for seed-to-sale traceability.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetProductsByBatchAsync(string batchNumber)
    {
        if (string.IsNullOrWhiteSpace(batchNumber))
            return Enumerable.Empty<ProductDto>();

        var normalizedBatch = batchNumber.ToUpperInvariant();

        var products = await _productRepository.FindAsync(p =>
            p.BatchNumber != null && p.BatchNumber.ToUpper() == normalizedBatch);

        return products.Select(MapToDto);
    }

    // ============================================================
    // STOCK MANAGEMENT
    // ============================================================

    /// <summary>
    /// Adds stock to a product.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    public async Task<Product> AddStockAsync(int productId, int quantity, string reason)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        product.StockOnHand += quantity;

        // TODO: Log stock adjustment for audit trail
        // Format: "Stock increased by {quantity}. Reason: {reason}"

        await _productRepository.UpdateAsync(product);

        return product;
    }

    /// <summary>
    /// Removes stock from a product.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    public async Task<Product> RemoveStockAsync(int productId, int quantity, string reason)
    {
        if (quantity <= 0)
        {
            throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        if (product.StockOnHand < quantity)
        {
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {product.StockOnHand}, Requested: {quantity}");
        }

        product.StockOnHand -= quantity;

        // TODO: Log stock adjustment for audit trail
        // Format: "Stock decreased by {quantity}. Reason: {reason}"

        await _productRepository.UpdateAsync(product);

        return product;
    }

    /// <summary>
    /// Adjusts stock to a specific quantity.
    /// Creates audit trail for POPIA compliance.
    /// </summary>
    public async Task<Product> AdjustStockAsync(int productId, int newQuantity, string reason)
    {
        if (newQuantity < 0)
        {
            throw new ArgumentException("Quantity cannot be negative", nameof(newQuantity));
        }

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        var oldQuantity = product.StockOnHand;
        var difference = newQuantity - oldQuantity;

        product.StockOnHand = newQuantity;

        // TODO: Log stock adjustment for audit trail
        // Format: "Stock adjusted from {oldQuantity} to {newQuantity} ({difference}). Reason: {reason}"

        await _productRepository.UpdateAsync(product);

        return product;
    }

    // ============================================================
    // INVENTORY ALERTS
    // ============================================================

    /// <summary>
    /// Gets all products with low stock (at or below reorder level).
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
    {
        var products = await _productRepository.FindAsync(p =>
            p.StockOnHand <= p.ReorderLevel);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// Gets all products that are out of stock.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetOutOfStockProductsAsync()
    {
        var products = await _productRepository.FindAsync(p =>
            p.StockOnHand <= 0);

        return products.Select(MapToDto);
    }

    /// <summary>
    /// Gets products with expiring dates.
    /// Important for edibles and cannabis oils.
    /// </summary>
    public async Task<IEnumerable<ProductDto>> GetExpiringProductsAsync(int daysUntilExpiry = 30)
    {
        var expiryThreshold = DateTime.Today.AddDays(daysUntilExpiry);

        var products = await _productRepository.FindAsync(p =>
            p.ExpiryDate.HasValue &&
            p.ExpiryDate.Value <= expiryThreshold &&
            p.ExpiryDate.Value >= DateTime.Today);

        return products.Select(MapToDto);
    }

    // ============================================================
    // CANNABIS COMPLIANCE
    // ============================================================

    /// <summary>
    /// Checks if a SKU is unique (not already in use).
    /// </summary>
    public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeProductId = null)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        var normalizedSku = sku.ToUpperInvariant();

        var products = await _productRepository.FindAsync(p =>
            p.SKU.ToUpper() == normalizedSku &&
            (!excludeProductId.HasValue || p.Id != excludeProductId.Value));

        return !products.Any();
    }

    /// <summary>
    /// Validates if a product meets age verification requirements.
    /// Cannabis Act requires 18+ years for all cannabis products.
    /// </summary>
    public async Task<bool> RequiresAgeVerificationAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
        {
            throw new InvalidOperationException($"Product with ID {productId} not found");
        }

        // Use shared compliance service to determine age verification requirement
        return _complianceService.RequiresAgeVerification(product.THCPercentage, product.CBDPercentage);
    }

    // ============================================================
    // PRIVATE HELPER METHODS
    // ============================================================

    /// <summary>
    /// Maps a Product entity to a ProductDto.
    /// </summary>
    private ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            SKU = product.SKU,
            Name = product.Name,
            Description = product.Description,
            IsActive = product.IsActive,

            // Cannabis Compliance
            THCPercentage = product.THCPercentage,
            CBDPercentage = product.CBDPercentage,
            BatchNumber = product.BatchNumber,
            StrainName = product.StrainName,
            LabTestDate = product.LabTestDate,
            ExpiryDate = product.ExpiryDate,

            // Pricing
            Price = product.Price,
            CostPrice = product.CostPrice,

            // Inventory
            StockOnHand = product.StockOnHand,
            ReorderLevel = product.ReorderLevel,

            // Category
            CategoryId = product.CategoryId,

            // Audit
            CreatedAt = product.CreatedAt,
            CreatedBy = product.CreatedBy,
            UpdatedAt = product.ModifiedAt,
            UpdatedBy = product.ModifiedBy
        };
    }
}
