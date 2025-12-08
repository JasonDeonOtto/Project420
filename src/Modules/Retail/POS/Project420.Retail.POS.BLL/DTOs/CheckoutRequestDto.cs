using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs
{
    /// <summary>
    /// Request DTO for processing a POS checkout
    /// </summary>
    /// <remarks>
    /// Cannabis Act Compliance:
    /// - AgeVerified must be true (18+ minimum)
    /// - Customer information required for traceability
    /// - Batch numbers tracked for seed-to-sale compliance
    ///
    /// POPIA Compliance:
    /// - Customer data protected and consented
    /// - Audit trail maintained
    /// </remarks>
    public class CheckoutRequestDto
    {
        // ========================================
        // CUSTOMER INFORMATION
        // ========================================

        /// <summary>
        /// Customer/Debtor ID (null for walk-in customers)
        /// </summary>
        public int? DebtorId { get; set; }

        /// <summary>
        /// Customer name (required - even for walk-ins, use "Walk-In Customer")
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Age verification confirmed (REQUIRED - Cannabis Act 18+ minimum)
        /// </summary>
        /// <remarks>
        /// CRITICAL COMPLIANCE: Must be true for all cannabis sales.
        /// System should verify age before allowing checkout.
        /// </remarks>
        public bool AgeVerified { get; set; }

        /// <summary>
        /// Customer age verification date/time
        /// </summary>
        public DateTime? AgeVerificationDate { get; set; }

        // ========================================
        // CART ITEMS
        // ========================================

        /// <summary>
        /// List of items being purchased
        /// </summary>
        public List<CartItemDto> CartItems { get; set; } = new();

        // ========================================
        // PAYMENT INFORMATION
        // ========================================

        /// <summary>
        /// Payment method (Cash, Card, EFT, etc.)
        /// </summary>
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Amount tendered by customer (for cash payments - to calculate change)
        /// </summary>
        public decimal? AmountTendered { get; set; }

        /// <summary>
        /// External payment reference (card transaction ID, EFT reference, etc.)
        /// </summary>
        public string? PaymentReference { get; set; }

        // ========================================
        // PRICELIST & DISCOUNTS
        // ========================================

        /// <summary>
        /// Pricelist ID to use (null = default pricelist)
        /// </summary>
        public int? PricelistId { get; set; }

        /// <summary>
        /// Overall discount percentage (if applicable)
        /// </summary>
        public decimal? DiscountPercentage { get; set; }

        /// <summary>
        /// Overall discount amount (if applicable)
        /// </summary>
        public decimal? DiscountAmount { get; set; }

        // ========================================
        // TRANSACTION METADATA
        // ========================================

        /// <summary>
        /// Cashier/user ID processing the sale
        /// </summary>
        public int ProcessedBy { get; set; }

        /// <summary>
        /// Optional notes for the transaction
        /// </summary>
        public string? Notes { get; set; }
    }
}
