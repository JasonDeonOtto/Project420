using Project420.Retail.POS.Models.Entities;
using Project420.Shared.Infrastructure.DTOs;
using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.DAL.Repositories
{
    /// <summary>
    /// Repository interface for POS transaction operations (CRUD + business queries)
    /// </summary>
    /// <remarks>
    /// Cannabis Act Compliance:
    /// - All sales must be traceable (batch numbers, timestamps)
    /// - Age verification data must be captured
    /// - Audit trails required for 7 years (POPIA + SARS)
    ///
    /// SARS VAT Compliance:
    /// - VAT calculations must be accurate and auditable
    /// - Transaction numbers must be sequential and unique
    /// - All financial data retained for 5+ years
    /// </remarks>
    public interface ITransactionRepository
    {
        // ========================================
        // CREATE OPERATIONS
        // ========================================

        /// <summary>
        /// Create a new POS sale transaction with details and payment
        /// </summary>
        /// <param name="header">Transaction header with customer, totals, etc.</param>
        /// <param name="details">List of line items (products sold)</param>
        /// <param name="payment">Payment information (method, amount, reference)</param>
        /// <returns>Created transaction header with assigned ID and transaction number</returns>
        /// <remarks>
        /// Transactional operation - all 3 entities saved atomically.
        /// Rollback on any failure to maintain data integrity.
        ///
        /// Cannabis Act Requirements:
        /// - Age verification must be completed before sale
        /// - Batch numbers must be recorded for traceability
        /// - Medical license verified if applicable
        /// </remarks>
        Task<POSTransactionHeader> CreateSaleAsync(
            POSTransactionHeader header,
            List<POSTransactionDetail> details,
            Payment payment);

        // ========================================
        // READ OPERATIONS
        // ========================================

        /// <summary>
        /// Get transaction by ID with all related entities
        /// </summary>
        /// <param name="transactionId">Transaction header ID</param>
        /// <returns>Transaction header with details and payment, or null if not found</returns>
        Task<POSTransactionHeader?> GetByIdAsync(int transactionId);

        /// <summary>
        /// Get transaction by transaction number (e.g., "SALE-20251206-001")
        /// </summary>
        /// <param name="transactionNumber">Unique transaction number</param>
        /// <returns>Transaction header with details and payment, or null if not found</returns>
        Task<POSTransactionHeader?> GetByTransactionNumberAsync(string transactionNumber);

        /// <summary>
        /// Get all transactions for a specific customer
        /// </summary>
        /// <param name="debtorId">Customer ID</param>
        /// <param name="pageNumber">Page number (1-based)</param>
        /// <param name="pageSize">Items per page (default 50)</param>
        /// <returns>List of transactions ordered by date descending (newest first)</returns>
        Task<List<POSTransactionHeader>> GetByCustomerAsync(int debtorId, int pageNumber = 1, int pageSize = 50);

        /// <summary>
        /// Get all transactions for today (useful for daily sales reports)
        /// </summary>
        /// <returns>List of today's transactions</returns>
        Task<List<POSTransactionHeader>> GetTodaysTransactionsAsync();

        /// <summary>
        /// Get transactions within a date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <returns>List of transactions in date range</returns>
        Task<List<POSTransactionHeader>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        // ========================================
        // UPDATE OPERATIONS
        // ========================================

        /// <summary>
        /// Void/cancel a transaction (sets status to Cancelled)
        /// </summary>
        /// <param name="transactionId">Transaction ID to void</param>
        /// <param name="voidReason">Reason for voiding (audit requirement)</param>
        /// <param name="userId">User ID performing the void (audit requirement)</param>
        /// <returns>True if voided successfully, false if not found or already voided</returns>
        /// <remarks>
        /// Compliance Note: Voided transactions must be retained for audit purposes.
        /// Never delete - only mark as Cancelled with audit trail.
        /// </remarks>
        Task<bool> VoidTransactionAsync(int transactionId, string voidReason, int userId);

        // ========================================
        // REPORTING QUERIES
        // ========================================

        /// <summary>
        /// Get total sales amount for a date range (for reporting)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Total sales amount (excluding voided/cancelled transactions)</returns>
        Task<decimal> GetTotalSalesAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get VAT summary for a date range (for VAT201 reporting to SARS)
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Tuple of (Subtotal, VATAmount, Total)</returns>
        /// <remarks>
        /// Used for monthly/quarterly VAT201 returns to SARS.
        /// Must exclude voided/cancelled transactions.
        /// </remarks>
        Task<(decimal Subtotal, decimal VATAmount, decimal Total)> GetVATSummaryAsync(DateTime startDate, DateTime endDate);

        // ========================================
        // REFUND OPERATIONS (High Priority Feature #1)
        // ========================================

        /// <summary>
        /// Process a refund transaction for a previous sale
        /// </summary>
        /// <param name="originalTransactionNumber">Transaction number being refunded</param>
        /// <param name="refundHeader">Refund transaction header</param>
        /// <param name="refundDetails">Refund line items</param>
        /// <param name="payment">Refund payment (negative amount)</param>
        /// <param name="refundReason">Reason for refund (compliance requirement)</param>
        /// <returns>Created refund transaction header</returns>
        /// <remarks>
        /// Cannabis Act Compliance:
        /// - Refunds must reference original sale for traceability
        /// - Refunded products returned to inventory with batch tracking
        /// - Manager approval required for refunds > R1000
        ///
        /// Business Rules:
        /// - Cannot refund more than original sale amount
        /// - Refund window: 30 days from original sale
        /// - Partial refunds supported
        /// </remarks>
        Task<POSTransactionHeader> ProcessRefundAsync(
            string originalTransactionNumber,
            POSTransactionHeader refundHeader,
            List<POSTransactionDetail> refundDetails,
            Payment payment,
            RefundReason refundReason);

        /// <summary>
        /// Validate if a refund can be processed for a transaction
        /// </summary>
        /// <param name="originalTransactionNumber">Transaction to be refunded</param>
        /// <param name="refundAmount">Amount to refund</param>
        /// <returns>Validation result with errors if invalid</returns>
        Task<RefundValidationResult> ValidateRefundAsync(string originalTransactionNumber, decimal refundAmount);

        /// <summary>
        /// Get original transaction for refund with refund history
        /// </summary>
        /// <param name="transactionNumber">Original transaction number</param>
        /// <returns>Transaction with all previous refunds applied</returns>
        Task<POSTransactionHeader?> GetRefundableTransactionAsync(string transactionNumber);

        /// <summary>
        /// Get all refunds processed for a specific transaction
        /// </summary>
        /// <param name="originalTransactionNumber">Original sale transaction number</param>
        /// <returns>List of refund transactions</returns>
        Task<List<POSTransactionHeader>> GetRefundHistoryAsync(string originalTransactionNumber);

        // ========================================
        // ADVANCED SEARCH & FILTERING (High Priority Feature #2)
        // ========================================

        /// <summary>
        /// Search transactions with advanced filtering criteria
        /// </summary>
        /// <param name="criteria">Search criteria (date range, customer, amount, payment method, etc.)</param>
        /// <returns>Paginated result with matching transactions</returns>
        /// <remarks>
        /// Supports filtering by:
        /// - Date range
        /// - Customer
        /// - Transaction type (Sale, Refund, etc.)
        /// - Transaction status
        /// - Amount range
        /// - Payment method
        /// - Product ID
        /// - Batch number (Cannabis traceability)
        /// - User/cashier
        ///
        /// Cannabis Compliance: Batch number search enables seed-to-sale traceability
        /// </remarks>
        Task<PagedResult<POSTransactionHeader>> SearchTransactionsAsync(TransactionSearchCriteria criteria);

        /// <summary>
        /// Get detailed statistics for transactions in a date range
        /// </summary>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <returns>Transaction statistics (count, totals, averages by type/status/method)</returns>
        Task<TransactionStatistics> GetTransactionStatisticsAsync(DateTime startDate, DateTime endDate);
    }
}
