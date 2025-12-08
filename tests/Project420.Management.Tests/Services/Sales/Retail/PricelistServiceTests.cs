using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using Project420.Management.BLL.Sales.Retail.DTOs;
using Project420.Management.BLL.Sales.Retail.Services;
using Project420.Management.DAL.Repositories.Sales.Retail;
using Project420.Management.Models.Entities.Sales.Retail;

namespace Project420.Management.Tests.Services.Sales.Retail;

/// <summary>
/// Comprehensive unit tests for PricelistService.
/// Tests pricelist management, validation, and pricing strategies.
/// CRITICAL: Pricelists drive multi-tier pricing for cannabis products.
/// </summary>
public class PricelistServiceTests
{
    private readonly Mock<IRetailPricelistRepository> _mockPricelistRepository;
    private readonly Mock<IRetailPricelistItemRepository> _mockPricelistItemRepository;
    private readonly Mock<IValidator<CreatePricelistDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdatePricelistDto>> _mockUpdateValidator;
    private readonly Mock<IValidator<CreatePricelistItemDto>> _mockCreateItemValidator;
    private readonly Mock<IValidator<UpdatePricelistItemDto>> _mockUpdateItemValidator;
    private readonly PricelistService _service;

    public PricelistServiceTests()
    {
        // Create mocks
        _mockPricelistRepository = new Mock<IRetailPricelistRepository>();
        _mockPricelistItemRepository = new Mock<IRetailPricelistItemRepository>();
        _mockCreateValidator = new Mock<IValidator<CreatePricelistDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdatePricelistDto>>();
        _mockCreateItemValidator = new Mock<IValidator<CreatePricelistItemDto>>();
        _mockUpdateItemValidator = new Mock<IValidator<UpdatePricelistItemDto>>();

        // Create service with mocked dependencies
        _service = new PricelistService(
            _mockPricelistRepository.Object,
            _mockPricelistItemRepository.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object,
            _mockCreateItemValidator.Object,
            _mockUpdateItemValidator.Object);

        // Setup default valid validation results
        _mockCreateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<CreatePricelistDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _mockUpdateValidator
            .Setup(x => x.ValidateAsync(It.IsAny<UpdatePricelistDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _mockCreateItemValidator
            .Setup(x => x.ValidateAsync(It.IsAny<CreatePricelistItemDto>(), default))
            .ReturnsAsync(new ValidationResult());

        _mockUpdateItemValidator
            .Setup(x => x.ValidateAsync(It.IsAny<UpdatePricelistItemDto>(), default))
            .ReturnsAsync(new ValidationResult());
    }

    #region CreatePricelistAsync Tests

    [Fact]
    public async Task CreatePricelistAsync_ValidData_ReturnsPricelist()
    {
        // Arrange
        var dto = new CreatePricelistDto
        {
            Name = "Standard Retail",
            Description = "Standard retail pricing for walk-in customers",
            Code = "STD",
            IsActive = true,
            IsDefault = true,
            EffectiveFrom = DateTime.Today,
            EffectiveTo = null,
            PricingStrategy = "Standard",
            Priority = 1
        };

        // Mock: Name and code are unique
        _mockPricelistRepository.Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist>());

        // Mock: Repository saves pricelist
        _mockPricelistRepository
            .Setup(x => x.AddAsync(It.IsAny<RetailPricelist>()))
            .ReturnsAsync((RetailPricelist pl) =>
            {
                pl.Id = 1;
                return pl;
            });

        // Act
        var result = await _service.CreatePricelistAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be(dto.Name);
        result.Code.Should().Be("STD"); // Normalized to uppercase
        result.IsDefault.Should().BeTrue();
        result.IsActive.Should().BeTrue();

        // Verify repository was called
        _mockPricelistRepository.Verify(x => x.AddAsync(It.IsAny<RetailPricelist>()), Times.Once);
    }

    [Fact]
    public async Task CreatePricelistAsync_ValidationFails_ThrowsValidationException()
    {
        // Arrange
        var dto = new CreatePricelistDto
        {
            Name = "", // Invalid: empty name
            Code = "TOOLONGCODE123", // Invalid: too long
            PricingStrategy = ""
        };

        // Mock: Validation fails
        var validationErrors = new List<ValidationFailure>
        {
            new ValidationFailure("Name", "Name is required"),
            new ValidationFailure("Code", "Code must be between 2-10 characters"),
            new ValidationFailure("PricingStrategy", "Pricing strategy is required")
        };
        _mockCreateValidator
            .Setup(x => x.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(validationErrors));

        // Act
        Func<Task> act = async () => await _service.CreatePricelistAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _mockPricelistRepository.Verify(x => x.AddAsync(It.IsAny<RetailPricelist>()), Times.Never);
    }

    [Fact]
    public async Task CreatePricelistAsync_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePricelistDto
        {
            Name = "Duplicate Pricelist",
            Code = "DUP",
            IsActive = true
        };

        // Mock: Existing pricelist with same name
        var existingPricelist = new RetailPricelist
        {
            Id = 10,
            Name = "Duplicate Pricelist"
        };
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist> { existingPricelist });

