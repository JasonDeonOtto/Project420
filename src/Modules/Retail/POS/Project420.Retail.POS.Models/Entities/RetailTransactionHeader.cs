using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Project420.Shared.Core.Entities;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.Models.Entities
{
    /// <summary>
    /// Represents a retail transaction header (invoice/receipt) for point-of-sale operations
    /// </summary>
    /// <remarks>
    /// RENAMED FROM POSTransactionHeader → RetailTransactionHeader (Phase 7B)
    ///
    /// Reason for Rename:
    /// - Aligns with unified transaction architecture
    /// - Headers stay in module-specific databases (Project420_Dev)
    /// - Details move to unified TransactionDetails (Project420_Shared)
    /// - Supports future retail transaction types beyond POS
    ///
    /// This is the "header" or "master" record for a retail transaction.
    /// Think of it as the invoice/receipt that contains multiple line items.
    ///
    /// Header-Detail Pattern (Unified Architecture):
    /// RetailTransactionHeader (Project420_Dev) → TransactionDetails (Project420_Shared)
    /// TransactionDetails → Movements (for SOH calculation)
    ///
    /// Example:
    /// RetailTransactionHeader #12345 (Invoice)
    /// ├─ TransactionDetail #1: Blue Dream x2 @ R150 = R300
    /// ├─ TransactionDetail #2: OG Kush x1 @ R155 = R155
    /// └─ TransactionDetail #3: CBD Oil x1 @ R450 = R450
    ///    Total: R905
    ///
    /// Cannabis Compliance:
    /// - All transactions must be recorded for SAHPRA reporting
    /// - Age verification required before transaction completes
    /// - Audit trail mandatory (who sold what, when, to whom)
    /// - Tax calculations must be accurate for SARS
    ///
    /// POPIA Compliance:
    /// - Audit trail via AuditableEntity inheritance
    /// - Customer PII protected (link via DebtorId, not embedded)
    /// - 7-year retention for tax purposes
    /// </remarks>
    public class RetailTransactionHeader : AuditableEntity
    {
        // ============================================================
        // TRANSACTION IDENTIFICATION
        // ============================================================

        /// <summary>
        /// Unique transaction/invoice number (user-facing)
        /// </summary>
        /// <remarks>
        /// Different from database Id - this appears on receipts/invoices
        /// Format examples:
        /// - "INV-2024-00001"
        /// - "SALE-20241202-001"
        /// - "REC-12345"
        ///
        /// Best Practice: Auto-generate with prefix + date + sequence
        /// Must be unique across all transactions
        /// </remarks>
        [Required(ErrorMessage = "Transaction number is required")]
        [MaxLength(50)]
        [Display(Name = "Transaction Number")]
        public string TransactionNumber { get; set; } = string.Empty;

        /// <summary>
        /// Date and time when this transaction occurred
        /// </summary>
        [Required]
        [Display(Name = "Transaction Date")]
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Type of transaction
        /// </summary>
        /// <remarks>
        /// Sale = Customer purchases products
        /// Refund = Customer returns products
        /// AccountPayment = Customer pays off debt (no products)
        /// Layby = Reserve products, pay over time
        /// Quote = Estimate/quotation (not a final sale)
        /// </remarks>
        [Required]
        [Display(Name = "Transaction Type")]
        public TransactionType TransactionType { get; set; } = TransactionType.Sale;

        /// <summary>
        /// Current status of this transaction
        /// </summary>
        /// <remarks>
        /// Pending = Being created, not finalized
        /// Completed = Finalized and payment received
        /// Cancelled = Voided/cancelled
        /// OnHold = Awaiting approval or payment
        /// Refunded = Has been refunded (fully or partially)
        /// </remarks>
        [Required]
        [Display(Name = "Status")]
        public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

        // ============================================================
        // CUSTOMER INFORMATION
        // ============================================================

        /// <summary>
        /// Foreign key to Customer/Debtor (optional for walk-in customers)
        /// </summary>
        /// <remarks>
        /// Nullable: Walk-in customers don't need an account
        /// Not Nullable: Account customers, credit sales
        ///
        /// Cannabis Compliance: Even walk-ins must show age verification
        /// (verified at POS, but no account created)
        /// </remarks>
        [Display(Name = "Customer")]
        public int? DebtorId { get; set; }

        /// <summary>
        /// Name of customer (for walk-ins or quick reference)
        /// </summary>
        /// <remarks>
        /// For walk-in customers: Capture name from ID during age verification
        /// For account customers: Copy from Debtor record (for speed)
        /// Denormalization: Duplicates Debtor.Name for reporting performance
        /// </remarks>
        [MaxLength(200)]
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        // ============================================================
        // PRICING INFORMATION
        // ============================================================

        /// <summary>
        /// Foreign key to Pricelist used for this transaction (optional)
        /// </summary>
        /// <remarks>
        /// Determines which prices were used
        /// If null, Product.Price (default price) was used
        /// If set, prices came from PricelistItems
        ///
        /// Useful for:
        /// - VIP customer pricing
        /// - Medical patient pricing
        /// - Promotional pricing
        /// - Audit trail (what price list was active)
        /// </remarks>
        [Display(Name = "Pricelist")]
        public int? PricelistId { get; set; }

        // ============================================================
        // FINANCIAL TOTALS
        // ============================================================

        /// <summary>
        /// Subtotal before tax (sum of all line item subtotals)
        /// </summary>
        /// <remarks>
        /// Calculated from TransactionDetails:
        /// Sum(Quantity * UnitPrice) for all details
        ///
        /// South African VAT: Prices usually include VAT
        /// If price is VAT-inclusive:
        /// Subtotal = TotalAmount / 1.15
        /// TaxAmount = TotalAmount - Subtotal
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        /// <summary>
        /// Total tax amount (VAT 15% in South Africa)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance:
        /// - Current: VAT 15% (if turnover > R1 million)
        /// - Future: May have cannabis-specific excise duty
        ///
        /// Calculation (VAT-inclusive pricing):
        /// TaxAmount = TotalAmount - (TotalAmount / 1.15)
        ///
        /// SARS Reporting: Must be accurate for VAT201 returns
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; }

        /// <summary>
        /// Total amount payable (Subtotal + Tax)
        /// </summary>
        /// <remarks>
        /// Final amount customer must pay
        /// Sum of all TransactionDetail.Total values
        ///
        /// For refunds: This will be negative
        /// </remarks>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Discount amount applied to this transaction (optional)
        /// </summary>
        /// <remarks>
        /// Future Enhancement: Overall transaction discounts
        /// (separate from pricelist discounts per product)
        ///
        /// Examples:
        /// - Manager override discount
        /// - Loyalty program discount
        /// - Promotional coupon
        /// </remarks>
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Discount Amount")]
        public decimal DiscountAmount { get; set; } = 0.00m;

        // ============================================================
        // TRANSACTION METADATA
        // ============================================================

        /// <summary>
        /// User/Cashier who processed this transaction
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Must track WHO made the sale for audit
        ///
        /// Future Enhancement: Link to User/Employee entity
        /// For now, store username or employee ID as string
        ///
        /// This is separate from CreatedBy (AuditableEntity)
        /// because CreatedBy is the system user, this is the cashier
        /// </remarks>
        [Required]
        [MaxLength(100)]
        [Display(Name = "Cashier / Processed By")]
        public string ProcessedBy { get; set; } = string.Empty;

        /// <summary>
        /// Internal notes about this transaction
        /// </summary>
        /// <remarks>
        /// Use for:
        /// - Special instructions
        /// - Manager approval notes
        /// - Reasons for refund/cancellation
        /// - Customer requests
        /// </remarks>
        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        /// <summary>
        /// Reference to original transaction (for refunds/exchanges)
        /// </summary>
        /// <remarks>
        /// Cannabis Compliance: Refunds must reference original sale
        ///
        /// If this is a Refund:
        /// - OriginalTransactionId = the Sale being refunded
        /// - Links refund to original for audit trail
        ///
        /// If this is a Sale:
        /// - OriginalTransactionId = null
        /// </remarks>
        [Display(Name = "Original Transaction")]
        public int? OriginalTransactionId { get; set; }

        // ============================================================
        // NAVIGATION PROPERTIES (EF Core Relationships)
        // ============================================================

        /// <summary>
        /// Navigation property to Customer/Debtor (optional)
        /// </summary>
        [ForeignKey(nameof(DebtorId))]
        public virtual Debtor? Debtor { get; set; }

        /// <summary>
        /// Navigation property to Pricelist (optional)
        /// </summary>
        [ForeignKey(nameof(PricelistId))]
        public virtual Pricelist? Pricelist { get; set; }

        /// <summary>
        /// Navigation property to original transaction (for refunds)
        /// </summary>
        [ForeignKey(nameof(OriginalTransactionId))]
        public virtual RetailTransactionHeader? OriginalTransaction { get; set; }

        /// <summary>
        /// Collection of line items (products) in this transaction
        /// </summary>
        /// <remarks>
        /// Phase 7B: Uses unified TransactionDetail from Shared.Core.
        /// TransactionDetails are stored in SharedDbContext with HeaderId + TransactionType discriminator.
        /// This navigation property is populated by repositories querying SharedDbContext.
        /// </remarks>
        public virtual ICollection<TransactionDetail> TransactionDetails { get; set; } = new List<TransactionDetail>();

        /// <summary>
        /// Collection of payments for this transaction
        /// </summary>
        /// <remarks>
        /// One transaction can have multiple payments
        /// Example: R200 sale paid with R100 cash + R100 card
        ///
        /// RetailTransactionHeader (1) → Payments (Many)
        /// </remarks>
        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

        /// <summary>
        /// Collection of refund transactions that reference this as original
        /// </summary>
        /// <remarks>
        /// If this is a Sale, can have multiple partial refunds
        /// Each refund transaction has OriginalTransactionId = this.Id
        /// </remarks>
        public virtual ICollection<RetailTransactionHeader> RefundTransactions { get; set; } = new List<RetailTransactionHeader>();
    }
}
