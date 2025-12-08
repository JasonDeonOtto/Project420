using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Infrastructure.DTOs;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.DAL.Repositories;

/// <summary>
/// Repository interface for payment reconciliation operations
/// </summary>
/// <remarks>
/// Payment reconciliation is critical for:
/// - Cash drawer balancing (end-of-shift)
/// - Daily close-out procedures
/// - Fraud detection (variance analysis)
/// - Cashier performance tracking
/// - SARS tax compliance (verifying payment methods)
///
/// Compliance Notes:
/// - All payment totals must be verifiable against transaction records
/// - Used for VAT201 reporting preparation
/// - Audit trail for cash handling (FIC compliance for R25,000+ cash)
/// </remarks>
public interface IPaymentRepository
{
    // ========================================
    // PAYMENT SUMMARY & REPORTING
    // ========================================

    /// <summary>
    /// Get payment summary for a date/time period
    /// </summary>
    /// <param name="periodStart">Start of period (inclusive)</param>
    /// <param name="periodEnd">End of period (inclusive)</param>
    /// <param name="userId">Optional: Filter by specific user/cashier</param>
    /// <returns>Payment summary with totals by payment method</returns>
    /// <remarks>
    /// Used for:
    /// - End-of-shift reports
    /// - Daily close-out
    /// - Manager dashboards
    /// - SARS VAT preparation
    ///
    /// Returns totals for:
    /// - Cash, Card, EFT, Mobile Payment, On Account, Voucher
    /// - Transaction counts per method
    /// - Grand totals
    /// </remarks>
    Task<PaymentSummary> GetPaymentSummaryAsync(
        DateTime periodStart,
        DateTime periodEnd,
        int? userId = null);

    /// <summary>
    /// Get all payments for a specific date range
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="paymentMethod">Optional: Filter by payment method</param>
    /// <returns>List of payments with transaction details</returns>
    Task<List<Payment>> GetPaymentsByDateRangeAsync(
        DateTime startDate,
        DateTime endDate,
        PaymentMethod? paymentMethod = null);

    /// <summary>
    /// Get all payments for a specific user/cashier
    /// </summary>
    /// <param name="userId">User/cashier ID</param>
    /// <param name="shiftStart">Shift start date/time</param>
    /// <param name="shiftEnd">Shift end date/time</param>
    /// <returns>List of payments processed by user during shift</returns>
    Task<List<Payment>> GetPaymentsByUserAsync(
        int userId,
        DateTime shiftStart,
        DateTime shiftEnd);

    // ========================================
    // CASH DRAWER RECONCILIATION
    // ========================================

    /// <summary>
    /// Get cash drawer reconciliation data for a shift
    /// </summary>
    /// <param name="userId">Cashier ID</param>
    /// <param name="shiftStart">Shift start date/time</param>
    /// <param name="shiftEnd">Shift end date/time</param>
    /// <param name="openingFloat">Opening cash float (e.g., R500)</param>
    /// <returns>Cash drawer reconciliation with expected vs actual cash</returns>
    /// <remarks>
    /// Calculates:
    /// - Opening Float: Starting cash in drawer
    /// - Cash Sales: Sum of all cash payments during shift
    /// - Cash Paid Out: Refunds, petty cash, etc.
    /// - Expected Cash: What SHOULD be in drawer
    /// - Variance: Difference between expected and actual
    ///
    /// Variance Severity Levels:
    /// - Balanced: < R0.01 (perfect)
    /// - Acceptable: +/- R1.00 (rounding)
    /// - Minor: +/- R10.00 (no action)
    /// - Moderate: +/- R100.00 (requires investigation)
    /// - Severe: > R100.00 (requires incident report)
    ///
    /// Manager Review Required: Variance > R10.00
    /// </remarks>
    Task<CashDrawerReconciliation> GetCashDrawerReconciliationAsync(
        int userId,
        DateTime shiftStart,
        DateTime shiftEnd,
        decimal openingFloat);

    /// <summary>
    /// Record cash drawer reconciliation results
    /// </summary>
    /// <param name="reconciliation">Completed reconciliation with actual cash count</param>
    /// <returns>Saved reconciliation record ID</returns>
    /// <remarks>
    /// Audit Compliance:
    /// - All reconciliations must be recorded
    /// - Variances must be explained and signed off
    /// - Pattern of variances may indicate training issues or fraud
    ///
    /// Required Data:
    /// - Actual cash counted
    /// - Closing float (cash left for next shift)
    /// - Amount to banking (deposited to safe/bank)
    /// - Variance explanation (if > R10)
    /// - Manager verification (if variance > R10)
    /// </remarks>
    Task<int> RecordCashDrawerReconciliationAsync(CashDrawerReconciliation reconciliation);

    /// <summary>
    /// Get reconciliation history for a cashier
    /// </summary>
    /// <param name="userId">Cashier ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>List of reconciliations with variance analysis</returns>
    /// <remarks>
    /// Used for:
    /// - Cashier performance tracking
    /// - Fraud detection (pattern analysis)
    /// - Manager oversight
    /// - Audit compliance
    /// </remarks>
    Task<List<CashDrawerReconciliation>> GetReconciliationHistoryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate);

    // ========================================
    // VARIANCE ANALYSIS & ALERTS
    // ========================================

    /// <summary>
    /// Get all reconciliations with variances exceeding threshold
    /// </summary>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <param name="varianceThreshold">Minimum variance amount (e.g., R10.00)</param>
    /// <returns>List of reconciliations requiring investigation</returns>
    Task<List<CashDrawerReconciliation>> GetVarianceAlertsAsync(
        DateTime startDate,
        DateTime endDate,
        decimal varianceThreshold = 10.00m);

    /// <summary>
    /// Get cashier variance summary (performance metrics)
    /// </summary>
    /// <param name="userId">Cashier ID</param>
    /// <param name="startDate">Start date</param>
    /// <param name="endDate">End date</param>
    /// <returns>Summary statistics (total reconciliations, avg variance, accuracy rate)</returns>
    Task<CashierVarianceSummary> GetCashierVarianceSummaryAsync(
        int userId,
        DateTime startDate,
        DateTime endDate);
}

/// <summary>
/// Summary statistics for a cashier's reconciliation performance
/// </summary>
public class CashierVarianceSummary
{
    public int CashierId { get; set; }
    public string CashierName { get; set; } = string.Empty;
    public int TotalReconciliations { get; set; }
    public int BalancedReconciliations { get; set; }
    public int ReconciliationsWithinTolerance { get; set; }
    public int ReconciliationsRequiringReview { get; set; }
    public decimal TotalVariance { get; set; }
    public decimal AverageVariance { get; set; }
    public decimal LargestVariance { get; set; }
    public decimal AccuracyRate => TotalReconciliations > 0
        ? (decimal)BalancedReconciliations / TotalReconciliations * 100
        : 0;
}
