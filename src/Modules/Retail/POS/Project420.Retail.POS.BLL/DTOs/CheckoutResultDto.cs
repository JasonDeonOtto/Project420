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
        /// Amount tendered by customer
        /// </summary>
        public decimal? AmountTendered { get; set; }

        /// <summary>
        /// Change due to customer
        /// </summary>
        public decimal? ChangeDue { get; set; }

        // ========================================
        // RECEIPT DATA
        // ========================================

        /// <summary>
        /// Customer name for receipt
        /// </summary>
        public string CustomerName { get; set; } = string.Empty;

        /// <summary>
        /// Payment method used
        /// </summary>
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
    }
}
