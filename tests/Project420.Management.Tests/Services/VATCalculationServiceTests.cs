using FluentAssertions;
using Project420.Shared.Infrastructure.DTOs;
using Project420.Shared.Infrastructure.Services;

namespace Project420.Management.Tests.Services;

/// <summary>
/// Comprehensive unit tests for VATCalculationService.
/// Tests South African VAT calculations (15% VAT rate) with rounding accuracy.
/// CRITICAL: This service is the foundation for ALL financial transactions.
/// </summary>
public class VATCalculationServiceTests
{
    private readonly VATCalculationService _service;

    public VATCalculationServiceTests()
    {
        _service = new VATCalculationService();
    }

    #region Line Item Calculation Tests

    [Fact]
    public void CalculateLineItem_SingleItem_ReturnsCorrectBreakdown()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 1;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(10.00m);
        result.Subtotal.Should().Be(8.70m);  // 10.00 / 1.15 = 8.6956... rounds to 8.70
        result.TaxAmount.Should().Be(1.30m); // 10.00 - 8.70
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_MultipleQuantity_ReturnsCorrectBreakdown()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 5;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Total.Should().Be(50.00m);
        result.Subtotal.Should().Be(43.48m);  // 50.00 / 1.15 = 43.478... rounds to 43.48
        result.TaxAmount.Should().Be(6.52m);  // 50.00 - 43.48
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_LargeQuantity_HandlesRoundingCorrectly()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 100;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Total.Should().Be(1000.00m);
        result.Subtotal.Should().Be(869.57m);  // 1000.00 / 1.15 = 869.5652... rounds to 869.57
        result.TaxAmount.Should().Be(130.43m); // 1000.00 - 869.57
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_OddPrice_RoundsCorrectly()
    {
        // Arrange: Cannabis product priced at R123.45
        decimal unitPriceInclVAT = 123.45m;
        int quantity = 1;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Total.Should().Be(123.45m);
        result.Subtotal.Should().Be(107.35m);  // 123.45 / 1.15 = 107.3478... rounds to 107.35
        result.TaxAmount.Should().Be(16.10m);  // 123.45 - 107.35
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_SmallPrice_RoundsCorrectly()
    {
        // Arrange: R1.00 item (e.g., rolling papers)
        decimal unitPriceInclVAT = 1.00m;
        int quantity = 1;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Total.Should().Be(1.00m);
        result.Subtotal.Should().Be(0.87m);  // 1.00 / 1.15 = 0.8696... rounds to 0.87
        result.TaxAmount.Should().Be(0.13m); // 1.00 - 0.87
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_ZeroQuantity_ReturnsZeroBreakdown()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 0;

        // Act
        var result = _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        result.Total.Should().Be(0.00m);
        result.Subtotal.Should().Be(0.00m);
        result.TaxAmount.Should().Be(0.00m);
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItem_NegativePrice_ThrowsException()
    {
        // Arrange
        decimal unitPriceInclVAT = -10.00m;
        int quantity = 1;

        // Act
        Action act = () => _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Unit price cannot be negative*");
    }

    [Fact]
    public void CalculateLineItem_NegativeQuantity_ThrowsException()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = -5;

        // Act
        Action act = () => _service.CalculateLineItem(unitPriceInclVAT, quantity);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Quantity cannot be negative*");
    }

    #endregion

    #region Line Item with Discount Tests

    [Fact]
    public void CalculateLineItemWithDiscount_FixedDiscount_ReturnsCorrectBreakdown()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 1;
        decimal discountAmount = 2.00m;

        // Act
        var result = _service.CalculateLineItemWithDiscount(unitPriceInclVAT, quantity, discountAmount);

        // Assert
        result.Total.Should().Be(8.00m);       // 10.00 - 2.00
        result.Subtotal.Should().Be(6.96m);    // 8.00 / 1.15 = 6.9565... rounds to 6.96
        result.TaxAmount.Should().Be(1.04m);   // 8.00 - 6.96
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateLineItemWithDiscount_DiscountExceedsTotal_ReturnsZero()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 1;
        decimal discountAmount = 15.00m;

        // Act
        var result = _service.CalculateLineItemWithDiscount(unitPriceInclVAT, quantity, discountAmount);

        // Assert
        result.Total.Should().Be(0.00m);
        result.Subtotal.Should().Be(0.00m);
        result.TaxAmount.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateLineItemWithDiscount_NegativeDiscount_ThrowsException()
    {
        // Arrange
        decimal unitPriceInclVAT = 10.00m;
        int quantity = 1;
        decimal discountAmount = -2.00m;

        // Act
        Action act = () => _service.CalculateLineItemWithDiscount(unitPriceInclVAT, quantity, discountAmount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Discount amount cannot be negative*");
    }

    #endregion

    #region VAT Amount and Subtotal Extraction Tests

    [Theory]
    [InlineData(10.00, 1.30)]    // Standard case
    [InlineData(100.00, 13.04)]  // Larger amount
    [InlineData(1.00, 0.13)]     // Small amount
    [InlineData(0.00, 0.00)]     // Zero
    public void CalculateVATAmount_VariousAmounts_ReturnsCorrectVAT(decimal amountInclVAT, decimal expectedVAT)
    {
        // Act
        var result = _service.CalculateVATAmount(amountInclVAT);

        // Assert
        result.Should().Be(expectedVAT);
    }

    [Theory]
    [InlineData(10.00, 8.70)]    // Standard case
    [InlineData(100.00, 86.96)]  // Larger amount
    [InlineData(1.00, 0.87)]     // Small amount
    [InlineData(0.00, 0.00)]     // Zero
    public void CalculateSubtotal_VariousAmounts_ReturnsCorrectSubtotal(decimal amountInclVAT, decimal expectedSubtotal)
    {
        // Act
        var result = _service.CalculateSubtotal(amountInclVAT);

        // Assert
        result.Should().Be(expectedSubtotal);
    }

    [Fact]
    public void CalculateVATAmount_NegativeAmount_ThrowsException()
    {
        // Arrange
        decimal amountInclVAT = -10.00m;

        // Act
        Action act = () => _service.CalculateVATAmount(amountInclVAT);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void CalculateSubtotal_NegativeAmount_ThrowsException()
    {
        // Arrange
        decimal amountInclVAT = -10.00m;

        // Act
        Action act = () => _service.CalculateSubtotal(amountInclVAT);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Header Aggregation Tests

    [Fact]
    public void CalculateHeaderTotals_MultipleLines_AggregatesCorrectly()
    {
        // Arrange: 3 line items
        var lineItems = new List<VATBreakdown>
        {
            _service.CalculateLineItem(10.00m, 1),  // Total: R10.00
            _service.CalculateLineItem(20.00m, 2),  // Total: R40.00
            _service.CalculateLineItem(5.00m, 3)    // Total: R15.00
        };

        // Act
        var result = _service.CalculateHeaderTotals(lineItems);

        // Assert
        result.Total.Should().Be(65.00m);     // 10 + 40 + 15
        result.Subtotal.Should().Be(56.52m);  // Sum of individual subtotals
        result.TaxAmount.Should().Be(8.48m);  // Sum of individual taxes
        result.IsValid().Should().BeTrue();
    }

    [Fact]
    public void CalculateHeaderTotals_100Lines_HandlesRoundingCorrectly()
    {
        // Arrange: 100 line items @ R10.00 each
        var lineItems = Enumerable.Range(1, 100)
            .Select(_ => _service.CalculateLineItem(10.00m, 1))
            .ToList();

        // Act
        var result = _service.CalculateHeaderTotals(lineItems);

        // Assert
        result.Total.Should().Be(1000.00m);    // 100 × R10.00
        // When aggregating 100 lines @ R8.70 subtotal each = R870.00 (not R869.57)
        // This is correct: sum of individually rounded values vs calculating total first
        result.Subtotal.Should().Be(870.00m);  // Sum of 100 × R8.70
        result.TaxAmount.Should().Be(130.00m); // Sum of 100 × R1.30
        result.IsValid().Should().BeTrue();

        // Verify variance is acceptable (< 1 cent after aggregation)
        Math.Abs(result.RoundingAdjustment).Should().BeLessThan(0.01m);
    }

    [Fact]
    public void CalculateHeaderTotals_EmptyList_ReturnsZeroBreakdown()
    {
        // Arrange
        var lineItems = new List<VATBreakdown>();

        // Act
        var result = _service.CalculateHeaderTotals(lineItems);

        // Assert
        result.Total.Should().Be(0.00m);
        result.Subtotal.Should().Be(0.00m);
        result.TaxAmount.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateHeaderTotals_NullList_ReturnsZeroBreakdown()
    {
        // Arrange
        IEnumerable<VATBreakdown>? lineItems = null;

        // Act
        var result = _service.CalculateHeaderTotals(lineItems!);

        // Assert
        result.Total.Should().Be(0.00m);
        result.Subtotal.Should().Be(0.00m);
        result.TaxAmount.Should().Be(0.00m);
    }

    #endregion

    #region Discount Calculation Tests

    [Theory]
    [InlineData(100.00, 10, 90.00)]   // 10% discount
    [InlineData(100.00, 50, 50.00)]   // 50% discount
    [InlineData(100.00, 100, 0.00)]   // 100% discount
    [InlineData(100.00, 0, 100.00)]   // 0% discount (no change)
    public void ApplyPercentageDiscount_VariousPercentages_ReturnsCorrectAmount(
        decimal amount, decimal percentage, decimal expected)
    {
        // Act
        var result = _service.ApplyPercentageDiscount(amount, percentage);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyPercentageDiscount_NegativePercentage_ThrowsException()
    {
        // Arrange
        decimal amount = 100.00m;
        decimal percentage = -10m;

        // Act
        Action act = () => _service.ApplyPercentageDiscount(amount, percentage);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Discount percentage must be between 0 and 100*");
    }

    [Fact]
    public void ApplyPercentageDiscount_PercentageOver100_ThrowsException()
    {
        // Arrange
        decimal amount = 100.00m;
        decimal percentage = 150m;

        // Act
        Action act = () => _service.ApplyPercentageDiscount(amount, percentage);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Discount percentage must be between 0 and 100*");
    }

    [Theory]
    [InlineData(100.00, 10.00, 90.00)]   // R10 discount
    [InlineData(100.00, 50.00, 50.00)]   // R50 discount
    [InlineData(100.00, 100.00, 0.00)]   // Full discount
    [InlineData(100.00, 150.00, 0.00)]   // Discount exceeds amount (capped at 0)
    public void ApplyFixedDiscount_VariousAmounts_ReturnsCorrectAmount(
        decimal amount, decimal discountAmount, decimal expected)
    {
        // Act
        var result = _service.ApplyFixedDiscount(amount, discountAmount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyFixedDiscount_NegativeDiscount_ThrowsException()
    {
        // Arrange
        decimal amount = 100.00m;
        decimal discountAmount = -10.00m;

        // Act
        Action act = () => _service.ApplyFixedDiscount(amount, discountAmount);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Discount amount cannot be negative*");
    }

    #endregion

    #region Utility Method Tests

    [Theory]
    [InlineData(10.123, 10.12)]      // Round down
    [InlineData(10.126, 10.13)]      // Round up
    [InlineData(10.125, 10.13)]      // Midpoint rounds away from zero
    [InlineData(-10.125, -10.13)]    // Negative midpoint rounds away from zero
    public void RoundToNearestCent_VariousAmounts_RoundsCorrectly(decimal input, decimal expected)
    {
        // Act
        var result = _service.RoundToNearestCent(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GetCurrentVATRate_ReturnsCorrectRate()
    {
        // Act
        var result = _service.GetCurrentVATRate();

        // Assert
        result.Should().Be(0.15m); // 15% VAT
    }

    [Fact]
    public void CalculateRoundingAdjustment_CalculatesVariance()
    {
        // Arrange
        decimal expectedTotal = 100.00m;
        decimal calculatedTotal = 99.99m;

        // Act
        var result = _service.CalculateRoundingAdjustment(expectedTotal, calculatedTotal);

        // Assert
        result.Should().Be(0.01m);
    }

    #endregion

    #region Integration/Real-World Scenarios

    [Fact]
    public void RealWorldScenario_CannabisRetailSale_CalculatesAccurately()
    {
        // Arrange: Typical cannabis retail sale
        // - 3.5g Blue Dream @ R350.00
        // - Pre-roll @ R50.00
        // - Grinder @ R120.00
        var lineItems = new List<VATBreakdown>
        {
            _service.CalculateLineItem(350.00m, 1),  // Cannabis: Subtotal R304.35, VAT R45.65
            _service.CalculateLineItem(50.00m, 1),   // Pre-roll: Subtotal R43.48, VAT R6.52
            _service.CalculateLineItem(120.00m, 1)   // Accessory: Subtotal R104.35, VAT R15.65
        };

        // Act
        var result = _service.CalculateHeaderTotals(lineItems);

        // Assert
        result.Total.Should().Be(520.00m);      // Total sale
        result.Subtotal.Should().Be(452.18m);   // Sum of individual subtotals (304.35 + 43.48 + 104.35)
        result.TaxAmount.Should().Be(67.82m);   // Sum of individual VAT amounts (45.65 + 6.52 + 15.65)
        result.IsValid().Should().BeTrue();

        // Verify rounding variance is minimal
        Math.Abs(result.RoundingAdjustment).Should().BeLessThan(0.01m);
    }

    [Fact]
    public void RealWorldScenario_LargeWholesaleOrder_HandlesAccurately()
    {
        // Arrange: Wholesale order of 500 units @ R10.00 each
        var lineItems = new List<VATBreakdown>
        {
            _service.CalculateLineItem(10.00m, 500)
        };

        // Act
        var result = _service.CalculateHeaderTotals(lineItems);

        // Assert
        result.Total.Should().Be(5000.00m);
        result.Subtotal.Should().Be(4347.83m);  // 5000 / 1.15 = 4347.826... rounds to 4347.83
        result.TaxAmount.Should().Be(652.17m);  // 5000 - 4347.83
        result.IsValid().Should().BeTrue();
    }

    #endregion
}