        // Act
        Func<Task> act = async () => await _service.CreatePricelistAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.Name}*already exists*");
        _mockPricelistRepository.Verify(x => x.AddAsync(It.IsAny<RetailPricelist>()), Times.Never);
    }

    [Fact]
    public async Task CreatePricelistAsync_DuplicateCode_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new CreatePricelistDto
        {
            Name = "New Pricelist",
            Code = "DUPCODE",
            IsActive = true
        };

        // Mock: Name is unique
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.Is<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>(expr => expr.ToString().Contains("Name"))))
            .ReturnsAsync(new List<RetailPricelist>());

        // Mock: Code is NOT unique
        var existingPricelist = new RetailPricelist
        {
            Id = 10,
            Code = "DUPCODE"
        };
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.Is<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>(expr => expr.ToString().Contains("Code"))))
            .ReturnsAsync(new List<RetailPricelist> { existingPricelist });

        // Act
        Func<Task> act = async () => await _service.CreatePricelistAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.Code}*already exists*");
    }

    [Fact]
    public async Task CreatePricelistAsync_SetAsDefault_UnsetsOtherDefaults()
    {
        // Arrange
        var dto = new CreatePricelistDto
        {
            Name = "New Default Pricelist",
            Code = "NEWDEF",
            IsDefault = true, // Setting as default
            IsActive = true
        };

        // Mock: Existing default pricelist
        var existingDefault = new RetailPricelist
        {
            Id = 5,
            Name = "Old Default",
            IsDefault = true
        };

        // Mock: Name and code uniqueness checks (no duplicates)
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.Is<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>(
                expr => expr.ToString().Contains("Name") || expr.ToString().Contains("Code"))))
            .ReturnsAsync(new List<RetailPricelist>());

        // Mock: FindAsync for IsDefault query returns existing default
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.Is<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>(
                expr => expr.ToString().Contains("IsDefault"))))
            .ReturnsAsync(new List<RetailPricelist> { existingDefault });

        // Mock: UpdateAsync for unsetting default
        _mockPricelistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<RetailPricelist>()))
            .Returns(Task.CompletedTask);

        // Mock: AddAsync for creating new pricelist
        _mockPricelistRepository
            .Setup(x => x.AddAsync(It.IsAny<RetailPricelist>()))
            .ReturnsAsync((RetailPricelist pl) =>
            {
                pl.Id = 1;
                return pl;
            });

        // Act
        var result = await _service.CreatePricelistAsync(dto);

        // Assert
        result.IsDefault.Should().BeTrue();

        // Verify that existing default was unset
        _mockPricelistRepository.Verify(x => x.UpdateAsync(It.Is<RetailPricelist>(p => p.Id == 5 && p.IsDefault == false)), Times.Once);
    }

    #endregion

    #region GetPricelistAsync Tests

    [Fact]
    public async Task GetPricelistByIdAsync_ExistingPricelist_ReturnsPricelist()
    {
        // Arrange
        var pricelistId = 5;
        var expectedPricelist = new RetailPricelist
        {
            Id = pricelistId,
            Name = "VIP Pricelist",
            Code = "VIP",
            IsActive = true
        };

        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(pricelistId))
            .ReturnsAsync(expectedPricelist);

        // Act
        var result = await _service.GetPricelistByIdAsync(pricelistId);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(pricelistId);
        result.Name.Should().Be("VIP Pricelist");
        result.Code.Should().Be("VIP");
    }

    [Fact]
    public async Task GetPricelistByIdAsync_NonExistingPricelist_ReturnsNull()
    {
        // Arrange
        var pricelistId = 999;
        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(pricelistId))
            .ReturnsAsync((RetailPricelist?)null);

        // Act
        var result = await _service.GetPricelistByIdAsync(pricelistId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetDefaultPricelistAsync_DefaultExists_ReturnsDefaultPricelist()
    {
        // Arrange
        var defaultPricelist = new RetailPricelist
        {
            Id = 1,
            Name = "Standard Retail",
            IsDefault = true,
            IsActive = true
        };

        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist> { defaultPricelist });

        // Act
        var result = await _service.GetDefaultPricelistAsync();

        // Assert
        result.Should().NotBeNull();
        result!.IsDefault.Should().BeTrue();
        result.Name.Should().Be("Standard Retail");
    }

    [Fact]
    public async Task GetDefaultPricelistAsync_NoDefault_ReturnsNull()
    {
        // Arrange
        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist>());

        // Act
        var result = await _service.GetDefaultPricelistAsync();

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllPricelistsAsync_ReturnsAllPricelists()
    {
        // Arrange
        var pricelists = new List<RetailPricelist>
        {
            new RetailPricelist { Id = 1, Name = "Pricelist 1", IsActive = true },
            new RetailPricelist { Id = 2, Name = "Pricelist 2", IsActive = true },
            new RetailPricelist { Id = 3, Name = "Pricelist 3", IsActive = false }
        };

        _mockPricelistRepository
            .Setup(x => x.GetAllAsync())
            .ReturnsAsync(pricelists);

        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(new List<RetailPricelistItem>());

        // Act
        var result = await _service.GetAllPricelistsAsync();

        // Assert
        result.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetEffectivePricelistsAsync_CurrentDate_ReturnsEffectivePricelists()
    {
        // Arrange
        var currentDate = DateTime.Today;
        var effectivePricelists = new List<RetailPricelist>
        {
            new RetailPricelist
            {
                Id = 1,
                Name = "Current Promotion",
                EffectiveFrom = currentDate.AddDays(-7),
                EffectiveTo = currentDate.AddDays(7), // Active for 2 weeks
                IsActive = true
            },
            new RetailPricelist
            {
                Id = 2,
                Name = "Standard Pricing",
                EffectiveFrom = currentDate.AddMonths(-6),
                EffectiveTo = null, // No end date
                IsActive = true
            }
        };

        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(effectivePricelists);

        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(new List<RetailPricelistItem>());

        // Act
        var result = await _service.GetEffectivePricelistsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Current Promotion");
        result.Should().Contain(p => p.Name == "Standard Pricing");
    }

    #endregion

    #region UpdatePricelistAsync Tests

    [Fact]
    public async Task UpdatePricelistAsync_ValidData_UpdatesSuccessfully()
    {
        // Arrange
        var dto = new UpdatePricelistDto
        {
            Id = 1,
            Name = "Updated Pricelist",
            Description = "Updated description",
            IsActive = true,
            Priority = 5
        };

        var existingPricelist = new RetailPricelist
        {
            Id = 1,
            Name = "Old Name",
            Description = "Old description",
            IsActive = false,
            Priority = 1
        };

        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(dto.Id))
            .ReturnsAsync(existingPricelist);

        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist>()); // No duplicates

        _mockPricelistRepository
            .Setup(x => x.UpdateAsync(It.IsAny<RetailPricelist>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdatePricelistAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Pricelist");
        result.Description.Should().Be("Updated description");
        result.IsActive.Should().BeTrue();
        result.Priority.Should().Be(5);

        _mockPricelistRepository.Verify(x => x.UpdateAsync(It.IsAny<RetailPricelist>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePricelistAsync_NonExistingPricelist_ThrowsInvalidOperationException()
    {
        // Arrange
        var dto = new UpdatePricelistDto
        {
            Id = 999,
            Name = "Non-existent"
        };

        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(dto.Id))
            .ReturnsAsync((RetailPricelist?)null);

        // Act
        Func<Task> act = async () => await _service.UpdatePricelistAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"*{dto.Id}*not found*");

        _mockPricelistRepository.Verify(x => x.UpdateAsync(It.IsAny<RetailPricelist>()), Times.Never);
    }

    #endregion

    #region DeactivatePricelistAsync Tests

    [Fact]
    public async Task DeactivatePricelistAsync_ValidId_DeactivatesSuccessfully()
    {
        // Arrange
        var pricelistId = 10;
        var existingPricelist = new RetailPricelist
        {
            Id = pricelistId,
            Name = "Pricelist to Deactivate",
            IsActive = true
        };

        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(pricelistId))
            .ReturnsAsync(existingPricelist);

        _mockPricelistRepository
            .Setup(x => x.DeleteAsync(pricelistId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeactivatePricelistAsync(pricelistId);

        // Assert
        _mockPricelistRepository.Verify(x => x.DeleteAsync(pricelistId), Times.Once);
    }

    #endregion

    #region PricelistItem Tests

    [Fact]
    public async Task AddProductToPricelistAsync_ValidData_AddsSuccessfully()
    {
        // Arrange
        var dto = new CreatePricelistItemDto
        {
            PricelistId = 1,
            ProductId = 5,
            Price = 250.00m,
            MinimumQuantity = 1,
            MaximumQuantity = null
        };

        var pricelist = new RetailPricelist { Id = 1, Name = "Test Pricelist", IsActive = true };

        _mockPricelistRepository
            .Setup(x => x.GetByIdAsync(dto.PricelistId))
            .ReturnsAsync(pricelist);

        // Mock: No duplicate item
        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(new List<RetailPricelistItem>());

        _mockPricelistItemRepository
            .Setup(x => x.AddAsync(It.IsAny<RetailPricelistItem>()))
            .ReturnsAsync((RetailPricelistItem item) =>
            {
                item.Id = 1;
                return item;
            });

        // Act
        var result = await _service.AddProductToPricelistAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.RetailPricelistId.Should().Be(1);
        result.ProductId.Should().Be(5);
        result.Price.Should().Be(250.00m);

        _mockPricelistItemRepository.Verify(x => x.AddAsync(It.IsAny<RetailPricelistItem>()), Times.Once);
    }

    [Fact]
    public async Task GetProductPriceInPricelistAsync_ProductInPricelist_ReturnsPrice()
    {
        // Arrange
        var pricelistId = 1;
        var productId = 5;
        var expectedPrice = 250.00m;

        var pricelistItem = new RetailPricelistItem
        {
            Id = 10,
            RetailPricelistId = pricelistId,
            ProductId = productId,
            Price = expectedPrice,
            MinimumQuantity = 1
        };

        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(new List<RetailPricelistItem> { pricelistItem });

        // Act
        var result = await _service.GetProductPriceInPricelistAsync(pricelistId, productId);

        // Assert
        result.Should().NotBeNull();
        result!.Price.Should().Be(expectedPrice);
    }

    [Fact]
    public async Task GetProductPriceInPricelistAsync_ProductNotInPricelist_ReturnsNull()
    {
        // Arrange
        var pricelistId = 1;
        var productId = 999; // Product not in this pricelist

        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(new List<RetailPricelistItem>());

        // Act
        var result = await _service.GetProductPriceInPricelistAsync(pricelistId, productId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetPricelistItemsAsync_ValidPricelistId_ReturnsItems()
    {
        // Arrange
        var pricelistId = 1;
        var items = new List<RetailPricelistItem>
        {
            new RetailPricelistItem { Id = 1, RetailPricelistId = pricelistId, ProductId = 1, Price = 100m },
            new RetailPricelistItem { Id = 2, RetailPricelistId = pricelistId, ProductId = 2, Price = 200m },
            new RetailPricelistItem { Id = 3, RetailPricelistId = pricelistId, ProductId = 3, Price = 300m }
        };

        _mockPricelistItemRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelistItem, bool>>>()))
            .ReturnsAsync(items);

        // Act
        var result = await _service.GetPricelistItemsAsync(pricelistId);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(item => item.PricelistId == pricelistId);
    }

    #endregion

    #region Pricing Strategy Tests

    [Fact]
    public async Task CreatePricelist_PercentageStrategy_StoresPercentageAdjustment()
    {
        // Arrange: VIP pricing with 20% discount
        var dto = new CreatePricelistDto
        {
            Name = "VIP - 20% Discount",
            Code = "VIP20",
            IsActive = true,
            PricingStrategy = "Percentage",
            PercentageAdjustment = -20.00m, // 20% discount
            Priority = 10
        };

        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist>());

        _mockPricelistRepository
            .Setup(x => x.AddAsync(It.IsAny<RetailPricelist>()))
            .ReturnsAsync((RetailPricelist pl) =>
            {
                pl.Id = 1;
                return pl;
            });

        // Act
        var result = await _service.CreatePricelistAsync(dto);

        // Assert
        result.PricingStrategy.Should().Be("Percentage");
        result.PercentageAdjustment.Should().Be(-20.00m);
    }

    [Fact]
    public async Task CreatePricelist_WholesaleStrategy_StoresWholesalePricing()
    {
        // Arrange: Wholesale pricing with 30% discount for bulk orders
        var dto = new CreatePricelistDto
        {
            Name = "Wholesale",
            Code = "WSL",
            IsActive = true,
            PricingStrategy = "Wholesale",
            PercentageAdjustment = -30.00m,
            Priority = 5
        };

        _mockPricelistRepository
            .Setup(x => x.FindAsync(It.IsAny<System.Linq.Expressions.Expression<System.Func<RetailPricelist, bool>>>()))
            .ReturnsAsync(new List<RetailPricelist>());

        _mockPricelistRepository
            .Setup(x => x.AddAsync(It.IsAny<RetailPricelist>()))
            .ReturnsAsync((RetailPricelist pl) =>
            {
                pl.Id = 1;
                return pl;
            });

        // Act
        var result = await _service.CreatePricelistAsync(dto);

        // Assert
        result.PricingStrategy.Should().Be("Wholesale");
        result.PercentageAdjustment.Should().Be(-30.00m);
        result.Priority.Should().Be(5);
    }

    #endregion
}
