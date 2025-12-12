using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service interface for POS calculation operations (VAT, totals, discounts, rounding)
    /// </summary>
    /// <remarks>
    /// South African VAT Context:
    /// - Standard VAT Rate: 15%
    /// - VAT Registration Threshold: R1,000,000 annual turnover
    /// - Price Display: VAT-inclusive (standard practice in SA retail)
    /// - Reporting: VAT201 returns to SARS
    ///
    /// Calculation Strategy:
    /// - Detail-first approach: Build line items, then aggregate to header
    /// - VAT-inclusive pricing: Extract VAT from total (divide by 1.15)
    /// - Rounding: Handle per-line to avoid accumulation errors
    /// - Variance handling: Adjust VAT amount if total variance > R0.01
    ///
    /// Compliance:
    /// - SARS requires accurate VAT calculations
    /// - All rounding must favor the business (not customer) for tax purposes
    /// - Audit trail of all calculations required
    ///
    /// Phase 7B: Updated to use unified TransactionDetail entity.
    /// </remarks>
    public interface IPOSCalculationService
    {
        // ========================================
        // LINE ITEM CALCULATIONS
        // ========================================

        /// <summary>
        /// Calculate a complete line item with VAT breakdown
        /// </summary>
        /// <param name="unitPriceInclVAT">Unit price including VAT (e.g., R10.00)</param>
        /// <param name="quantity">Quantity sold</param>
        /// <returns>Populated TransactionDetail with calculated amounts</returns>
        /// <remarks>
        /// Phase 7B: Now returns unified TransactionDetail from Shared.Core.
        ///
        /// Calculation flow:
        /// 1. LineTotal = UnitPrice * Quantity
        /// 2. VATAmount = LineTotal - (LineTotal / 1.15)
        /// 3. Subtotal (implicit) = LineTotal - VATAmount
        ///
        /// Example: R10.00 × 5 = R50.00 total
        ///          VATAmount = R50.00 - (R50.00 / 1.15) = R6.52
        ///          Subtotal = R50.00 - R6.52 = R43.48
        /// </remarks>
        TransactionDetail CalculateLineItem(decimal unitPriceInclVAT, int quantity);

        /// <summary>
        /// Calculate line subtotal (excluding VAT)
        /// </summary>
        /// <param name="unitPrice">Unit price including VAT</param>
        /// <param name="quantity">Quantity</param>
        /// <returns>Subtotal excluding VAT</returns>
        decimal CalculateLineSubtotal(decimal unitPrice, int quantity);

        /// <summary>
        /// Calculate VAT amount for a line total
        /// </summary>
        /// <param name="lineTotal">Line total including VAT</param>
        /// <returns>VAT amount</returns>
        /// <remarks>
        /// Formula: VAT = Total - (Total / 1.15)
        /// Example: R115.00 → VAT = R115.00 - R100.00 = R15.00
        /// </remarks>
        decimal CalculateLineVAT(decimal lineTotal);

        // ========================================
        // HEADER CALCULATIONS (AGGREGATION)
        // ========================================

        /// <summary>
        /// Calculate header totals by aggregating all detail lines
        /// </summary>
        /// <param name="header">Transaction header with populated detail lines</param>
        /// <remarks>
        /// Aggregation flow:
        /// 1. Sum all detail subtotals → Header.Subtotal
        /// 2. Sum all detail VAT amounts → Header.TaxAmount
        /// 3. Sum all detail totals → Header.TotalAmount
        /// 4. Check for rounding variance (Total ≠ Subtotal + VAT)
        /// 5. If variance > R0.01, adjust VAT amount to match
        ///
        /// Why adjust VAT? SARS accepts minor VAT adjustments for rounding,
        /// but total must ALWAYS match the sum of line items.
        /// </remarks>
        void CalculateHeaderTotals(RetailTransactionHeader header);

        /// <summary>
        /// Calculate subtotal (sum of all line item subtotals)
        /// </summary>
        /// <param name="details">List of transaction detail lines</param>
        /// <returns>Total subtotal excluding VAT</returns>
        /// <remarks>
        /// Phase 7B: Now uses unified TransactionDetail.
        /// Subtotal = Sum(LineTotal - VATAmount) for each detail.
        /// </remarks>
        decimal CalculateSubtotal(List<TransactionDetail> details);

        /// <summary>
        /// Calculate total VAT (sum of all line item VAT amounts)
        /// </summary>
        /// <param name="details">List of transaction detail lines</param>
        /// <returns>Total VAT amount</returns>
        /// <remarks>
        /// Phase 7B: Now uses unified TransactionDetail.VATAmount.
        /// </remarks>
        decimal CalculateTotalVAT(List<TransactionDetail> details);

        /// <summary>
        /// Calculate total amount (sum of all line item totals)
        /// </summary>
        /// <param name="details">List of transaction detail lines</param>
        /// <returns>Total amount including VAT</returns>
        /// <remarks>
        /// Phase 7B: Now uses unified TransactionDetail.LineTotal.
        /// </remarks>
        decimal CalculateTotalAmount(List<TransactionDetail> details);

        // ========================================
        // DISCOUNT CALCULATIONS
        // ========================================

        /// <summary>
        /// Apply a percentage discount to an amount
        /// </summary>
        /// <param name="amount">Original amount</param>
        /// <param name="percentage">Discount percentage (e.g., 10 for 10%)</param>
        /// <returns>Discounted amount</returns>
        /// <remarks>
        /// Example: R100.00 × 10% discount = R90.00
        /// </remarks>
        decimal ApplyPercentageDiscount(decimal amount, decimal percentage);

        /// <summary>
        /// Apply a fixed discount to an amount
        /// </summary>
        /// <param name="amount">Original amount</param>
        /// <param name="discountAmount">Fixed discount amount</param>
        /// <returns>Discounted amount</returns>
        /// <remarks>
        /// Example: R100.00 - R10.00 discount = R90.00
        /// </remarks>
        decimal ApplyFixedDiscount(decimal amount, decimal discountAmount);

        // ========================================
        // ROUNDING AND VARIANCE
        // ========================================

        /// <summary>
        /// Round amount to nearest cent (2 decimal places)
        /// </summary>
        /// <param name="amount">Amount to round</param>
        /// <returns>Rounded amount</returns>
        /// <remarks>
        /// Uses MidpointRounding.AwayFromZero (standard for financial calculations).
        /// Example: R10.125 → R10.13, R10.124 → R10.12
        /// </remarks>
        decimal RoundToNearestCent(decimal amount);

        /// <summary>
        /// Calculate rounding adjustment needed to reconcile totals
        /// </summary>
        /// <param name="expectedTotal">Expected total (sum of line items)</param>
        /// <param name="calculatedTotal">Calculated total (subtotal + VAT)</param>
        /// <returns>Adjustment amount (can be positive or negative)</returns>
        /// <remarks>
        /// If variance > R0.01, this indicates a rounding issue that needs correction.
        /// Typically applied to VAT amount to keep total accurate.
        ///
        /// Example: Expected R1000.00, Calculated R999.97
        ///          Adjustment = R0.03 (add to VAT)
        /// </remarks>
        decimal CalculateRoundingAdjustment(decimal expectedTotal, decimal calculatedTotal);
    }
}
