using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a single payment made towards a transaction or on account
    /// </summary>
    /// <remarks>
    /// CRITICAL CONCEPT: Transaction ≠ Payment
    /// - Transaction = WHAT they're buying (invoice/receipt with products)
    /// - Payment = HOW they're paying (cash, card, EFT, etc.)
    ///
    /// One Transaction can have MULTIPLE Payments:
    /// Example: R300 sale
    /// ├─ Payment #1: Cash R100
    /// ├─ Payment #2: Card R150
    /// └─ Payment #3: EFT R50
    ///
    /// One Payment can exist WITHOUT a Transaction:
    /// Example: Customer pays R500 on account (reduces debt, no products sold)
    ///
    /// Relationship:
    /// TransactionHeader (0 or 1) → Payments (Many)
    ///
    /// Cannabis Compliance:
    /// - FIC Act: Cash payments over R25,000 must be reported (anti-money laundering)
    /// - SARS: All payment methods must be tracked for tax reporting
    /// - Audit trail required for all financial transactions
    ///
    /// POPIA Compliance:
    /// - Payment data is financial information (protected under POPIA)
    /// - Card details must NEVER be stored (PCI-DSS compliance)
    /// - Only store last 4 digits, masked number, or reference
    /// - 7-year retention for tax audit purposes
    /// </remarks>
    public class Payment : AuditableEntity
    {
        // ============================================================
        // FOREIGN KEYS (Relationships)
        // ============================================================

        /// <summary>
        /// Foreign key to TransactionHeader (optional - null for account payments)
        /// </summary>
        /// <remarks>
        /// Nullable: Account payments don't have a transaction
        /// Not Null: Payments for sales/refunds link to transaction
        ///
        /// Example with Transaction:
        /// - Customer buys products (Transaction created)
        /// - Customer pays cash (Payment links to Transaction)
        ///
        /// Example without Transaction:
        /// - Customer owes R1000 from previous sales
        /// - Customer pays R500 to reduce debt
        /// - Payment created with TransactionHeaderId = null
        /// </remarks>
        [Display(Name = "Transaction")]
        public int? TransactionHeaderId { get; set; }

        /// <summary>
        /// Foreign key to Customer/Debtor (optional for walk-in cash payments)
        /// </summary>
        /// <remarks>
        /// Nullable: Walk-in cash customers don't need account
        /// Not Null: Account payments, card payments (need to track who paid)
        /// </remarks>
        [Display(Name = "Customer")]
        public int? DebtorId { get; set; }

        // ============================================================
        // PAYMENT INFORMATION
        // ============================================================

        /// <summary>
        /// Unique payment reference number
        /// </summary>
        /// <remarks>
        /// Format examples:
        /// - "PAY-2024-00001"
        /// - "PMT-20241202-001"
        ///
        /// Best Practice: Auto-generate
        /// Appears on receipts for customer reference
        /// </remarks>
        [Required]
        [MaxLength(50)]
        [Display(Name = "Payment Reference")]
        public string PaymentReference { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when payment was received
        /// </summary>
        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        /// <summary>
        /// Payment method used
        /// </summary>
        /// <remarks>
        /// Cash, Card, EFT, MobilePayment, OnAccount, Voucher
        ///
        /// Cannabis Compliance: FIC Act reporting for cash > R25,000
        /// </remarks>
        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        /// <summary>
        /// Amount paid (always positive)
        /// </summary>
        /// <remarks>
        /// Always positive, even for refunds
        /// Refund = money given back to customer
        /// Payment = money received from customer
        ///
        /// For refunds: Transaction.TotalAmount is negative, Payment.Amount is positive
        /// </remarks>
        [Required]
        [Range(0.01, 999999.99, ErrorMessage = "Payment amount must be positive")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Whether this payment was successful
        /// </summary>
        /// <remarks>
        /// Cash: Always true (if recorded)
        /// Card/EFT: May be false if declined/failed
        ///
        /// Failed payments still recorded for audit trail:
        /// - Attempted transactions
        /// - Failed card attempts
        /// - Reconciliation purposes
        /// </remarks>
        [Required]
        [Display(Name = "Is Successful")]
        public bool IsSuccessful { get; set; } = true;

        // ============================================================
        // PAYMENT METHOD SPECIFIC DETAILS
        // ============================================================

        /// <summary>
        /// External reference number (bank/card processor)
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Card transactions: Authorization code
        /// - EFT: Bank reference number
        /// - Mobile payments: Transaction ID from payment gateway
        ///
        /// DO NOT STORE: Full card number, CVV, PIN
        /// PCI-DSS Compliance: Only store masked or last 4 digits
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "External Reference")]
        public string? ExternalReference { get; set; }

        /// <summary>
        /// Masked card number or account reference (last 4 digits only)
        /// </summary>
        /// <remarks>
        /// PCI-DSS Compliance: NEVER store full card numbers
        /// Format: "****1234" or "XXXX-XXXX-XXXX-1234"
        ///
        /// Only for display/reconciliation purposes
        /// Customer service: "ending in 1234"
        /// </remarks>
        [MaxLength(20)]
        [Display(Name = "Card/Account Last 4")]
        public string? MaskedCardNumber { get; set; }

        /// <summary>
        /// Name of bank or payment provider
        /// </summary>
        /// <remarks>
        /// Examples: "FNB", "Absa", "Standard Bank", "Yoco", "SnapScan", "Zapper"
        /// Useful for reconciliation and reporting
        /// </remarks>
        [MaxLength(100)]
        [Display(Name = "Bank/Provider")]
        public string? BankOrProvider { get; set; }

        // ============================================================
        // METADATA
        /// ============================================================

        /// <summary>
        /// User/Cashier who processed this payment
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Must track WHO processed payment for audit
        /// Different from CreatedBy (system user) vs ProcessedBy (cashier)
        /// </remarks>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Processed By")]
        public string ProcessedBy { get; set; } = string.Empty;

        /// <summary>
        /// Internal notes about this payment
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Reason for failure (if IsSuccessful = false)
        /// - Special circumstances
        /// - Manager approval notes
        /// - Customer requests
        /// </remarks>
        [MaxLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // ============================================================
        // CASH HANDLING COMPLIANCE (FIC Act)
        // ============================================================

        /// <summary>
        /// Whether FIC Act reporting is required for this payment
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Financial Intelligence Centre Act
        ///
        /// Cash payments over R25,000 must be reported to FIC
        /// (Anti-money laundering legislation)
        ///
        /// Automatically set to true if:
        /// - PaymentMethod = Cash
        /// - Amount >= 25000
        ///
        /// Triggers compliance workflow:
        /// - Customer identification verification
        /// - Source of funds declaration
        /// - FIC reporting form
        /// </remarks>
        [Display(Name = "FIC Reporting Required")]
        public bool FICReportingRequired { get; set; } = false;

        /// <summary>
        /// Date when FIC report was submitted (if required)
        /// </summary>
        [Display(Name = "FIC Report Date")]
        public DateTime? FICReportDate { get; set; }

        /// <summary>
        /// FIC report reference number
        /// </summary>
        [MaxLength(50)]
        [Display(Name = "FIC Report Reference")]
        public string? FICReportReference { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Navigation property to TransactionHeader (optional)
        /// </summary>
        [ForeignKey(nameof(TransactionHeaderId))]
        public virtual POSTransactionHeader? TransactionHeader { get; set; }

        /// <summary>
        /// Navigation property to Customer/Debtor (optional)
        /// </summary>
        [ForeignKey(nameof(DebtorId))]
        public virtual Debtor? Debtor { get; set; }
    }
}
