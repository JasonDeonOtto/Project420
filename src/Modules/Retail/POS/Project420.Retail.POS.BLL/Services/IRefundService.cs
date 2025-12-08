using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service interface for POS refund business logic
    /// </summary>
    /// <remarks>
    /// This service orchestrates the complete refund workflow:
    /// 1. Validate refund eligibility (transaction exists, within time window, valid amounts)
    /// 2. Verify manager approval if required (large amounts, outside window)
    /// 3. Generate unique refund transaction number
    /// 4. Calculate VAT and totals for refund items
    /// 5. Create refund transaction entities (header, details, payment)
    /// 6. Reverse inventory movements (return to stock)
    /// 7. Process refund payment (cash, card, account credit)
    /// 8. Persist to database via repository
    /// 9. Return refund result with receipt data
    ///
    /// Cannabis Act Compliance:
    /// - Maintains complete audit trail for all refunds
    /// - Tracks batch numbers for seed-to-sale traceability
    /// - Requires documented reason for all refunds
    /// - Manager approval for high-value or unusual refunds
    ///
    /// SARS VAT Compliance:
    /// - Accurate VAT reversal on refunded amounts
    /// - Proper documentation for VAT adjustments
    /// - VAT-inclusive refund calculations
    /// - Proper rounding and variance handling
    ///
    /// Business Rules:
    /// - 30-day refund window (manager override available)
    /// - Refund amount cannot exceed original transaction
    /// - Partial refunds supported (individual items or quantities)
    /// - Manager approval required for refunds > R1000
    /// - Inventory adjustments must maintain accuracy
    /// </remarks>
    public interface IRefundService
    {
        /// <summary>
        /// Process a complete POS refund
        /// </summary>
        /// <param name="request">Refund request with original transaction, items, and approval info</param>
        /// <returns>Refund result with transaction details and receipt data</returns>
        /// <remarks>
        /// Complete workflow:
        /// 1. Validate original transaction exists and is not voided
        /// 2. Verify refund window (30 days, or manager override)
        /// 3. Validate refund items match original transaction
        /// 4. Validate refund quantities don't exceed original quantities
        /// 5. Check manager approval if required:
        ///    - Amount > R1000
        ///    - Outside 30-day window
        ///    - Compliance-related refund
        /// 6. Generate refund transaction number (e.g., "REFUND-20251206-001")
        /// 7. Calculate line item totals with VAT reversal
        /// 8. Create refund transaction header, details, and payment entities
        /// 9. Reverse inventory movements (add back to stock)
        /// 10. Process refund payment (cash out, card reversal, account credit)
        /// 11. Save to database (atomic transaction)
        /// 12. Return result with receipt data
        ///
        /// Validation Rules:
        /// - Original transaction must exist and not be voided
        /// - All refund items must be from original transaction
        /// - Refund quantities cannot exceed original quantities
        /// - Manager approval required if:
        ///   * Refund amount > R1000
        ///   * Refund is outside 30-day window
        ///   * RefundReason is ComplianceIssue or ManagerOverride
        /// - Refund payment method must be appropriate
        /// - Notes required for certain refund reasons
        ///
        /// Error Handling:
        /// - Returns Success = false with error messages if validation fails
        /// - Transaction rollback if any step fails
        /// - Maintains data integrity across all entities
        /// </remarks>
        Task<RefundResultDto> ProcessRefundAsync(RefundRequestDto request);

        /// <summary>
        /// Validate if a transaction is eligible for refund
        /// </summary>
        /// <param name="transactionNumber">Original transaction number (e.g., "SALE-20251206-001")</param>
        /// <returns>Validation result with eligibility status and any warnings</returns>
        /// <remarks>
        /// Checks:
        /// 1. Transaction exists and is not voided
        /// 2. Transaction has not been fully refunded already
        /// 3. Transaction is within 30-day refund window (or note if outside)
        /// 4. Transaction payment method supports refunds
        ///
        /// Returns:
        /// - Eligible: true/false
        /// - Warnings: Array of issues (e.g., "Outside 30-day window - manager approval required")
        /// - OriginalTransaction: Details of original sale for refund processing
        /// </remarks>
        Task<RefundEligibilityDto> ValidateRefundEligibilityAsync(string transactionNumber);

        /// <summary>
        /// Get refund transaction details by refund transaction number
        /// </summary>
        /// <param name="refundTransactionNumber">Refund transaction number (e.g., "REFUND-20251206-001")</param>
        /// <returns>Refund details or null if not found</returns>
        /// <remarks>
        /// Used for:
        /// - Refund receipt reprinting
        /// - Audit trail review
        /// - Customer inquiry
        /// - Compliance reporting
        /// </remarks>
        Task<RefundResultDto?> GetRefundByTransactionNumberAsync(string refundTransactionNumber);

        /// <summary>
        /// Get all refunds for a specific original transaction
        /// </summary>
        /// <param name="originalTransactionNumber">Original sale transaction number</param>
        /// <returns>List of all refunds processed against this sale</returns>
        /// <remarks>
        /// Useful for:
        /// - Checking if sale has been partially refunded
        /// - Calculating remaining refundable amount
        /// - Audit trail for specific sale
        /// </remarks>
        Task<List<RefundResultDto>> GetRefundsForTransactionAsync(string originalTransactionNumber);

        /// <summary>
        /// Calculate refund preview without processing
        /// </summary>
        /// <param name="request">Refund request (without processing)</param>
        /// <returns>Preview of refund amounts and totals</returns>
        /// <remarks>
        /// Allows UI to show customer:
        /// - Exact refund amount they will receive
        /// - VAT breakdown
        /// - Individual item refund amounts
        ///
        /// Does NOT:
        /// - Create any database records
        /// - Modify inventory
        /// - Process any payments
        ///
        /// Use this for "what-if" scenarios before actual refund processing
        /// </remarks>
        Task<RefundPreviewDto> CalculateRefundPreviewAsync(RefundRequestDto request);
    }
}
