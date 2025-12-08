namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Cash drawer reconciliation for verifying physical cash vs system records
/// </summary>
/// <remarks>
/// Used for:
/// - End-of-shift cash counting
/// - Daily close-out procedures
/// - Fraud detection (large variances)
/// - Cashier performance tracking
///
/// Best Practice:
/// - Reconcile cash drawer at end of each shift
/// - Variance tolerance: +/- R1.00 (acceptable rounding)
/// - Variance > R10: Requires manager investigation
/// - Variance > R100: Requires incident report
///
/// Audit Compliance:
/// - All reconciliations must be recorded
/// - Variances must be explained and signed off
/// - Pattern of variances may indicate training issues or fraud
/// </remarks>
public class CashDrawerReconciliation
{
    /// <summary>
    /// Date/time of reconciliation
    /// </summary>
    public DateTime ReconciliationDate { get; set; }

    /// <summary>
    /// Shift start date/time
    /// </summary>
    public DateTime ShiftStart { get; set; }

    /// <summary>
    /// Shift end date/time
    /// </summary>
    public DateTime ShiftEnd { get; set; }

    // ========================================
    // EXPECTED CASH (From System)
    // ========================================

    /// <summary>
    /// Opening float at start of shift
    /// </summary>
    /// <remarks>
    /// Standard opening float: R500-R1000
    /// Used to provide change for first customers.
    /// </remarks>
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// Total cash sales during shift (from system)
    /// </summary>
    /// <remarks>
    /// Sum of all cash payment amounts from transactions.
    /// Excludes card, EFT, and on-account payments.
    /// </remarks>
    public decimal CashSales { get; set; }

    /// <summary>
    /// Cash paid out during shift (refunds, petty cash, etc.)
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - Cash refunds to customers
    /// - Petty cash withdrawals
    /// - Change requests
    /// Should be minimal - large payouts require manager approval.
    /// </remarks>
    public decimal CashPaidOut { get; set; }

    /// <summary>
    /// Expected cash in drawer (calculated)
    /// </summary>
    /// <remarks>
    /// Formula: OpeningFloat + CashSales - CashPaidOut
    /// This is what SHOULD be in the drawer.
    /// </remarks>
    public decimal ExpectedCash => OpeningFloat + CashSales - CashPaidOut;

    // ========================================
    // ACTUAL CASH (Physical Count)
    // ========================================

    /// <summary>
    /// Actual cash counted in drawer
    /// </summary>
    /// <remarks>
    /// Physical count by cashier at end of shift.
    /// Should be verified by manager/supervisor.
    /// Count by denomination for accuracy.
    /// </remarks>
    public decimal ActualCash { get; set; }

    /// <summary>
    /// Closing float (amount left in drawer for next shift)
    /// </summary>
    /// <remarks>
    /// Usually same as opening float (R500-R1000).
    /// Remaining cash goes to safe/bank deposit.
    /// </remarks>
    public decimal ClosingFloat { get; set; }

    /// <summary>
    /// Amount to bank (cash removed from drawer)
    /// </summary>
    /// <remarks>
    /// Formula: ActualCash - ClosingFloat
    /// This amount should be deposited to bank or safe.
    /// </remarks>
    public decimal AmountToBanking => ActualCash - ClosingFloat;

    // ========================================
    // VARIANCE ANALYSIS
    // ========================================

    /// <summary>
    /// Variance between expected and actual cash
    /// </summary>
    /// <remarks>
    /// Formula: ActualCash - ExpectedCash
    /// - Positive: Cash over (more than expected) - may indicate pricing errors
    /// - Negative: Cash short (less than expected) - may indicate theft or errors
    /// </remarks>
    public decimal Variance => ActualCash - ExpectedCash;

    /// <summary>
    /// Absolute variance (unsigned)
    /// </summary>
    public decimal AbsoluteVariance => Math.Abs(Variance);

    /// <summary>
    /// Indicates if variance is within acceptable tolerance
    /// </summary>
    /// <remarks>
    /// Acceptable variance: +/- R1.00 (coins/rounding)
    /// </remarks>
    public bool IsWithinTolerance => AbsoluteVariance <= 1.00m;

    /// <summary>
    /// Indicates if reconciliation is considered balanced
    /// </summary>
    /// <remarks>
    /// Balanced if variance is less than R0.01 (1 cent)
    /// </remarks>
    public bool IsReconciled => AbsoluteVariance < 0.01m;

    // ========================================
    // TRANSACTION STATISTICS
    // ========================================

    /// <summary>
    /// Number of cash transactions during shift
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Number of refunds processed (cash only)
    /// </summary>
    public int RefundCount { get; set; }

    /// <summary>
    /// Average cash transaction value
    /// </summary>
    public decimal AverageTransactionValue => TransactionCount > 0
        ? CashSales / TransactionCount
        : 0;

    // ========================================
    // USER TRACKING
    // ========================================

    /// <summary>
    /// Cashier/user ID who operated this drawer
    /// </summary>
    public int CashierId { get; set; }

    /// <summary>
    /// Cashier/user name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Manager/supervisor who verified the count
    /// </summary>
    public int? VerifiedByUserId { get; set; }

    /// <summary>
    /// Manager/supervisor name
    /// </summary>
    public string? VerifiedByUserName { get; set; }

    // ========================================
    // NOTES & EXPLANATIONS
    // ========================================

    /// <summary>
    /// Notes about the reconciliation
    /// </summary>
    /// <remarks>
    /// REQUIRED if variance > R10.00
    /// Examples:
    /// - "Short R15.50 - pricing error on transaction SALE-20251206-045"
    /// - "Over R5.00 - customer did not take change"
    /// - "Balanced - no issues"
    /// </remarks>
    public string? Notes { get; set; }

    /// <summary>
    /// Variance explanation (required for variances > R10)
    /// </summary>
    public string? VarianceExplanation { get; set; }

    // ========================================
    // SEVERITY CLASSIFICATION
    // ========================================

    /// <summary>
    /// Gets the severity level of the variance
    /// </summary>
    public string VarianceSeverity
    {
        get
        {
            if (IsReconciled) return "Balanced";
            if (AbsoluteVariance <= 1.00m) return "Acceptable";
            if (AbsoluteVariance <= 10.00m) return "Minor";
            if (AbsoluteVariance <= 100.00m) return "Moderate - Requires Investigation";
            return "Severe - Requires Incident Report";
        }
    }

    /// <summary>
    /// Indicates if this reconciliation requires manager review
    /// </summary>
    public bool RequiresManagerReview => AbsoluteVariance > 10.00m;
}
