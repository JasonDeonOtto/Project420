using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service interface for POS transaction business logic
    /// </summary>
    /// <remarks>
    /// This service orchestrates the complete checkout workflow:
    /// 1. Validate checkout request (age verification, items, payment)
    /// 2. Generate unique transaction number
    /// 3. Calculate VAT and totals using VATCalculationService
    /// 4. Create transaction entities (header, details, payment)
    /// 5. Persist to database via repository
    /// 6. Return checkout result with receipt data
    ///
    /// Cannabis Act Compliance:
    /// - Enforces 18+ age verification
    /// - Tracks batch numbers for seed-to-sale traceability
    /// - Maintains audit trail
    ///
    /// SARS VAT Compliance:
    /// - Accurate 15% VAT calculation
    /// - VAT-inclusive pricing
    /// - Proper rounding and variance handling
    /// </remarks>
    public interface ITransactionService
    {
        /// <summary>
        /// Process a complete POS checkout
        /// </summary>
        /// <param name="request">Checkout request with cart items, customer, and payment info</param>
        /// <returns>Checkout result with transaction details and receipt data</returns>
        /// <remarks>
        /// Complete workflow:
        /// 1. Validate request (age verification, non-empty cart, valid payment)
        /// 2. Generate transaction number (e.g., "SALE-20251206-001")
        /// 3. Calculate line item totals with VAT
        /// 4. Aggregate header totals
        /// 5. Create transaction header, details, and payment entities
        /// 6. Save to database (atomic transaction)
        /// 7. Return result with receipt data
        ///
        /// Validation Rules:
        /// - Age must be verified (AgeVerified = true)
        /// - Cart must have at least 1 item
        /// - All product IDs must be valid
        /// - Payment method must be valid
        /// - For cash: AmountTendered must be >= TotalAmount
        /// </remarks>
        Task<CheckoutResultDto> ProcessCheckoutAsync(CheckoutRequestDto request);

        /// <summary>
        /// Get transaction details by transaction number (for receipt reprinting)
        /// </summary>
        /// <param name="transactionNumber">Transaction number (e.g., "SALE-20251206-001")</param>
        /// <returns>Transaction details or null if not found</returns>
        Task<CheckoutResultDto?> GetTransactionByNumberAsync(string transactionNumber);

        /// <summary>
        /// Void/cancel a transaction (manager override required)
        /// </summary>
        /// <param name="transactionNumber">Transaction number to void</param>
        /// <param name="voidReason">Reason for voiding (audit requirement)</param>
        /// <param name="userId">User ID performing the void</param>
        /// <returns>True if voided successfully, false if not found or already voided</returns>
        Task<bool> VoidTransactionAsync(string transactionNumber, string voidReason, int userId);

        // ========================================
        // PHASE 9.6: TRANSACTION CANCELLATION
        // ========================================

        /// <summary>
        /// Validate if a transaction is eligible for cancellation
        /// </summary>
        /// <param name="transactionNumber">Transaction number to check</param>
        /// <returns>Eligibility details including whether manager approval is required</returns>
        /// <remarks>
        /// Phase 9.6: Transaction Cancellation
        /// - Checks if transaction exists and is in a cancellable state
        /// - Determines if manager approval is required based on:
        ///   - Time since transaction (> 30 minutes = manager required)
        ///   - Transaction status (Completed = manager required)
        ///   - Amount threshold (> R1000 = manager required)
        /// </remarks>
        Task<CancellationEligibilityDto> ValidateCancellationEligibilityAsync(string transactionNumber);

        /// <summary>
        /// Process a transaction cancellation
        /// </summary>
        /// <param name="request">Cancellation request with reason and approvals</param>
        /// <returns>Cancellation result with movement reversal details</returns>
        /// <remarks>
        /// Phase 9.6: Transaction Cancellation
        /// For completed transactions:
        /// 1. Validate manager approval (if required)
        /// 2. Reverse stock movements (soft delete in Movement ledger)
        /// 3. Update transaction status to Cancelled
        /// 4. Log cancellation reason and approver for audit
        /// 5. Return payment reversal requirements
        /// </remarks>
        Task<CancellationResultDto> ProcessCancellationAsync(CancellationRequestDto request);

        /// <summary>
        /// Validate manager PIN for authorization
        /// </summary>
        /// <param name="managerId">Manager user ID</param>
        /// <param name="pin">Manager PIN</param>
        /// <returns>Validation result with manager name if valid</returns>
        /// <remarks>
        /// Phase 9.6: Manager Override
        /// Simple PIN validation for manager authorization.
        /// In production, this should integrate with proper authentication.
        /// Current implementation uses a simple PIN stored in configuration.
        /// </remarks>
        Task<ManagerValidationDto> ValidateManagerPinAsync(int managerId, string pin);
    }
}
