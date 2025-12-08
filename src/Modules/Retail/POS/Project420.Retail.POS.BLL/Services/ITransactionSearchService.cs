using Project420.Retail.POS.BLL.DTOs;

namespace Project420.Retail.POS.BLL.Services
{
    /// <summary>
    /// Service interface for advanced POS transaction search and filtering
    /// </summary>
    /// <remarks>
    /// This service provides powerful search capabilities for:
    /// 1. Transaction lookup by multiple criteria
    /// 2. Advanced filtering (date, customer, amount, payment method, product)
    /// 3. Batch number tracking (Cannabis traceability)
    /// 4. Customer purchase history
    /// 5. Product sales analysis
    /// 6. Transaction statistics and reporting
    ///
    /// Cannabis Act Compliance:
    /// - Batch number search for seed-to-sale traceability
    /// - Customer purchase history for compliance reporting
    /// - Complete audit trail for all searches
    ///
    /// SARS Compliance:
    /// - Transaction history for tax audits
    /// - VAT reporting by period
    /// - Payment method analysis
    ///
    /// Use Cases:
    /// - Customer service (find receipt, review purchase)
    /// - Inventory tracking (which batches were sold)
    /// - Compliance reporting (customer limits, product tracking)
    /// - Business intelligence (sales trends, popular products)
    /// - Fraud detection (unusual patterns)
    /// </remarks>
    public interface ITransactionSearchService
    {
        /// <summary>
        /// Search transactions with advanced filtering criteria
        /// </summary>
        /// <param name="criteria">Search criteria with filters and pagination</param>
        /// <returns>Paginated search results with matching transactions</returns>
        /// <remarks>
        /// Supports filtering by:
        /// - Transaction number (exact or partial match)
        /// - Date range (start/end dates)
        /// - Customer name or ID
        /// - Transaction type (Sale, Refund, Void)
        /// - Transaction status (Completed, Cancelled)
        /// - Amount range (min/max)
        /// - Payment method (Cash, Card, OnAccount)
        /// - Product ID or SKU
        /// - Batch number (Cannabis traceability)
        /// - Cashier/user ID
        ///
        /// Pagination:
        /// - Page number (1-based)
        /// - Page size (default 50, max 500)
        /// - Total count returned
        /// - Total pages calculated
        ///
        /// Sorting:
        /// - Transaction date (ascending/descending)
        /// - Transaction number
        /// - Total amount
        /// - Customer name
        /// </remarks>
        Task<TransactionSearchResultDto> SearchTransactionsAsync(TransactionSearchCriteriaDto criteria);

        /// <summary>
        /// Quick search by transaction number
        /// </summary>
        /// <param name="transactionNumber">Transaction number (exact match)</param>
        /// <returns>Transaction details or null if not found</returns>
        /// <remarks>
        /// Fast lookup for:
        /// - Receipt reprinting
        /// - Customer inquiries
        /// - Refund processing
        /// - Transaction review
        /// </remarks>
        Task<TransactionDetailDto?> GetTransactionByNumberAsync(string transactionNumber);

        /// <summary>
        /// Search transactions by customer
        /// </summary>
        /// <param name="customerName">Customer name (partial match supported)</param>
        /// <param name="debtorId">Optional debtor ID (exact match)</param>
        /// <param name="pageNumber">Page number (default 1)</param>
        /// <param name="pageSize">Items per page (default 50)</param>
        /// <returns>Paginated customer transaction history</returns>
        /// <remarks>
        /// Returns:
        /// - All transactions for customer
        /// - Ordered by date (newest first)
        /// - Includes sales and refunds
        /// - Shows total spent and transaction count
        ///
        /// Used for:
        /// - Customer service inquiries
        /// - Loyalty program tracking
        /// - Purchase pattern analysis
        /// - Cannabis compliance (purchase limits)
        /// </remarks>
        Task<TransactionSearchResultDto> SearchTransactionsByCustomerAsync(
            string? customerName = null,
            int? debtorId = null,
            int pageNumber = 1,
            int pageSize = 50);

