using FluentAssertions;
using FluentValidation.TestHelper;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.BLL.StockManagement.Validators;
using Project420.Shared.Core.Compliance;
using Xunit;

namespace Project420.Management.Tests.Validators;

/// <summary>
/// Unit tests for UpdateProductValidator.
/// Tests ID validation, cannabis compliance, pricing rules, and inventory validation for product updates.
/// </summary>
public class UpdateProductValidatorTests
{
    private readonly UpdateProductValidator _validator;

    public UpdateProductValidatorTests()
    {
        _validator = new UpdateProductValidator();
    }

    // ============================================================
    // ID VALIDATION TESTS (UNIQUE TO UPDATE)
    // ============================================================

    [Fact]
    public void Validate_IdZero_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Id = 0;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID must be greater than 0");
    }

    [Fact]
    public void Validate_IdNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Id = -1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id)
            .WithErrorMessage("Product ID must be greater than 0");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(99999)]
    public void Validate_ValidId_PassesValidation(int id)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Id = id;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    // ============================================================
    // SKU VALIDATION TESTS
    // ============================================================

    [Fact]
    public void Validate_EmptySKU_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.SKU = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU is required");
    }

    [Fact]
    public void Validate_SKUExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.SKU = new string('A', CannabisComplianceConstants.SKU_MAX_LENGTH + 1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage($"SKU cannot exceed {CannabisComplianceConstants.SKU_MAX_LENGTH} characters");
    }

    [Fact]
    public void Validate_SKUWithInvalidCharacters_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.SKU = "cbd-oil-001"; // lowercase not allowed

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SKU)
            .WithErrorMessage("SKU can only contain uppercase letters, numbers, hyphens, and underscores");
    }

    [Theory]
    [InlineData("CBD-OIL-001")]
    [InlineData("FLOWER_IND_002")]
    [InlineData("UPDATED-SKU-999")]
    public void Validate_ValidSKUFormats_PassesValidation(string sku)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.SKU = sku;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SKU);
    }

    // ============================================================
    // NAME VALIDATION TESTS
    // ============================================================

    [Fact]
    public void Validate_EmptyName_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Name = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name is required");
    }

    [Fact]
    public void Validate_NameTooShort_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Name = "A";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name must be between 2 and 200 characters");
    }

    [Fact]
    public void Validate_NameTooLong_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Name = new string('A', 201);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name must be between 2 and 200 characters");
    }

    // ============================================================
    // CANNABIS COMPLIANCE - THC/CBD VALIDATION
    // ============================================================

    [Fact]
    public void Validate_BothTHCAndCBDEmpty_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.THCPercentage = null;
        dto.CBDPercentage = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor("Cannabis Compliance")
            .WithErrorMessage(CannabisComplianceConstants.ValidationMessages.THC_CBD_REQUIRED);
    }

    [Fact]
    public void Validate_OnlyTHCSpecified_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.THCPercentage = "20%"; // Updated THC percentage
        dto.CBDPercentage = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Cannabis Compliance");
    }

    [Fact]
    public void Validate_THCExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.THCPercentage = new string('1', CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH + 1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.THCPercentage)
            .WithErrorMessage($"THC content cannot exceed {CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH} characters");
    }

    [Fact]
    public void Validate_CBDExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CBDPercentage = new string('1', CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH + 1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CBDPercentage)
            .WithErrorMessage($"CBD content cannot exceed {CannabisComplianceConstants.CANNABINOID_CONTENT_MAX_LENGTH} characters");
    }

    // ============================================================
    // BATCH NUMBER VALIDATION
    // ============================================================

    [Fact]
    public void Validate_BatchNumberExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.BatchNumber = new string('A', CannabisComplianceConstants.BATCH_NUMBER_MAX_LENGTH + 1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BatchNumber)
            .WithErrorMessage($"Batch number cannot exceed {CannabisComplianceConstants.BATCH_NUMBER_MAX_LENGTH} characters");
    }

    [Fact]
    public void Validate_InvalidBatchNumberFormat_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.BatchNumber = "InvalidBatch!@#";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BatchNumber)
            .WithErrorMessage("Batch number format is invalid");
    }

    [Theory]
    [InlineData("BATCH-2024-12-002")]
    [InlineData("BATCH-2025-01-999")]
    public void Validate_UpdatedBatchNumber_PassesValidation(string batchNumber)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.BatchNumber = batchNumber;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BatchNumber);
    }

    // ============================================================
    // LAB TEST DATE VALIDATION
    // ============================================================

    [Fact]
    public void Validate_LabTestDateInFuture_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.LabTestDate = DateTime.Today.AddDays(1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LabTestDate)
            .WithErrorMessage("Lab test date cannot be in the future");
    }

    [Fact]
    public void Validate_UpdatedLabTestDateToday_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.LabTestDate = DateTime.Today;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LabTestDate);
    }

    // ============================================================
    // EXPIRY DATE VALIDATION
    // ============================================================

    [Fact]
    public void Validate_ExpiryDateInPast_FailsValidation()
    {
        // Arrange - Cannot update expiry to past date
        var dto = CreateValidProductDto();
        dto.ExpiryDate = DateTime.Today.AddDays(-1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
            .WithErrorMessage("Expiry date must be in the future");
    }

    [Fact]
    public void Validate_ExtendedExpiryDate_PassesValidation()
    {
        // Arrange - Extending expiry date
        var dto = CreateValidProductDto();
        dto.ExpiryDate = DateTime.Today.AddMonths(12);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    // ============================================================
    // PRICING VALIDATION (UPDATE SCENARIOS)
    // ============================================================

    [Fact]
    public void Validate_PriceZero_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Price = 0m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price must be greater than R0");
    }

    [Fact]
    public void Validate_PriceNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Price = -10m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price must be greater than R0");
    }

    [Fact]
    public void Validate_PriceLessThanCostPrice_FailsValidation()
    {
        // Arrange - Cannot update price below cost
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 50m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price must be greater than cost price");
    }

    [Fact]
    public void Validate_PriceIncrease_PassesValidation()
    {
        // Arrange - Price increase scenario
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 175m; // Increased from 150 to 175

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_PriceDecrease_PassesValidation()
    {
        // Arrange - Price decrease (sale/clearance)
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 125m; // Decreased from 150 to 125 (still above cost)

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_CostPriceNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CostPrice = -10m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CostPrice)
            .WithErrorMessage("Cost price cannot be negative");
    }

    [Fact]
    public void Validate_CostPriceExceedsMaximum_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CostPrice = 1000000m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CostPrice)
            .WithErrorMessage("Cost price cannot exceed R999,999.99");
    }

    // ============================================================
    // INVENTORY/STOCK VALIDATION (UPDATE SCENARIOS)
    // ============================================================

    [Fact]
    public void Validate_StockOnHandNegative_FailsValidation()
    {
        // Arrange - Cannot have negative stock
        var dto = CreateValidProductDto();
        dto.StockOnHand = -10;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockOnHand)
            .WithErrorMessage("Stock on hand cannot be negative");
    }

    [Theory]
    [InlineData(0)]  // Out of stock
    [InlineData(5)]  // Low stock
    [InlineData(100)] // Restocked
    public void Validate_UpdatedStockLevels_PassesValidation(int stock)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.StockOnHand = stock;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StockOnHand);
    }

    [Fact]
    public void Validate_ReorderLevelNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.ReorderLevel = -5;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ReorderLevel)
            .WithErrorMessage("Reorder level cannot be negative");
    }

    [Theory]
    [InlineData(5)]   // Decreased reorder level
    [InlineData(20)]  // Increased reorder level
    public void Validate_UpdatedReorderLevel_PassesValidation(int reorderLevel)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.ReorderLevel = reorderLevel;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ReorderLevel);
    }

    // ============================================================
    // CATEGORY VALIDATION (UPDATE SCENARIOS)
    // ============================================================

    [Fact]
    public void Validate_CategoryIdZero_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CategoryId = 0;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID must be a positive number");
    }

    [Fact]
    public void Validate_CategoryIdNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CategoryId = -1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CategoryId)
            .WithErrorMessage("Category ID must be a positive number");
    }

    [Fact]
    public void Validate_CategoryChange_PassesValidation()
    {
        // Arrange - Changing product category
        var dto = CreateValidProductDto();
        dto.CategoryId = 5; // Changed from 1 to 5

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_RemoveCategory_PassesValidation()
    {
        // Arrange - Removing category assignment
        var dto = CreateValidProductDto();
        dto.CategoryId = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    // ============================================================
    // FULL PRODUCT UPDATE VALIDATION
    // ============================================================

    [Fact]
    public void Validate_CompleteProductUpdate_PassesAllValidation()
    {
        // Arrange - Complete product update scenario
        var dto = new UpdateProductDto
        {
            Id = 123,
            SKU = "CBD-OIL-1000-V2",
            Name = "CBD Oil 1000mg (Updated Formula)",
            Description = "Updated high-quality CBD oil with improved extraction method.",
            THCPercentage = "0.3%", // Updated
            CBDPercentage = "12%", // Increased
            BatchNumber = "BATCH-2024-12-099",
            StrainName = "Charlotte's Web",
            LabTestDate = DateTime.Today.AddDays(-3), // Recent retest
            ExpiryDate = DateTime.Today.AddMonths(18), // Extended shelf life
            Price = 649.99m, // Price increase
            CostPrice = 375.00m, // Updated cost
            StockOnHand = 75, // Restocked
            ReorderLevel = 15, // Adjusted
            CategoryId = 2 // Category changed
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MinimalProductUpdate_PassesValidation()
    {
        // Arrange - Minimal required fields for update
        var dto = new UpdateProductDto
        {
            Id = 1,
            SKU = "FLOWER-002",
            Name = "Updated Sativa",
            THCPercentage = "22%", // Increased potency
            Price = 180.00m,
            CostPrice = 120.00m,
            StockOnHand = 15,
            ReorderLevel = 5
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_PriceOnlyUpdate_PassesValidation()
    {
        // Arrange - Price adjustment only
        var dto = CreateValidProductDto();
        dto.Price = 165.00m; // Adjusted price

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_StockOnlyUpdate_PassesValidation()
    {
        // Arrange - Stock adjustment after receiving/sale
        var dto = CreateValidProductDto();
        dto.StockOnHand = 25; // Updated stock level

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Creates a valid update product DTO for testing.
    /// Tests can modify specific fields to trigger validation errors.
    /// </summary>
    private UpdateProductDto CreateValidProductDto()
    {
        return new UpdateProductDto
        {
            Id = 1,
            SKU = "TEST-PRODUCT-001",
            Name = "Test Product",
            Description = "Test description",
            THCPercentage = "15%",
            CBDPercentage = "5%",
            BatchNumber = "BATCH-2024-12-001",
            StrainName = "Test Strain",
            LabTestDate = DateTime.Today.AddDays(-10),
            ExpiryDate = DateTime.Today.AddMonths(6),
            Price = 150.00m,
            CostPrice = 100.00m,
            StockOnHand = 10,
            ReorderLevel = 5,
            CategoryId = 1
        };
    }
}
