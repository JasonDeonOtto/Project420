using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Core.Entities;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Implementation of POS calculation service for VAT, totals, discounts, and rounding
    /// </summary>
    /// <remarks>
    /// South African VAT Implementation:
    /// - VAT Rate: 15% (as of 2025)
    /// - VAT Divisor: 1.15 (for extracting VAT from VAT-inclusive prices)
    /// - Rounding: MidpointRounding.AwayFromZero (standard for SA financial calculations)
    ///
    /// Accuracy:
    /// - All calculations round to 2 decimal places (cents)
    /// - Variance detection prevents accumulation errors
    /// - VAT adjustments keep totals accurate while maintaining SARS compliance
    ///
    /// Phase 7B: Updated to use unified TransactionDetail entity.
    /// </remarks>
    public class POSCalculationService : IPOSCalculationService
    {
        // ========================================
        // CONSTANTS
        // ========================================

        /// <summary>
        /// South African VAT rate (15%)
        /// </summary>
        private const decimal VAT_RATE = 0.15m;

        /// <summary>
        /// VAT divisor for extracting VAT from VAT-inclusive prices (1 + VAT_RATE)
        /// </summary>
        /// <remarks>
        /// Formula: Subtotal = Total / VAT_DIVISOR
        /// Example: R115.00 / 1.15 = R100.00
        /// </remarks>
        private const decimal VAT_DIVISOR = 1.15m;

        /// <summary>
        /// Maximum acceptable variance before triggering adjustment (1 cent)
        /// </summary>
        private const decimal MAX_VARIANCE = 0.01m;

        // ========================================
        // LINE ITEM CALCULATIONS
        // ========================================

        /// <inheritdoc />
        public TransactionDetail CalculateLineItem(decimal unitPriceInclVAT, int quantity)
        {
            // Calculate line total (VAT-inclusive)
            decimal lineTotal = unitPriceInclVAT * quantity;

            // Calculate VAT portion using the recommended approach:
            // VAT = Total - (Total / 1.15)
            decimal vatAmount = RoundToNearestCent(lineTotal - (lineTotal / VAT_DIVISOR));

            // Phase 7B: Return unified TransactionDetail with updated property names
            return new TransactionDetail
            {
                Quantity = quantity,
                UnitPrice = unitPriceInclVAT,
                LineTotal = lineTotal,
                VATAmount = vatAmount
                // Note: Other properties (ProductId, HeaderId, TransactionType, etc.) should be set by the caller
            };
        }

        /// <inheritdoc />
        public decimal CalculateLineSubtotal(decimal unitPrice, int quantity)
        {
            decimal lineTotal = unitPrice * quantity;
            decimal subtotal = lineTotal / VAT_DIVISOR;
            return RoundToNearestCent(subtotal);
        }

        /// <inheritdoc />
        public decimal CalculateLineVAT(decimal lineTotal)
        {
            // VAT = Total - (Total / 1.15)
            decimal vatAmount = lineTotal - (lineTotal / VAT_DIVISOR);
            return RoundToNearestCent(vatAmount);
        }

        // ========================================
        // HEADER CALCULATIONS (AGGREGATION)
        // ========================================

        /// <inheritdoc />
        public void CalculateHeaderTotals(RetailTransactionHeader header)
        {
            if (header.TransactionDetails == null || !header.TransactionDetails.Any())
            {
                // No line items - set everything to zero
                header.Subtotal = 0.00m;
                header.TaxAmount = 0.00m;
                header.TotalAmount = 0.00m;
                return;
            }

            // Phase 7B: Use unified TransactionDetail property names
            // Subtotal = LineTotal - VATAmount (calculated, not stored in TransactionDetail)
            decimal detailSubtotalSum = header.TransactionDetails.Sum(d => d.LineTotal - d.VATAmount);
            decimal detailTaxSum = header.TransactionDetails.Sum(d => d.VATAmount);
            decimal detailTotalSum = header.TransactionDetails.Sum(d => d.LineTotal);

            // Check for rounding variance
            decimal calculatedTotal = detailSubtotalSum + detailTaxSum;
            decimal variance = detailTotalSum - calculatedTotal;

            // If variance exceeds threshold, adjust VAT to compensate
            if (Math.Abs(variance) > MAX_VARIANCE)
            {
                // Adjust VAT by the variance amount (keeps total accurate)
                detailTaxSum += variance;
            }

            // Set header totals
            header.Subtotal = detailSubtotalSum;
            header.TaxAmount = detailTaxSum;
            header.TotalAmount = detailTotalSum;

            // Note: DiscountAmount is handled separately (transaction-level discounts)
        }

        /// <inheritdoc />
        public decimal CalculateSubtotal(List<TransactionDetail> details)
        {
            if (details == null || !details.Any())
                return 0.00m;

            // Phase 7B: Subtotal = LineTotal - VATAmount (not stored directly in TransactionDetail)
            return details.Sum(d => d.LineTotal - d.VATAmount);
        }

        /// <inheritdoc />
        public decimal CalculateTotalVAT(List<TransactionDetail> details)
        {
            if (details == null || !details.Any())
                return 0.00m;

            // Phase 7B: Use VATAmount instead of TaxAmount
            return details.Sum(d => d.VATAmount);
        }

        /// <inheritdoc />
        public decimal CalculateTotalAmount(List<TransactionDetail> details)
        {
            if (details == null || !details.Any())
                return 0.00m;

            // Phase 7B: Use LineTotal instead of Total
            return details.Sum(d => d.LineTotal);
        }

        // ========================================
        // DISCOUNT CALCULATIONS
        // ========================================

        /// <inheritdoc />
        public decimal ApplyPercentageDiscount(decimal amount, decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
                throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");

            decimal discountAmount = amount * (percentage / 100m);
            decimal discountedAmount = amount - discountAmount;

            return RoundToNearestCent(discountedAmount);
        }

        /// <inheritdoc />
        public decimal ApplyFixedDiscount(decimal amount, decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(discountAmount), "Discount amount cannot be negative");

            if (discountAmount > amount)
                throw new ArgumentOutOfRangeException(nameof(discountAmount), "Discount amount cannot exceed original amount");

            decimal discountedAmount = amount - discountAmount;

            return RoundToNearestCent(discountedAmount);
        }

        // ========================================
        // DISCOUNT WITH VAT RECALCULATION (Phase 9.2)
        // ========================================

        /// <inheritdoc />
        public (decimal subtotal, decimal vatAmount, decimal total) CalculateLineWithDiscount(decimal originalTotal, decimal discountAmount)
        {
            if (discountAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(discountAmount), "Discount amount cannot be negative");

            if (discountAmount > originalTotal)
                throw new ArgumentOutOfRangeException(nameof(discountAmount), "Discount amount cannot exceed original total");

            // Step 1: Calculate new total after discount
            decimal newTotal = RoundToNearestCent(originalTotal - discountAmount);

            // Step 2: Recalculate VAT on the new total (SA compliance)
            // VAT = NewTotal - (NewTotal / 1.15)
            decimal newVatAmount = RoundToNearestCent(newTotal - (newTotal / VAT_DIVISOR));

            // Step 3: Calculate new subtotal (excluding VAT)
            decimal newSubtotal = RoundToNearestCent(newTotal - newVatAmount);

            return (newSubtotal, newVatAmount, newTotal);
        }

        /// <inheritdoc />
        public decimal CalculateVATAfterDiscount(decimal discountedTotal)
        {
            if (discountedTotal < 0)
                throw new ArgumentOutOfRangeException(nameof(discountedTotal), "Discounted total cannot be negative");

            // VAT = DiscountedTotal - (DiscountedTotal / 1.15)
            decimal vatAmount = discountedTotal - (discountedTotal / VAT_DIVISOR);
            return RoundToNearestCent(vatAmount);
        }

        /// <inheritdoc />
        public Dictionary<int, decimal> CalculateHeaderDiscountProration(decimal headerDiscount, List<(int lineId, decimal lineTotal)> lineItems)
        {
            var result = new Dictionary<int, decimal>();

            if (lineItems == null || !lineItems.Any())
                return result;

            if (headerDiscount <= 0)
            {
                // No discount to prorate
                foreach (var item in lineItems)
                    result[item.lineId] = 0m;
                return result;
            }

            decimal totalAmount = lineItems.Sum(x => x.lineTotal);

            if (totalAmount <= 0)
            {
                // No amount to prorate against
                foreach (var item in lineItems)
                    result[item.lineId] = 0m;
                return result;
            }

            decimal allocatedDiscount = 0m;

            // Prorate all lines except the last
            for (int i = 0; i < lineItems.Count - 1; i++)
            {
                var item = lineItems[i];
                // Formula: LineDiscount = (LineTotal / TotalAmount) × HeaderDiscount
                decimal proportion = item.lineTotal / totalAmount;
                decimal lineDiscount = RoundToNearestCent(headerDiscount * proportion);
                result[item.lineId] = lineDiscount;
                allocatedDiscount += lineDiscount;
            }

            // Last line gets the remainder (ensures total equals header discount exactly)
            var lastItem = lineItems.Last();
            result[lastItem.lineId] = RoundToNearestCent(headerDiscount - allocatedDiscount);

            return result;
        }

        /// <inheritdoc />
        public decimal CalculateDiscountAmount(decimal originalAmount, decimal discountPercentage)
        {
            if (discountPercentage < 0 || discountPercentage > 100)
                throw new ArgumentOutOfRangeException(nameof(discountPercentage), "Discount percentage must be between 0 and 100");

            if (originalAmount < 0)
                throw new ArgumentOutOfRangeException(nameof(originalAmount), "Original amount cannot be negative");

            return RoundToNearestCent(originalAmount * (discountPercentage / 100m));
        }

        // ========================================
        // ROUNDING AND VARIANCE
        // ========================================

        /// <inheritdoc />
        public decimal RoundToNearestCent(decimal amount)
        {
            // Round to 2 decimal places using MidpointRounding.AwayFromZero
            // This is the standard for South African financial calculations
            return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        /// <inheritdoc />
        public decimal CalculateRoundingAdjustment(decimal expectedTotal, decimal calculatedTotal)
        {
            decimal variance = expectedTotal - calculatedTotal;

            // Round the variance to nearest cent
            return RoundToNearestCent(variance);
        }
    }
}