        /// <summary>
        /// Search transactions by product
        /// </summary>
        /// <param name="productId">Product ID (exact match)</param>
        /// <param name="productSku">Product SKU (exact match)</param>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <returns>Transactions containing the specified product</returns>
        /// <remarks>
        /// Used for:
        /// - Sales analysis for specific product
        /// - Inventory tracking (who bought what)
        /// - Product recall tracing
        /// - Pricing analysis
        /// </remarks>
        Task<TransactionSearchResultDto> SearchTransactionsByProductAsync(
            int? productId = null,
            string? productSku = null,
            DateTime? startDate = null,
            DateTime? endDate = null);

        /// <summary>
        /// Search transactions by batch number (Cannabis traceability)
        /// </summary>
        /// <param name="batchNumber">Batch number to search for</param>
        /// <returns>All transactions containing products from this batch</returns>
        /// <remarks>
        /// Cannabis Act Requirement:
        /// - Track all sales from specific batch
        /// - Enable recall capability
        /// - Seed-to-sale traceability
        /// - Compliance reporting
        ///
        /// Returns:
        /// - Transaction details
        /// - Customer information
        /// - Quantity sold from batch
        /// - Sale dates
        ///
        /// Critical for:
        /// - Product recalls
        /// - Quality issues
        /// - Compliance audits
        /// - Batch performance analysis
        /// </remarks>
        Task<BatchTraceabilityDto> SearchTransactionsByBatchNumberAsync(string batchNumber);

        /// <summary>
        /// Get transaction statistics for a date range
        /// </summary>
        /// <param name="startDate">Start date (inclusive)</param>
        /// <param name="endDate">End date (inclusive)</param>
        /// <param name="groupBy">Optional grouping (Day, Week, Month, PaymentMethod, Cashier)</param>
        /// <returns>Aggregated transaction statistics</returns>
        /// <remarks>
        /// Returns:
        /// - Total transaction count
        /// - Total sales amount
        /// - Total refund amount
        /// - Net sales (sales - refunds)
        /// - Average transaction value
        /// - Transaction count by payment method
        /// - Sales by cashier (if grouped)
        /// - Daily/weekly/monthly totals (if grouped)
        ///
        /// Used for:
        /// - Sales reports
        /// - Performance dashboards
        /// - Business intelligence
        /// - Cashier performance analysis
        /// - Payment method trends
        /// </remarks>
        Task<TransactionStatisticsDto> GetTransactionStatisticsAsync(
            DateTime startDate,
            DateTime endDate,
            string? groupBy = null);

        /// <summary>
        /// Get recent transactions (useful for monitoring and dashboards)
        /// </summary>
        /// <param name="count">Number of recent transactions to return (default 20, max 100)</param>
        /// <param name="includeVoided">Include voided transactions (default false)</param>
        /// <returns>List of recent transactions</returns>
        /// <remarks>
        /// Returns most recent transactions ordered by date descending
        ///
        /// Used for:
        /// - Manager dashboards
        /// - Transaction monitoring
        /// - Activity overview
        /// - Quick access to latest sales
        /// </remarks>
        Task<List<TransactionSummaryDto>> GetRecentTransactionsAsync(int count = 20, bool includeVoided = false);

        /// <summary>
        /// Search for duplicate or suspicious transactions
        /// </summary>
        /// <param name="startDate">Start date to search</param>
        /// <param name="endDate">End date to search</param>
        /// <returns>Potentially duplicate or suspicious transactions</returns>
        /// <remarks>
        /// Detects:
        /// - Identical transactions (same items, amounts, within minutes)
        /// - Multiple voids by same cashier
        /// - Large refunds without corresponding sale
        /// - Unusual patterns (many small sales, many voids)
        ///
        /// Used for:
        /// - Fraud detection
        /// - Audit compliance
        /// - Training identification
        /// - Process improvement
        /// </remarks>
        Task<List<SuspiciousTransactionDto>> SearchSuspiciousTransactionsAsync(DateTime startDate, DateTime endDate);
    }
}
