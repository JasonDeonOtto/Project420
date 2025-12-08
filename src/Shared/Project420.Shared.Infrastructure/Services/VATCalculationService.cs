using Project420.Shared.Infrastructure.DTOs;
using Project420.Shared.Infrastructure.Interfaces;

namespace Project420.Shared.Infrastructure.Services;

/// <summary>
/// Implementation of VAT calculation service for South African financial transactions.
/// Provides accurate VAT calculations following SARS (South African Revenue Service) requirements.
/// </summary>
/// <remarks>
/// Design Principles:
/// 1. All retail prices are VAT-inclusive (SA standard)
/// 2. VAT is extracted using reverse calculation (Total / 1.15)
/// 3. Rounding follows SA convention (MidpointRounding.AwayFromZero)
/// 4. Rounding adjustments are absorbed into TaxAmount
/// 5. Maximum variance allowed: R0.01 (1 cent)
///
/// VAT Calculation Formula:
/// - VAT Rate: 15% (0.15)
/// - VAT Divisor: 1.15 (for reverse calculation)
/// - VAT Amount = Total - (Total / 1.15)
/// - Subtotal = Total - VAT Amount
/// </remarks>
public class VATCalculationService : IVATCalculationService
{
    #region Constants

    /// <summary>
    /// The current South African VAT rate (15% = 0.15).
    /// </summary>
    private const decimal VAT_RATE = 0.15m;

    /// <summary>
    /// The VAT divisor for reverse calculation (1 + VAT_RATE = 1.15).
    /// Used to extract VAT from VAT-inclusive amounts.
    /// </summary>
    private const decimal VAT_DIVISOR = 1.15m;

    /// <summary>
    /// Maximum acceptable rounding variance (1 cent).
    /// </summary>
    private const decimal MAX_VARIANCE = 0.01m;

    #endregion

    #region Line Item Calculations

    /// <inheritdoc />
    public VATBreakdown CalculateLineItem(decimal unitPriceInclVAT, int quantity)
    {
        // Input validation
        if (unitPriceInclVAT < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPriceInclVAT));

        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative.", nameof(quantity));

        // Step 1: Calculate line total (VAT-inclusive)
        decimal lineTotal = unitPriceInclVAT * quantity;

        // Step 2: Calculate subtotal (VAT-exclusive)
        decimal subtotal = RoundToNearestCent(lineTotal / VAT_DIVISOR);

        // Step 3: Calculate VAT amount
        decimal taxAmount = lineTotal - subtotal;

        // Step 4: Round tax amount to nearest cent
        taxAmount = RoundToNearestCent(taxAmount);

        // Step 5: Verify accuracy (Total should = Subtotal + TaxAmount)
        decimal calculatedTotal = subtotal + taxAmount;
        decimal variance = lineTotal - calculatedTotal;

        // Apply rounding adjustment if needed
        if (Math.Abs(variance) > MAX_VARIANCE)
        {
            taxAmount += variance; // Absorb variance into tax
        }

        return new VATBreakdown
        {
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = lineTotal,
            RoundingAdjustment = variance
        };
    }

    /// <inheritdoc />
    public VATBreakdown CalculateLineItemWithDiscount(decimal unitPriceInclVAT, int quantity, decimal lineDiscountAmount)
    {
        // Input validation
        if (lineDiscountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(lineDiscountAmount));

        // Calculate base line total
        decimal lineTotal = unitPriceInclVAT * quantity;

        // Apply discount
        decimal discountedTotal = Math.Max(0, lineTotal - lineDiscountAmount);

        // Calculate VAT breakdown on discounted amount
        decimal subtotal = RoundToNearestCent(discountedTotal / VAT_DIVISOR);
        decimal taxAmount = RoundToNearestCent(discountedTotal - subtotal);

        // Verify and adjust
        decimal calculatedTotal = subtotal + taxAmount;
        decimal variance = discountedTotal - calculatedTotal;

        if (Math.Abs(variance) > MAX_VARIANCE)
        {
            taxAmount += variance;
        }

        return new VATBreakdown
        {
            Subtotal = subtotal,
            TaxAmount = taxAmount,
            Total = discountedTotal,
            RoundingAdjustment = variance
        };
    }

    /// <inheritdoc />
    public decimal CalculateVATAmount(decimal amountInclVAT)
    {
        if (amountInclVAT < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amountInclVAT));

        // VAT Amount = Total - (Total / 1.15)
        decimal subtotal = amountInclVAT / VAT_DIVISOR;
        decimal vatAmount = amountInclVAT - subtotal;

        return RoundToNearestCent(vatAmount);
    }

    /// <inheritdoc />
    public decimal CalculateSubtotal(decimal amountInclVAT)
    {
        if (amountInclVAT < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amountInclVAT));

        // Subtotal = Total / 1.15
        decimal subtotal = amountInclVAT / VAT_DIVISOR;

        return RoundToNearestCent(subtotal);
    }

    #endregion

    #region Header Aggregation

    /// <inheritdoc />
    public VATBreakdown CalculateHeaderTotals(IEnumerable<VATBreakdown> lineItems)
    {
        if (lineItems == null || !lineItems.Any())
        {
            return new VATBreakdown
            {
                Subtotal = 0.00m,
                TaxAmount = 0.00m,
                Total = 0.00m,
                RoundingAdjustment = 0.00m
            };
        }

        // Sum all line item components
        decimal totalSubtotal = lineItems.Sum(item => item.Subtotal);
        decimal totalTax = lineItems.Sum(item => item.TaxAmount);
        decimal totalAmount = lineItems.Sum(item => item.Total);

        // Calculate what the total SHOULD be based on aggregated subtotal and tax
        decimal calculatedTotal = totalSubtotal + totalTax;

        // Check for variance
        decimal variance = totalAmount - calculatedTotal;

        // If variance exceeds 1 cent, adjust tax amount
        if (Math.Abs(variance) > MAX_VARIANCE)
        {
            totalTax += variance; // Absorb variance into tax
        }

        return new VATBreakdown
        {
            Subtotal = totalSubtotal,
            TaxAmount = totalTax,
            Total = totalAmount,
            RoundingAdjustment = variance
        };
    }

    /// <inheritdoc />
    public decimal CalculateRoundingAdjustment(decimal expectedTotal, decimal calculatedTotal)
    {
        return expectedTotal - calculatedTotal;
    }

    #endregion

    #region Discount Calculations

    /// <inheritdoc />
    public decimal ApplyPercentageDiscount(decimal amount, decimal discountPercentage)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (discountPercentage < 0 || discountPercentage > 100)
            throw new ArgumentException("Discount percentage must be between 0 and 100.", nameof(discountPercentage));

        decimal discountMultiplier = 1 - (discountPercentage / 100);
        decimal discountedAmount = amount * discountMultiplier;

        return RoundToNearestCent(discountedAmount);
    }

    /// <inheritdoc />
    public decimal ApplyFixedDiscount(decimal amount, decimal discountAmount)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative.", nameof(amount));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative.", nameof(discountAmount));

        decimal discountedAmount = Math.Max(0, amount - discountAmount);

        return RoundToNearestCent(discountedAmount);
    }

    #endregion

    #region Utility Methods

    /// <inheritdoc />
    public decimal RoundToNearestCent(decimal amount)
    {
        // South African rounding convention: Round to 2 decimal places, away from zero
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    /// <inheritdoc />
    public decimal GetCurrentVATRate()
    {
        return VAT_RATE;
    }

    #endregion
}

//Points to consider:
//VAT as a user defined Variable (since VAT rates can change)
//