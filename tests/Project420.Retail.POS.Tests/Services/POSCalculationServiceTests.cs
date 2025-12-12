using FluentAssertions;
using Project420.Retail.POS.BLL.Services;
using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.Tests.Services;

/// <summary>
/// Comprehensive tests for POSCalculationService
/// Tests SA VAT compliance (15%), rounding, and calculation accuracy
/// </summary>
public class POSCalculationServiceTests
{
    private readonly POSCalculationService _service;

    public POSCalculationServiceTests()
    {
        _service = new POSCalculationService();
    }

    #region Line Item Calculations

    [Theory]
    [InlineData(100.00, 1, 86.96, 13.04, 100.00)] // Single item at R100
    [InlineData(115.00, 1, 100.00, 15.00, 115.00)] // Single item at R115
    [InlineData(100.00, 2, 173.91, 26.09, 200.00)] // Two items at R100 each
    [InlineData(50.00, 3, 130.43, 19.57, 150.00)] // Three items at R50 each
    [InlineData(23.00, 1, 20.00, 3.00, 23.00)] // R23 item
    public void CalculateLineItem_WithVariousPrices_CalculatesCorrectly(
        decimal unitPrice, int quantity, decimal expectedSubtotal, decimal expectedVAT, decimal expectedTotal)
    {
        // Act
        var result = _service.CalculateLineItem(unitPrice, quantity);

        // Assert
        result.Should().NotBeNull();
        (result.LineTotal - result.VATAmount).Should().Be(expectedSubtotal); // Subtotal = LineTotal - VAT
        result.VATAmount.Should().Be(expectedVAT);
        result.LineTotal.Should().Be(expectedTotal);
        result.Quantity.Should().Be(quantity);
        result.UnitPrice.Should().Be(unitPrice);
    }

    [Fact]
    public void CalculateLineItem_SingleItemR100_CalculatesCorrectly()
    {
        // Arrange
        decimal unitPrice = 100.00m;
        int quantity = 1;

        // Act
        var result = _service.CalculateLineItem(unitPrice, quantity);

        // Assert
        result.LineTotal.Should().Be(100.00m);
        (result.LineTotal - result.VATAmount).Should().Be(86.96m);
        result.VATAmount.Should().Be(13.04m);
    }

    [Fact]
    public void CalculateLineItem_RealWorldScenario_R700Sale_CalculatesCorrectly()
    {
        // Real-world scenario from documentation
        // Arrange
        decimal unitPrice = 700.00m;
        int quantity = 1;

        // Act
        var result = _service.CalculateLineItem(unitPrice, quantity);

        // Assert
        result.LineTotal.Should().Be(700.00m);
        (result.LineTotal - result.VATAmount).Should().Be(608.70m);
        result.VATAmount.Should().Be(91.30m);
    }

    [Fact]
    public void CalculateLineSubtotal_WithVariousPrices_CalculatesCorrectly()
    {
        // Act & Assert
        _service.CalculateLineSubtotal(100.00m, 1).Should().Be(86.96m);
        _service.CalculateLineSubtotal(115.00m, 1).Should().Be(100.00m);
        _service.CalculateLineSubtotal(50.00m, 2).Should().Be(86.96m);
    }

    [Fact]
    public void CalculateLineVAT_WithVariousPrices_CalculatesCorrectly()
    {
        // Act & Assert
        _service.CalculateLineVAT(100.00m).Should().Be(13.04m);
        _service.CalculateLineVAT(115.00m).Should().Be(15.00m);
        _service.CalculateLineVAT(200.00m).Should().Be(26.09m);
    }

    #endregion

    #region Header Calculations (Aggregation)

    [Fact]
    public void CalculateHeaderTotals_WithNoDetails_SetsZeroTotals()
    {
        // Arrange
        var header = new RetailTransactionHeader
        {
            TransactionDetails = new List<TransactionDetail>()
        };

        // Act
        _service.CalculateHeaderTotals(header);

        // Assert
        header.Subtotal.Should().Be(0.00m);
        header.TaxAmount.Should().Be(0.00m);
        header.TotalAmount.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateHeaderTotals_WithSingleDetail_AggregatesCorrectly()
    {
        // Arrange
        var header = new RetailTransactionHeader
        {
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    VATAmount = 13.04m,
                    LineTotal = 100.00m // Subtotal = 100 - 13.04 = 86.96
                }
            }
        };

        // Act
        _service.CalculateHeaderTotals(header);

