using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.BLL.StockManagement.Services;
using Project420.Management.DAL.Repositories.StockManagement;
using Project420.Management.Models.Entities.ProductManagement;
using Project420.Shared.Core.Compliance.Services;

namespace Project420.Management.Tests.Services.StockManagement;

/// <summary>
/// Unit tests for ProductService.
/// Tests business logic without hitting the real database.
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateValidator;
    private readonly Mock<ICannabisComplianceService> _mockComplianceService;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        // Arrange: Set up mock dependencies
        _mockRepository = new Mock<IProductRepository>();
        _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();
        _mockComplianceService = new Mock<ICannabisComplianceService>();

        // Create service with mocked dependencies
        _service = new ProductService(
            _mockRepository.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockComplianceService.Object
        );
    }

    #region CreateProduct Tests

    [Fact]
    public async Task CreateProduct_WithValidData_ReturnsProduct()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "PROD001",
            Name = "Blue Dream",
            Description = "Premium Sativa strain",
            Price = 250.00m,
            CostPrice = 150.00m,
            StockOnHand = 100,
            ReorderLevel = 20,
            THCPercentage = "22%",
            CBDPercentage = "1%",
            StrainName = "Blue Dream",
            BatchNumber = "BD2025001",
            IsActive = true
        };

        // Mock validator to return success
        _mockCreateValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        // Mock repository to check SKU is unique
        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .ReturnsAsync(new List<Product>()); // No duplicates found

        // Mock repository to return the created product
        _mockRepository
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .ReturnsAsync((Product p) => p);

        // Act
        var result = await _service.CreateProductAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.SKU.Should().Be("PROD001");
        result.Name.Should().Be("Blue Dream");
        result.Price.Should().Be(250.00m);
        result.THCPercentage.Should().Be("22%");

        // Verify repository was called
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidData_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "", // Invalid: empty SKU
            Name = "Test Product",
            Price = -10 // Invalid: negative price
        };

        // Mock validator to return validation errors
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("SKU", "SKU is required"),
            new ValidationFailure("Price", "Price must be positive")
        };

        _mockCreateValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            async () => await _service.CreateProductAsync(dto)
        );

        // Verify repository was never called (validation failed first)
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task CreateProduct_WithDuplicateSKU_ThrowsException()
    {
        // Arrange
        var dto = new CreateProductDto
        {
            SKU = "DUPLICATE",
            Name = "Test Product",
            Price = 100
        };

        // Mock validator to return success
        _mockCreateValidator
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        // Mock repository to return existing product with same SKU
        var existingProduct = new Product { Id = 1, SKU = "DUPLICATE", Name = "Existing" };
        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .ReturnsAsync(new List<Product> { existingProduct });

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.CreateProductAsync(dto)
        );

        exception.Message.Should().Contain("DUPLICATE");
        exception.Message.Should().Contain("already exists");
    }

    #endregion

    #region GetProduct Tests

    [Fact]
    public async Task GetProductById_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var productId = 5;
        var product = new Product
        {
            Id = productId,
            SKU = "TEST001",
            Name = "Test Product",
            Price = 150.00m,
            CostPrice = 100.00m,
            StockOnHand = 50,
            ReorderLevel = 10,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "TestUser"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(productId);
        result.SKU.Should().Be("TEST001");
        result.Name.Should().Be("Test Product");
        result.Price.Should().Be(150.00m);

        // Verify profit margin is calculated correctly
        result.ProfitMarginPercentage.Should().BeApproximately(33.33m, 0.01m);
    }

    [Fact]
    public async Task GetProductById_NonExistentProduct_ReturnsNull()
    {
        // Arrange
        var productId = 999;

        _mockRepository
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(productId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region Stock Management Tests

    [Fact]
    public async Task AddStock_WithValidQuantity_IncreasesStock()
    {
        // Arrange
        var productId = 1;
        var currentStock = 50;
        var addQuantity = 25;
        var reason = "Received new shipment";

        var product = new Product
        {
            Id = productId,
            SKU = "TEST001",
            Name = "Test Product",
            StockOnHand = currentStock
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddStockAsync(productId, addQuantity, reason);

        // Assert
        result.Should().NotBeNull();
        result.StockOnHand.Should().Be(75); // 50 + 25

        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task AddStock_WithZeroQuantity_ThrowsArgumentException()
    {
        // Arrange
        var productId = 1;
        var invalidQuantity = 0;
        var reason = "Test";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _service.AddStockAsync(productId, invalidQuantity, reason)
        );
    }

    [Fact]
    public async Task RemoveStock_WithSufficientStock_DecreasesStock()
    {
        // Arrange
        var productId = 1;
        var currentStock = 50;
        var removeQuantity = 20;
        var reason = "Sold to customer";

        var product = new Product
        {
            Id = productId,
            SKU = "TEST001",
            Name = "Test Product",
            StockOnHand = currentStock
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        _mockRepository
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.RemoveStockAsync(productId, removeQuantity, reason);

        // Assert
        result.Should().NotBeNull();
        result.StockOnHand.Should().Be(30); // 50 - 20
    }

    [Fact]
    public async Task RemoveStock_WithInsufficientStock_ThrowsException()
    {
        // Arrange
        var productId = 1;
        var currentStock = 10;
        var removeQuantity = 50; // More than available

        var product = new Product
        {
            Id = productId,
            SKU = "TEST001",
            Name = "Test Product",
            StockOnHand = currentStock
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.RemoveStockAsync(productId, removeQuantity, "Sale")
        );

        exception.Message.Should().Contain("Insufficient stock");
        exception.Message.Should().Contain("Available: 10");
        exception.Message.Should().Contain("Requested: 50");
    }

    #endregion

    #region Inventory Alert Tests

    [Fact]
    public async Task GetLowStockProducts_ReturnsProductsBelowReorderLevel()
    {
        // Arrange
        var lowStockProducts = new List<Product>
        {
            new Product
            {
                Id = 1,
                SKU = "LOW001",
                Name = "Low Stock Product 1",
                StockOnHand = 5,
                ReorderLevel = 20
            },
            new Product
            {
                Id = 2,
                SKU = "LOW002",
                Name = "Low Stock Product 2",
                StockOnHand = 15,
                ReorderLevel = 20
            }
        };

        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .ReturnsAsync(lowStockProducts);

        // Act
        var result = await _service.GetLowStockProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(p => p.IsLowStock == true);
    }

    [Fact]
    public async Task GetExpiringProducts_ReturnsProductsExpiringWithinPeriod()
    {
        // Arrange
        var expiringProducts = new List<Product>
        {
            new Product
            {
                Id = 1,
                SKU = "EXP001",
                Name = "Expiring Product",
                ExpiryDate = DateTime.Today.AddDays(15) // Expires in 15 days
            }
        };

        _mockRepository
            .Setup(r => r.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .ReturnsAsync(expiringProducts);

        // Act
        var result = await _service.GetExpiringProductsAsync(daysUntilExpiry: 30);

        // Assert
        result.Should().HaveCount(1);
        result.First().SKU.Should().Be("EXP001");
    }

    #endregion
}
