namespace Project420.Retail.POS.BLL.DTOs
{
    /// <summary>
    /// Result DTO returned after successful checkout
    /// </summary>
    public class CheckoutResultDto
    {
        /// <summary>
        /// Indicates if checkout was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Error/validation messages if checkout failed
        /// </summary>
        public List<string> ErrorMessages { get; set; } = new();

        // ========================================
        // TRANSACTION DETAILS
        // ========================================

        /// <summary>
        /// Created transaction header ID
        /// </summary>
        public int TransactionId { get; set; }

        /// <summary>
        /// Unique transaction number (e.g., "SALE-20251206-001")
        /// </summary>
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Transaction date/time
        /// </summary>
        public DateTime TransactionDate { get; set; }

        // ========================================
        // FINANCIAL SUMMARY
        // ========================================

        /// <summary>
        /// Subtotal (excluding VAT)
        /// </summary>
        public decimal Subtotal { get; set; }

        /// <summary>
        /// VAT amount (15% SA VAT)
        /// </summary>
        public decimal VATAmount { get; set; }

        /// <summary>
        /// Total discount applied
        /// </summary>
        public decimal DiscountAmount { get; set; }

        /// <summary>
        /// Total amount (including VAT, after discounts)
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Amount tendered by customer (total across all tenders)
        /// </summary>
        public decimal? AmountTendered { get; set; }

        /// <summary>
        /// Change due to customer (only from cash tenders)
        /// </summary>
        public decimal? ChangeDue { get; set; }

        // ========================================
        // MULTI-TENDER PAYMENT BREAKDOWN (Phase 9.3)
        // ========================================

        /// <summary>
        /// Detailed payment breakdown for multi-tender transactions
        /// </summary>
        /// <remarks>
        /// Contains all individual tenders with amounts, methods, and references.
        /// For single-tender transactions, this will have one entry.
        /// For multi-tender, it shows the split across payment methods.
        /// </remarks>
        public PaymentBreakdownDto? PaymentBreakdown { get; set; }

        /// <summary>
        /// Indicates if this was a multi-tender transaction
        /// </summary>
        public bool IsMultiTender => PaymentBreakdown?.Tenders.Count > 1;

        // ========================================
        // RECEIPT DATA
        // ========================================

        /// <summary>
        /// Customer name for receipt
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Payment method used (primary method for multi-tender, or the single method)
        /// </summary>
        /// <remarks>
        /// For multi-tender, this shows the summary (e.g., "Cash + Card")
        /// For single tender, this shows the method name
        /// </remarks>
        public string PaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Number of items purchased
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Cashier who processed the sale
        /// </summary>
        public string ProcessedBy { get; set; } = string.Empty;

        /// <summary>
        /// Line items for receipt display
        /// </summary>
        public List<CartItemDto> LineItems { get; set; } = new();

        // ========================================
        // COMPLIANCE DATA
        // ========================================

        /// <summary>
        /// Age verification timestamp (Cannabis Act requirement)
        /// </summary>
        public DateTime? AgeVerificationDate { get; set; }

        /// <summary>
        /// Batch numbers for traceability (comma-separated if multiple)
        /// </summary>
        public string? BatchNumbers { get; set; }

        /// <summary>
        /// Serial numbers for unit traceability (comma-separated if multiple)
        /// Phase 9.1: Added for serialized item tracking
        /// </summary>
        public string? SerialNumbers { get; set; }
    }
}
