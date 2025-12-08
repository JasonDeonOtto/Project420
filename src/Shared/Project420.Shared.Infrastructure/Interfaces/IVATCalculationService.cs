using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Shared.Infrastructure.Interfaces;

/// <summary>
/// Service for calculating VAT (Value Added Tax) for South African financial transactions.
/// Provides universal VAT calculation logic used across all transaction types:
/// - POS Sales
/// - Goods Received Notes (GRV)
/// - Return to Supplier (RTS)
/// - Sales Invoices
/// - Credit Notes
/// - Stock Adjustments
/// </summary>
/// <remarks>
/// South African VAT Requirements:
/// - Standard VAT Rate: 15% (as of 2025)
/// - Retail prices are VAT-inclusive
/// - VAT must be accurately calculated for SARS VAT201 returns
/// - Rounding must follow South African rounding conventions (away from zero)
///
/// This service implements the "Detail-First" transaction pattern:
/// 1. Calculate VAT per line item
/// 2. Aggregate line items to header totals
/// 3. Handle rounding adjustments
/// </remarks>
public interface IVATCalculationService
{
    #region Line Item Calculations

    /// <summary>
    /// Calculates the VAT breakdown for a single line item.
    /// </summary>
    /// <param name="unitPriceInclVAT">The unit price including VAT (standard SA retail pricing).</param>
    /// <param name="quantity">The quantity of items.</param>
    /// <returns>A VATBreakdown containing Subtotal (excl VAT), TaxAmount (VAT), and Total (incl VAT).</returns>
    /// <example>
    /// <code>
    /// var breakdown = service.CalculateLineItem(unitPriceInclVAT: 10.00m, quantity: 1);
    /// // Result: Subtotal = R8.70, TaxAmount = R1.30, Total = R10.00
    /// </code>
    /// </example>
    VATBreakdown CalculateLineItem(decimal unitPriceInclVAT, int quantity);

    /// <summary>
    /// Calculates the VAT breakdown for a single line item with an applied discount.
    /// </summary>
    /// <param name="unitPriceInclVAT">The unit price including VAT.</param>
    /// <param name="quantity">The quantity of items.</param>
    /// <param name="lineDiscountAmount">The discount amount to apply to the line (VAT-inclusive).</param>
    /// <returns>A VATBreakdown with discount applied before VAT extraction.</returns>
    VATBreakdown CalculateLineItemWithDiscount(decimal unitPriceInclVAT, int quantity, decimal lineDiscountAmount);

    /// <summary>
    /// Extracts the VAT amount from a VAT-inclusive amount.
    /// </summary>
    /// <param name="amountInclVAT">The amount including VAT.</param>
    /// <returns>The VAT portion of the amount.</returns>
    /// <example>
    /// <code>
    /// decimal vat = service.CalculateVATAmount(10.00m);
    /// // Result: R1.30 (VAT portion of R10.00)
    /// </code>
    /// </example>
    decimal CalculateVATAmount(decimal amountInclVAT);

    /// <summary>
    /// Calculates the VAT-exclusive (subtotal) amount from a VAT-inclusive amount.
    /// </summary>
    /// <param name="amountInclVAT">The amount including VAT.</param>
    /// <returns>The amount excluding VAT.</returns>
    /// <example>
    /// <code>
    /// decimal subtotal = service.CalculateSubtotal(10.00m);
    /// // Result: R8.70 (VAT-exclusive portion of R10.00)
    /// </code>
    /// </example>
    decimal CalculateSubtotal(decimal amountInclVAT);

    #endregion

    #region Header Aggregation

    /// <summary>
    /// Aggregates multiple line item breakdowns into a single header-level breakdown.
    /// Handles rounding adjustments to ensure Total = Subtotal + TaxAmount.
    /// </summary>
    /// <param name="lineItems">Collection of line item VAT breakdowns.</param>
    /// <returns>Aggregated VATBreakdown for the entire transaction.</returns>
    /// <remarks>
    /// This method sums all line items and applies rounding adjustment if needed.
    /// The rounding adjustment is absorbed into TaxAmount (acceptable per SARS).
    /// </remarks>
    VATBreakdown CalculateHeaderTotals(IEnumerable<VATBreakdown> lineItems);

    /// <summary>
    /// Calculates the rounding adjustment needed to balance Total = Subtotal + TaxAmount.
    /// </summary>
    /// <param name="expectedTotal">The expected total (sum of all line item totals).</param>
    /// <param name="calculatedTotal">The calculated total (subtotal + tax from aggregation).</param>
    /// <returns>The rounding adjustment amount (positive or negative).</returns>
    decimal CalculateRoundingAdjustment(decimal expectedTotal, decimal calculatedTotal);

    #endregion

    #region Discount Calculations

    /// <summary>
    /// Applies a percentage-based discount to an amount.
    /// </summary>
    /// <param name="amount">The amount to discount.</param>
    /// <param name="discountPercentage">The discount percentage (e.g., 10 for 10%).</param>
    /// <returns>The discounted amount.</returns>
    /// <example>
    /// <code>
    /// decimal discounted = service.ApplyPercentageDiscount(100.00m, 10);
    /// // Result: R90.00 (10% discount applied)
    /// </code>
    /// </example>
    decimal ApplyPercentageDiscount(decimal amount, decimal discountPercentage);

    /// <summary>
    /// Applies a fixed discount amount.
    /// </summary>
    /// <param name="amount">The original amount.</param>
    /// <param name="discountAmount">The fixed discount to subtract.</param>
    /// <returns>The discounted amount (minimum R0.00).</returns>
    decimal ApplyFixedDiscount(decimal amount, decimal discountAmount);

    #endregion

    #region Utility Methods

    /// <summary>
    /// Rounds a decimal amount to 2 decimal places using South African rounding convention.
    /// </summary>
    /// <param name="amount">The amount to round.</param>
    /// <returns>Amount rounded to 2 decimal places (away from zero).</returns>
    decimal RoundToNearestCent(decimal amount);

    /// <summary>
    /// Gets the current VAT rate as a decimal (e.g., 0.15 for 15%).
    /// </summary>
    /// <returns>The current VAT rate.</returns>
    /// <remarks>
    /// Future enhancement: Make this configurable in appsettings.json
    /// to handle potential VAT rate changes.
    /// </remarks>
    decimal GetCurrentVATRate();

    #endregion
}
