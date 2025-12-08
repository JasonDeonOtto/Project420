using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service interface for POS payment reconciliation and cash drawer management
    /// </summary>
    /// <remarks>
    /// This service handles:
    /// 1. Cash drawer opening (float assignment)
    /// 2. Cash drawer reconciliation (end of shift/day)
    /// 3. Payment method reconciliation (cash, card, on-account)
    /// 4. Variance detection and reporting
    /// 5. Cash drawer closure and handover
    ///
    /// Compliance Requirements:
    /// - Cannabis Act: Cash handling must be auditable
    /// - SARS: All financial transactions must reconcile
    /// - POPIA: User accountability for cash handling
    ///
    /// Business Rules:
    /// - Each cashier has their own cash drawer session
    /// - Opening float must be counted and recorded
    /// - All transactions during session tracked
    /// - Closing count must match expected amount ± acceptable variance
    /// - Manager approval required for significant variances (> R50)
    /// - Complete audit trail for all cash movements
    /// </remarks>
    public interface IPaymentReconciliationService
    {
        /// <summary>
        /// Open a cash drawer for a new cashier session
        /// </summary>
        /// <param name="request">Opening request with float amount and cashier details</param>
        /// <returns>Cash drawer session details with unique session ID</returns>
        /// <remarks>
        /// Workflow:
        /// 1. Validate cashier credentials
        /// 2. Count opening float (cash on hand)
        /// 3. Create cash drawer session record
        /// 4. Record breakdown of denominations (notes/coins)
        /// 5. Generate session ID for tracking
        /// 6. Set session status to "Open"
        ///
        /// Compliance:
        /// - Opening float must be counted and verified
        /// - Cashier must sign off on float amount
        /// - Manager can verify large float amounts
        /// - All denominations recorded for accountability
        /// </remarks>
        Task<CashDrawerSessionDto> OpenCashDrawerAsync(CashDrawerOpenRequestDto request);

        /// <summary>
        /// Record a cash movement (addition or removal from drawer)
        /// </summary>
        /// <param name="sessionId">Active cash drawer session ID</param>
        /// <param name="request">Cash movement details (amount, reason, approver)</param>
        /// <returns>Updated cash drawer session with movement recorded</returns>
        /// <remarks>
        /// Use cases:
        /// - Cash drop (remove excess cash to safe)
        /// - Loan (add cash if running low)
        /// - Petty cash payment (remove for expenses)
        /// - Cash correction (adjust for errors)
        ///
        /// Business Rules:
        /// - Movements > R500 require manager approval
        /// - Reason must be documented
        /// - Movement must be witnessed/approved
        /// - Audit trail maintained
        /// </remarks>
        Task<CashDrawerSessionDto> RecordCashMovementAsync(int sessionId, CashMovementRequestDto request);

        /// <summary>
        /// Get current cash drawer session for a cashier
        /// </summary>
        /// <param name="cashierId">Cashier user ID</param>
        /// <returns>Active session or null if no open session</returns>
        /// <remarks>
        /// Returns:
        /// - Session ID and status
        /// - Opening float amount
        /// - Expected cash (float + sales - refunds ± movements)
        /// - Transaction count and totals by payment method
        /// - Cash movements during session
        /// </remarks>
        Task<CashDrawerSessionDto?> GetActiveCashDrawerSessionAsync(int cashierId);

        /// <summary>
        /// Calculate expected cash drawer totals for reconciliation
        /// </summary>
        /// <param name="sessionId">Cash drawer session ID</param>
        /// <returns>Expected amounts by payment method</returns>
        /// <remarks>
        /// Calculates:
        /// - Opening float
        /// - + Cash sales
        /// - - Cash refunds
        /// - + Cash movements (loans)
        /// - - Cash movements (drops)
        /// - = Expected cash on hand
        ///
        /// Also provides:
        /// - Card payment totals (for terminal reconciliation)
        /// - On-account totals (for AR verification)
        /// - Transaction counts by payment method
        /// </remarks>
        Task<ReconciliationExpectedDto> CalculateExpectedAmountsAsync(int sessionId);

        /// <summary>
        /// Reconcile and close a cash drawer session
        /// </summary>
        /// <param name="request">Closing request with counted amounts and variances</param>
        /// <returns>Reconciliation result with variance analysis</returns>
        /// <remarks>
        /// Workflow:
        /// 1. Get expected amounts (from transactions)
        /// 2. Count actual cash on hand (by denomination)
        /// 3. Calculate variance (actual - expected)
        /// 4. Analyze variance (acceptable/needs approval)
        /// 5. Record reconciliation results
        /// 6. Close session (if approved)
        /// 7. Generate reconciliation report
        ///
        /// Variance Rules:
        /// - Variance ≤ R10: Acceptable (auto-approve)
        /// - Variance > R10 and ≤ R50: Warning (requires reason)
        /// - Variance > R50: Requires manager approval
        /// - Consistent pattern of shortages: Flag for investigation
        ///
        /// Compliance:
        /// - All variances documented
        /// - Manager approval recorded
        /// - Complete audit trail
        /// - Denomination breakdown captured
        /// </remarks>
        Task<ReconciliationResultDto> ReconcileCashDrawerAsync(CashDrawerCloseRequestDto request);

        /// <summary>
        /// Get reconciliation history for reporting and audit
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <param name="cashierId">Optional cashier filter (null = all cashiers)</param>
        /// <returns>List of reconciliation results</returns>
        /// <remarks>
        /// Used for:
        /// - Daily reconciliation reports
        /// - Cashier performance analysis
        /// - Variance trend analysis
        /// - Audit trail review
        /// - SARS compliance reporting
        /// </remarks>
        Task<List<ReconciliationResultDto>> GetReconciliationHistoryAsync(
            DateTime startDate,
            DateTime endDate,
            int? cashierId = null);

        /// <summary>
        /// Get payment method breakdown for a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Payment totals by method (Cash, Card, On-Account, etc.)</returns>
        /// <remarks>
        /// Returns:
        /// - Total amount by payment method
        /// - Transaction count by payment method
        /// - Average transaction value by method
        /// - Refund totals by payment method
        ///
        /// Used for:
        /// - Daily sales reporting
        /// - Payment method analysis
        /// - Bank deposit preparation
        /// - Card terminal reconciliation
        /// </remarks>
        Task<PaymentMethodBreakdownDto> GetPaymentMethodBreakdownAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Validate cash drawer session before reconciliation
        /// </summary>
        /// <param name="sessionId">Session ID to validate</param>
        /// <returns>Validation result with warnings/errors</returns>
        /// <remarks>
        /// Checks:
        /// - Session exists and is open
        /// - No pending transactions
        /// - All payments recorded
        /// - No orphaned refunds
        /// - Expected amounts calculable
        ///
        /// Returns warnings for:
        /// - Unusually long session (> 12 hours)
        /// - Large transaction count (> 500)
        /// - Large cash amount (> R10,000)
        /// - Significant expected variance based on history
        /// </remarks>
        Task<ReconciliationValidationDto> ValidateReconciliationAsync(int sessionId);

        /// <summary>
        /// Generate cash denomination worksheet for counting
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <returns>Worksheet with denomination breakdown template</returns>
        /// <remarks>
        /// Provides template for counting:
        /// - R200 notes × ___ = R___
        /// - R100 notes × ___ = R___
        /// - R50 notes × ___ = R___
        /// - R20 notes × ___ = R___
        /// - R10 notes × ___ = R___
        /// - R5 coins × ___ = R___
        /// - R2 coins × ___ = R___
        /// - R1 coins × ___ = R___
        /// - R0.50 coins × ___ = R___
        /// - R0.20 coins × ___ = R___
        /// - R0.10 coins × ___ = R___
        /// - R0.05 coins × ___ = R___
        /// - Total: R___
        ///
        /// Also shows expected total for reference
        /// </remarks>
        Task<DenominationWorksheetDto> GenerateDenominationWorksheetAsync(int sessionId);
    }
}
