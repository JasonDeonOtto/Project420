namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Summary of payments by method for a specific date/period
/// </summary>
/// <remarks>
/// Used for:
/// - Daily cash drawer reconciliation
/// - End-of-shift reporting
/// - Manager dashboards
/// - Financial reporting
/// - Audit trails
///
/// SARS Compliance:
/// - All payment totals must be verifiable against transaction records
/// - Used for VAT201 reporting preparation
/// </remarks>
public class PaymentSummary
{
    /// <summary>
    /// Date/time this summary was generated
    /// </summary>
    public DateTime SummaryDate { get; set; }

    /// <summary>
    /// Start date/time of the period (inclusive)
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End date/time of the period (inclusive)
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    // ========================================
    // PAYMENT TOTALS BY METHOD
    // ========================================

    /// <summary>
    /// Total cash payments
    /// </summary>
    /// <remarks>
    /// Critical for cash drawer reconciliation.
    /// Must match physical cash count.
    /// </remarks>
    public decimal TotalCash { get; set; }

    /// <summary>
    /// Total card payments (credit/debit)
    /// </summary>
    /// <remarks>
    /// Should reconcile with card terminal batch totals.
    /// Includes Visa, Mastercard, Amex, etc.
    /// </remarks>
    public decimal TotalCard { get; set; }

    /// <summary>
    /// Total EFT payments
    /// </summary>
    /// <remarks>
    /// Direct bank transfers.
    /// Should reconcile with bank statement.
    /// </remarks>
    public decimal TotalEFT { get; set; }

    /// <summary>
    /// Total mobile payments (SnapScan, Zapper, etc.)
    /// </summary>
    /// <remarks>
    /// Should reconcile with mobile payment provider statements.
    /// </remarks>
    public decimal TotalMobilePayment { get; set; }

    /// <summary>
    /// Total on-account payments
    /// </summary>
    /// <remarks>
    /// Customer account payments (no cash received).
    /// Affects debtor balances only.
    /// </remarks>
    public decimal TotalOnAccount { get; set; }

    /// <summary>
    /// Total voucher/gift card payments
    /// </summary>
    /// <remarks>
    /// Should reconcile with voucher redemption records.
    /// </remarks>
    public decimal TotalVoucher { get; set; }

    // ========================================
    // AGGREGATE TOTALS
    // ========================================

    /// <summary>
    /// Grand total of all payments (sum of all methods)
    /// </summary>
    /// <remarks>
    /// Should match sum of all transaction TotalAmount fields.
    /// Used for high-level financial verification.
    /// </remarks>
    public decimal GrandTotal => TotalCash + TotalCard + TotalEFT +
                                 TotalMobilePayment + TotalOnAccount + TotalVoucher;

    /// <summary>
    /// Total number of transactions
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue => TransactionCount > 0
        ? GrandTotal / TransactionCount
        : 0;

    // ========================================
    // BREAKDOWN COUNTS (for audit)
    // ========================================

    /// <summary>
    /// Number of cash transactions
    /// </summary>
    public int CashTransactionCount { get; set; }

    /// <summary>
    /// Number of card transactions
    /// </summary>
    public int CardTransactionCount { get; set; }

    /// <summary>
    /// Number of EFT transactions
    /// </summary>
    public int EFTTransactionCount { get; set; }

    /// <summary>
    /// Number of mobile payment transactions
    /// </summary>
    public int MobilePaymentTransactionCount { get; set; }

    /// <summary>
    /// Number of on-account transactions
    /// </summary>
    public int OnAccountTransactionCount { get; set; }

    /// <summary>
    /// Number of voucher transactions
    /// </summary>
    public int VoucherTransactionCount { get; set; }

    // ========================================
    // USER TRACKING (Audit)
    // ========================================

    /// <summary>
    /// User/cashier ID who generated this summary
    /// </summary>
    public int? GeneratedByUserId { get; set; }

    /// <summary>
    /// User/cashier name who generated this summary
    /// </summary>
    public string? GeneratedByUserName { get; set; }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Get percentage of total for a specific payment method
    /// </summary>
    public decimal GetPaymentMethodPercentage(decimal methodTotal)
    {
        return GrandTotal > 0
            ? (methodTotal / GrandTotal) * 100
            : 0;
    }
}
