using FluentAssertions;
using FluentValidation.TestHelper;
using Project420.Management.BLL.StockManagement.DTOs;
using Project420.Management.BLL.StockManagement.Validators;
using Project420.Shared.Core.Compliance;
using Xunit;

namespace Project420.Management.Tests.Validators;

/// <summary>
/// Unit tests for CreateProductValidator.
/// Tests cannabis compliance validation (THC/CBD, batch, expiry), pricing rules, and inventory validation.
/// </summary>
public class CreateProductValidatorTests
{
    private readonly CreateProductValidator _validator;

    public CreateProductValidatorTests()
    {
        _validator = new CreateProductValidator();
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
    [InlineData("PRE-ROLL-SATIVA-003")]
    [InlineData("EXTRACT123")]
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
        dto.Name = "A"; // Only 1 character

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
        dto.Name = new string('A', 201); // 201 characters

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Product name must be between 2 and 200 characters");
    }

    [Theory]
    [InlineData("Blue Dream")]
    [InlineData("OG Kush Pre-Roll")]
    [InlineData("CBD Oil 1000mg")]
    public void Validate_ValidProductNames_PassesValidation(string name)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Name = name;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    // ============================================================
    // DESCRIPTION VALIDATION TESTS
    // ============================================================

    [Fact]
    public void Validate_DescriptionExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Description = new string('A', 2001); // 2001 characters

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 2000 characters");
    }

    [Fact]
    public void Validate_EmptyDescription_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Description = "";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    // ============================================================
    // CANNABIS COMPLIANCE - THC/CBD VALIDATION
    // ============================================================

    [Fact]
    public void Validate_BothTHCAndCBDEmpty_FailsValidation()
    {
        // Arrange - Cannabis product MUST have THC and/or CBD specified
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
        dto.THCPercentage = "18%";
        dto.CBDPercentage = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Cannabis Compliance");
    }

    [Fact]
    public void Validate_OnlyCBDSpecified_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.THCPercentage = null;
        dto.CBDPercentage = "10%";

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor("Cannabis Compliance");
    }

    [Fact]
    public void Validate_BothTHCAndCBDSpecified_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.THCPercentage = "15%";
        dto.CBDPercentage = "8%";

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
        dto.BatchNumber = "InvalidBatch!@#"; // Contains invalid characters

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BatchNumber)
            .WithErrorMessage("Batch number format is invalid");
    }

    [Theory]
    [InlineData("BATCH-2024-12-001")]
    [InlineData("BATCH-2025-01-123")]
    [InlineData("BATCH-2024-11-0001")]
    public void Validate_ValidBatchNumberFormats_PassesValidation(string batchNumber)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.BatchNumber = batchNumber;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BatchNumber);
    }

    [Fact]
    public void Validate_EmptyBatchNumber_PassesValidation()
    {
        // Arrange - Batch number is optional
        var dto = CreateValidProductDto();
        dto.BatchNumber = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.BatchNumber);
    }

    // ============================================================
    // STRAIN NAME VALIDATION
    // ============================================================

    [Fact]
    public void Validate_StrainNameExceedsMaxLength_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.StrainName = new string('A', CannabisComplianceConstants.STRAIN_NAME_MAX_LENGTH + 1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StrainName)
            .WithErrorMessage($"Strain name cannot exceed {CannabisComplianceConstants.STRAIN_NAME_MAX_LENGTH} characters");
    }

    [Theory]
    [InlineData("Blue Dream")]
    [InlineData("OG Kush")]
    [InlineData("Charlotte's Web")]
    public void Validate_ValidStrainNames_PassesValidation(string strainName)
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.StrainName = strainName;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.StrainName);
    }

    // ============================================================
    // LAB TEST DATE VALIDATION
    // ============================================================

    [Fact]
    public void Validate_LabTestDateInFuture_FailsValidation()
    {
        // Arrange - Lab test cannot be dated in the future
        var dto = CreateValidProductDto();
        dto.LabTestDate = DateTime.Today.AddDays(1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.LabTestDate)
            .WithErrorMessage("Lab test date cannot be in the future");
    }

    [Fact]
    public void Validate_LabTestDateToday_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.LabTestDate = DateTime.Today;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LabTestDate);
    }

    [Fact]
    public void Validate_LabTestDateInPast_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.LabTestDate = DateTime.Today.AddDays(-30);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.LabTestDate);
    }

    [Fact]
    public void Validate_NoLabTestDate_PassesValidation()
    {
        // Arrange - Lab test date is optional
        var dto = CreateValidProductDto();
        dto.LabTestDate = null;

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
        // Arrange - Expiry date must be in the future
        var dto = CreateValidProductDto();
        dto.ExpiryDate = DateTime.Today.AddDays(-1);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
            .WithErrorMessage("Expiry date must be in the future");
    }

    [Fact]
    public void Validate_ExpiryDateToday_FailsValidation()
    {
        // Arrange - Today counts as expired
        var dto = CreateValidProductDto();
        dto.ExpiryDate = DateTime.Today;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExpiryDate)
            .WithErrorMessage("Expiry date must be in the future");
    }

    [Fact]
    public void Validate_ExpiryDateInFuture_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.ExpiryDate = DateTime.Today.AddDays(30);

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    [Fact]
    public void Validate_NoExpiryDate_PassesValidation()
    {
        // Arrange - Expiry date is optional
        var dto = CreateValidProductDto();
        dto.ExpiryDate = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExpiryDate);
    }

    // ============================================================
    // PRICING VALIDATION
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
    public void Validate_PriceExceedsMaximum_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.Price = 1000000m; // Over R999,999.99

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price cannot exceed R999,999.99");
    }

    [Fact]
    public void Validate_PriceLessThanCostPrice_FailsValidation()
    {
        // Arrange - Price must be greater than cost price
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 50m; // Lower than cost

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price must be greater than cost price");
    }

    [Fact]
    public void Validate_PriceEqualToCostPrice_FailsValidation()
    {
        // Arrange - Price must be GREATER than cost price (not equal)
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 100m; // Equal to cost

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Selling price must be greater than cost price");
    }

    [Fact]
    public void Validate_ValidPrice_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CostPrice = 100m;
        dto.Price = 150m; // Valid markup

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
        dto.CostPrice = 1000000m; // Over R999,999.99

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CostPrice)
            .WithErrorMessage("Cost price cannot exceed R999,999.99");
    }

    [Fact]
    public void Validate_CostPriceZero_PassesValidation()
    {
        // Arrange - Free samples, promotional items, etc.
        var dto = CreateValidProductDto();
        dto.CostPrice = 0m;
        dto.Price = 10m;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CostPrice);
    }

    // ============================================================
    // INVENTORY/STOCK VALIDATION
    // ============================================================

    [Fact]
    public void Validate_StockOnHandNegative_FailsValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.StockOnHand = -10;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.StockOnHand)
            .WithErrorMessage("Stock on hand cannot be negative");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Validate_ValidStockOnHand_PassesValidation(int stock)
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
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(50)]
    public void Validate_ValidReorderLevel_PassesValidation(int reorderLevel)
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
    // CATEGORY VALIDATION
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
    public void Validate_CategoryIdPositive_PassesValidation()
    {
        // Arrange
        var dto = CreateValidProductDto();
        dto.CategoryId = 1;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    [Fact]
    public void Validate_CategoryIdNull_PassesValidation()
    {
        // Arrange - Category is optional
        var dto = CreateValidProductDto();
        dto.CategoryId = null;

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CategoryId);
    }

    // ============================================================
    // FULL PRODUCT VALIDATION (REAL-WORLD SCENARIOS)
    // ============================================================

    [Fact]
    public void Validate_CompleteValidProduct_PassesAllValidation()
    {
        // Arrange - Create a fully compliant cannabis product
        var dto = new CreateProductDto
        {
            SKU = "CBD-OIL-1000",
            Name = "CBD Oil 1000mg",
            Description = "High-quality CBD oil extracted from organic hemp. 1000mg concentration.",
            THCPercentage = "0.2%",
            CBDPercentage = "10%",
            BatchNumber = "BATCH-2024-12-001",
            StrainName = "Charlotte's Web",
            LabTestDate = DateTime.Today.AddDays(-7),
            ExpiryDate = DateTime.Today.AddMonths(12),
            Price = 599.99m,
            CostPrice = 350.00m,
            StockOnHand = 50,
            ReorderLevel = 10,
            CategoryId = 1
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MinimalValidProduct_PassesValidation()
    {
        // Arrange - Minimal required fields only
        var dto = new CreateProductDto
        {
            SKU = "FLOWER-001",
            Name = "Indica Flower",
            THCPercentage = "18%", // Must have THC or CBD
            Price = 150.00m,
            CostPrice = 100.00m,
            StockOnHand = 10,
            ReorderLevel = 5
        };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Creates a valid product DTO for testing.
    /// Tests can modify specific fields to trigger validation errors.
    /// </summary>
    private CreateProductDto CreateValidProductDto()
    {
        return new CreateProductDto
        {
            SKU = "TEST-PRODUCT-001",
            Name = "Test Product",
            Description = "Test description",
            THCPercentage = "15%",
            CBDPercentage = "5%",
            BatchNumber = "BATCH-2024-12-001",
            StrainName = "Test Strain",
            LabTestDate = DateTime.Today.AddDays(-10),
            ExpiryDate = DateTime.Today.AddMonths(6),
            Price = 100.00m,
            CostPrice = 50.00m,
            StockOnHand = 10,
            ReorderLevel = 5,
            CategoryId = 1
        };
    }
}