        // Assert
        header.Subtotal.Should().Be(86.96m);
        header.TaxAmount.Should().Be(13.04m);
        header.TotalAmount.Should().Be(100.00m);
    }

    [Fact]
    public void CalculateHeaderTotals_WithMultipleDetails_AggregatesCorrectly()
    {
        // Arrange
        var header = new RetailTransactionHeader
        {
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail { VATAmount = 13.04m, LineTotal = 100.00m }, // Subtotal = 86.96
                new TransactionDetail { VATAmount = 6.52m, LineTotal = 50.00m },   // Subtotal = 43.48
                new TransactionDetail { VATAmount = 19.57m, LineTotal = 150.00m }  // Subtotal = 130.43
            }
        };

        // Act
        _service.CalculateHeaderTotals(header);

        // Assert
        header.Subtotal.Should().Be(260.87m);
        header.TaxAmount.Should().Be(39.13m);
        header.TotalAmount.Should().Be(300.00m);
    }

    [Fact]
    public void CalculateHeaderTotals_WithVarianceAboveThreshold_AdjustsVAT()
    {
        // Arrange
        var header = new RetailTransactionHeader
        {
            TransactionDetails = new List<TransactionDetail>
            {
                new TransactionDetail
                {
                    VATAmount = 13.03m, // Intentional variance (Subtotal = 100 - 13.03 = 86.97)
                    LineTotal = 100.00m
                }
            }
        };

        // Act
        _service.CalculateHeaderTotals(header);

        // Assert - VAT should be adjusted to compensate for variance
        header.Subtotal.Should().Be(86.97m);
        header.TotalAmount.Should().Be(100.00m);
        // VAT adjusted to maintain total accuracy
        (header.Subtotal + header.TaxAmount).Should().Be(header.TotalAmount);
    }

    [Fact]
    public void CalculateSubtotal_WithDetails_SumsCorrectly()
    {
        // Arrange - Subtotal = LineTotal - VATAmount
        var details = new List<TransactionDetail>
        {
            new TransactionDetail { LineTotal = 100.00m, VATAmount = 13.04m }, // Subtotal = 86.96
            new TransactionDetail { LineTotal = 50.00m, VATAmount = 6.52m }    // Subtotal = 43.48
        };

        // Act
        var result = _service.CalculateSubtotal(details);

        // Assert
        result.Should().Be(130.44m);
    }

    [Fact]
    public void CalculateSubtotal_WithNullDetails_ReturnsZero()
    {
        // Act
        var result = _service.CalculateSubtotal(null!);

        // Assert
        result.Should().Be(0.00m);
    }

    [Fact]
    public void CalculateTotalVAT_WithDetails_SumsCorrectly()
    {
        // Arrange
        var details = new List<TransactionDetail>
        {
            new TransactionDetail { VATAmount = 13.04m, LineTotal = 100.00m },
            new TransactionDetail { VATAmount = 6.52m, LineTotal = 50.00m }
        };

        // Act
        var result = _service.CalculateTotalVAT(details);

        // Assert
        result.Should().Be(19.56m);
    }

    [Fact]
    public void CalculateTotalAmount_WithDetails_SumsCorrectly()
    {
        // Arrange
        var details = new List<TransactionDetail>
        {
            new TransactionDetail { LineTotal = 100.00m, VATAmount = 13.04m },
            new TransactionDetail { LineTotal = 50.00m, VATAmount = 6.52m }
        };

        // Act
        var result = _service.CalculateTotalAmount(details);

        // Assert
        result.Should().Be(150.00m);
    }

    #endregion

    #region Discount Calculations

    [Theory]
    [InlineData(100.00, 10, 90.00)] // 10% off R100 = R90
    [InlineData(100.00, 50, 50.00)] // 50% off R100 = R50
    [InlineData(200.00, 25, 150.00)] // 25% off R200 = R150
    [InlineData(75.50, 15, 64.18)] // 15% off R75.50 = R64.18
    public void ApplyPercentageDiscount_WithValidPercentage_CalculatesCorrectly(
        decimal amount, decimal percentage, decimal expected)
    {
        // Act
        var result = _service.ApplyPercentageDiscount(amount, percentage);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyPercentageDiscount_WithNegativePercentage_ThrowsException()
    {
        // Act & Assert
        _service.Invoking(s => s.ApplyPercentageDiscount(100.00m, -10))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Percentage must be between 0 and 100*");
    }

    [Fact]
    public void ApplyPercentageDiscount_WithPercentageOver100_ThrowsException()
    {
        // Act & Assert
        _service.Invoking(s => s.ApplyPercentageDiscount(100.00m, 150))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Percentage must be between 0 and 100*");
    }

    [Theory]
    [InlineData(100.00, 10.00, 90.00)] // R100 - R10 = R90
    [InlineData(150.00, 25.50, 124.50)] // R150 - R25.50 = R124.50
    [InlineData(75.00, 5.25, 69.75)] // R75 - R5.25 = R69.75
    public void ApplyFixedDiscount_WithValidAmount_CalculatesCorrectly(
        decimal amount, decimal discount, decimal expected)
    {
        // Act
        var result = _service.ApplyFixedDiscount(amount, discount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ApplyFixedDiscount_WithNegativeDiscount_ThrowsException()
    {
        // Act & Assert
        _service.Invoking(s => s.ApplyFixedDiscount(100.00m, -10.00m))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Discount amount cannot be negative*");
    }

    [Fact]
    public void ApplyFixedDiscount_WithDiscountExceedingAmount_ThrowsException()
    {
        // Act & Assert
        _service.Invoking(s => s.ApplyFixedDiscount(100.00m, 150.00m))
            .Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*Discount amount cannot exceed original amount*");
    }

    #endregion

    #region Rounding and Variance

    [Theory]
    [InlineData(100.123, 100.12)] // Round down
    [InlineData(100.125, 100.13)] // Round up (away from zero)
    [InlineData(100.124, 100.12)] // Round down
    [InlineData(100.126, 100.13)] // Round up
    [InlineData(99.999, 100.00)] // Round up
    public void RoundToNearestCent_WithVariousAmounts_RoundsCorrectly(decimal amount, decimal expected)
    {
        // Act
        var result = _service.RoundToNearestCent(amount);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void RoundToNearestCent_UsesMidpointRoundingAwayFromZero()
    {
        // Test MidpointRounding.AwayFromZero behavior
        _service.RoundToNearestCent(0.125m).Should().Be(0.13m);
        _service.RoundToNearestCent(0.115m).Should().Be(0.12m);
        _service.RoundToNearestCent(0.135m).Should().Be(0.14m);
    }

    [Theory]
    [InlineData(100.00, 99.99, 0.01)] // 1 cent variance
    [InlineData(150.00, 149.95, 0.05)] // 5 cent variance
    [InlineData(200.50, 200.47, 0.03)] // 3 cent variance
    public void CalculateRoundingAdjustment_WithVariance_CalculatesCorrectly(
        decimal expected, decimal calculated, decimal adjustment)
    {
        // Act
        var result = _service.CalculateRoundingAdjustment(expected, calculated);

        // Assert
        result.Should().Be(adjustment);
    }

    #endregion

    #region Real-World SA Retail Scenarios

    [Fact]
    public void RealWorld_Cannabis_R700Sale_CorrectVATBreakdown()
    {
        // Real-world scenario: R700 cannabis sale
        // Expected: R608.70 (subtotal) + R91.30 (VAT) = R700 (total)

        // Act
        var result = _service.CalculateLineItem(700.00m, 1);

        // Assert
        result.LineTotal.Should().Be(700.00m);
        (result.LineTotal - result.VATAmount).Should().Be(608.70m); // Subtotal
        result.VATAmount.Should().Be(91.30m);

        // Verify: Subtotal + VAT = Total
        ((result.LineTotal - result.VATAmount) + result.VATAmount).Should().Be(result.LineTotal);
    }

    [Fact]
    public void RealWorld_MultiItem_R435Sale_CorrectVATBreakdown()
    {
        // Real-world scenario: Multiple items totaling R435
        // 3 × R115 + 1 × R70 = R345 + R70 = R415 (not R435, adjusting)
        // Let's use: 3 × R100 + 1 × R135 = R435

        // Act
        var item1 = _service.CalculateLineItem(100.00m, 3); // R300
        var item2 = _service.CalculateLineItem(135.00m, 1); // R135

        // Assert - Subtotal = LineTotal - VATAmount
        var totalAmount = item1.LineTotal + item2.LineTotal;
        var totalSubtotal = (item1.LineTotal - item1.VATAmount) + (item2.LineTotal - item2.VATAmount);
        var totalVAT = item1.VATAmount + item2.VATAmount;

        totalAmount.Should().Be(435.00m);
        totalSubtotal.Should().Be(378.26m);
        totalVAT.Should().Be(56.74m);

        // Verify: Subtotal + VAT = Total
        (totalSubtotal + totalVAT).Should().Be(totalAmount);
    }

    [Fact]
    public void RealWorld_SmallSale_R23Item_CorrectVATBreakdown()
    {
        // Small sale scenario
        // Act
        var result = _service.CalculateLineItem(23.00m, 1);

        // Assert
        result.LineTotal.Should().Be(23.00m);
        (result.LineTotal - result.VATAmount).Should().Be(20.00m); // Subtotal
        result.VATAmount.Should().Be(3.00m);
    }

    #endregion
}
