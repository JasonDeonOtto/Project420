using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs
{
    /// <summary>
    /// DTO representing a single payment tender in a multi-tender checkout
    /// </summary>
    /// <remarks>
    /// Multi-Tender Checkout Example:
    /// A R500 transaction could be paid with:
    /// - Cash: R200
    /// - Card: R150
    /// - EFT: R150
    /// = 3 PaymentTenderDto objects
    ///
    /// FIC Act Compliance:
    /// - Cash payments > R25,000 trigger reporting requirements
    /// - Each tender tracks its own amount for compliance reporting
    ///
    /// PCI-DSS Compliance:
    /// - Only masked card numbers stored (last 4 digits)
    /// - Full card details NEVER stored
    /// </remarks>
    public class PaymentTenderDto
    {
        /// <summary>
        /// Payment method for this tender (Cash, Card, EFT, MobilePayment, etc.)
        /// </summary>
        public PaymentMethod Method { get; set; }

        /// <summary>
        /// Amount for this tender (always positive)
        /// </summary>
        /// <remarks>
        /// For cash tenders, this is the amount tendered (can exceed transaction balance for change)
        /// For other methods, this should equal exactly the amount being charged
        /// </remarks>
        public decimal Amount { get; set; }

        /// <summary>
        /// External reference number (card transaction ID, EFT reference, mobile payment reference)
        /// </summary>
        /// <remarks>
        /// - Card: Terminal transaction ID
        /// - EFT: Bank reference number
        /// - Mobile: SnapScan/Zapper transaction ID
        /// - Cash: Null
        /// </remarks>
        public string? Reference { get; set; }

        /// <summary>
        /// Bank or payment provider name
        /// </summary>
        /// <remarks>
        /// Examples: "FNB", "Standard Bank", "Yoco", "iKhokha", "Zapper", "SnapScan"
        /// </remarks>
        public string? BankOrProvider { get; set; }

        /// <summary>
        /// Masked card number (last 4 digits only - PCI-DSS compliance)
        /// </summary>
        /// <remarks>
        /// Format: "****1234" or just "1234"
        /// NEVER store full card numbers
        /// </remarks>
        public string? MaskedCardNumber { get; set; }

        /// <summary>
        /// Card type if applicable
        /// </summary>
        /// <remarks>
        /// Examples: "Visa", "Mastercard", "Amex", "Debit"
        /// </remarks>
        public string? CardType { get; set; }

        /// <summary>
        /// Authorization/approval code from payment terminal
        /// </summary>
        public string? AuthorizationCode { get; set; }

        /// <summary>
        /// Indicates if this tender was successfully processed
        /// </summary>
        /// <remarks>
        /// For card/EFT/mobile: Confirmed by terminal or bank
        /// For cash: Always true (cash is always successful)
        /// </remarks>
        public bool IsSuccessful { get; set; } = true;

        /// <summary>
        /// Optional notes for this tender
        /// </summary>
        /// <remarks>
        /// Examples: "Customer requested receipt", "Card declined - retry successful"
        /// </remarks>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Summary DTO for payment breakdown in checkout result
    /// </summary>
    public class PaymentBreakdownDto
    {
        /// <summary>
        /// List of individual tenders that make up the payment
        /// </summary>
        public List<PaymentTenderDto> Tenders { get; set; } = new();

        /// <summary>
        /// Total amount tendered (sum of all tenders)
        /// </summary>
        public decimal TotalTendered { get; set; }

        /// <summary>
        /// Change due to customer (only from cash tenders)
        /// </summary>
        /// <remarks>
        /// Change is calculated as: Cash Tendered - Amount Owed After Other Tenders
        /// Example: R500 transaction
        /// - Card: R300 (exact)
        /// - Cash: R250 tendered for R200 balance
        /// - Change due: R50
        /// </remarks>
        public decimal ChangeDue { get; set; }

        /// <summary>
        /// Indicates if transaction is fully paid
        /// </summary>
        public bool IsFullyPaid => TotalTendered >= TotalTendered - ChangeDue;

        /// <summary>
        /// Outstanding balance (if any)
        /// </summary>
        public decimal OutstandingBalance { get; set; }

        /// <summary>
        /// Primary payment method (largest tender)
        /// </summary>
        /// <remarks>
        /// Used for reporting when a single payment method is needed
        /// </remarks>
        public string PrimaryPaymentMethod { get; set; } = string.Empty;

        /// <summary>
        /// Summary of payment methods used (e.g., "Cash + Card")
        /// </summary>
        public string PaymentMethodSummary { get; set; } = string.Empty;
    }
}
