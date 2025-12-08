using Project420.Shared.Core.Enums;

namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Search criteria for filtering transactions
/// </summary>
/// <remarks>
/// Used for:
/// - Transaction history search
/// - Daily/monthly sales reports
/// - Customer purchase history
/// - Audit trail queries
/// - Manager dashboards
///
/// Pattern: Nullable properties = optional filters
/// - If null, filter is not applied
/// - If set, filter is applied to query
/// </remarks>
public class TransactionSearchCriteria
{
    // ========================================
    // DATE FILTERS
    // ========================================

    /// <summary>
    /// Start date for transaction search (inclusive)
    /// </summary>
    /// <remarks>
    /// If null, no lower date bound.
    /// If set, only transactions on or after this date.
    /// </remarks>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date for transaction search (inclusive)
    /// </summary>
    /// <remarks>
    /// If null, no upper date bound.
    /// If set, only transactions on or before this date.
    /// </remarks>
    public DateTime? EndDate { get; set; }

    // ========================================
    // CUSTOMER FILTERS
    // ========================================

    /// <summary>
    /// Filter by specific customer ID
    /// </summary>
    /// <remarks>
    /// Used for customer purchase history.
    /// If null, returns transactions for all customers.
    /// </remarks>
    public int? CustomerId { get; set; }

    // ========================================
    // TRANSACTION FILTERS
    // ========================================

    /// <summary>
    /// Search by exact transaction number
    /// </summary>
    /// <remarks>
    /// Format: "SALE-20251206-001"
    /// Used for receipt lookup/reprinting.
    /// </remarks>
    public string? TransactionNumber { get; set; }

    /// <summary>
    /// Filter by transaction type (Sale, Refund, etc.)
    /// </summary>
    public TransactionType? TransactionType { get; set; }

    /// <summary>
    /// Filter by transaction status (Completed, Cancelled, etc.)
    /// </summary>
    public TransactionStatus? Status { get; set; }

    // ========================================
    // AMOUNT FILTERS
    // ========================================

    /// <summary>
    /// Minimum transaction amount (inclusive)
    /// </summary>
    /// <remarks>
    /// Used for large transaction reports.
    /// Example: MinAmount = 1000 returns transactions >= R1000
    /// </remarks>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Maximum transaction amount (inclusive)
    /// </summary>
    /// <remarks>
    /// Used for small transaction reports.
    /// Example: MaxAmount = 100 returns transactions <= R100
    /// </remarks>
    public decimal? MaxAmount { get; set; }

    // ========================================
    // PAYMENT FILTERS
    // ========================================

    /// <summary>
    /// Filter by payment method (Cash, Card, etc.)
    /// </summary>
    /// <remarks>
    /// Used for cash drawer reconciliation.
    /// Example: PaymentMethod = Cash returns only cash transactions
    /// </remarks>
    public PaymentMethod? PaymentMethod { get; set; }

    // ========================================
    // PRODUCT FILTERS (Cannabis Traceability)
    // ========================================

    /// <summary>
    /// Filter by specific product ID
    /// </summary>
    /// <remarks>
    /// Used for product sales history.
    /// Returns all transactions containing this product.
    /// </remarks>
    public int? ProductId { get; set; }

    /// <summary>
    /// Filter by batch number (Cannabis Act compliance)
    /// </summary>
    /// <remarks>
    /// Cannabis Compliance: Seed-to-sale traceability.
    /// Used to track all sales of a specific batch.
    /// Example: "BATCH-20251206-001"
    /// </remarks>
    public string? BatchNumber { get; set; }

    // ========================================
    // USER FILTERS (Audit)
    // ========================================

    /// <summary>
    /// Filter by user/cashier who processed the transaction
    /// </summary>
    /// <remarks>
    /// Used for:
    /// - Cashier performance reports
    /// - Audit trails
    /// - Manager oversight
    /// </remarks>
    public int? ProcessedByUserId { get; set; }

    // ========================================
    // PAGINATION
    // ========================================

    /// <summary>
    /// Page number (1-based)
    /// </summary>
    /// <remarks>
    /// Default: 1 (first page)
    /// Must be >= 1
    /// </remarks>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of results per page
    /// </summary>
    /// <remarks>
    /// Default: 50
    /// Range: 1-100
    /// </remarks>
    public int PageSize { get; set; } = 50;

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Validates search criteria
    /// </summary>
    public bool IsValid(out List<string> errors)
    {
        errors = new List<string>();

        if (StartDate.HasValue && EndDate.HasValue && EndDate < StartDate)
        {
            errors.Add("End date must be >= start date");
        }

        if (MinAmount.HasValue && MaxAmount.HasValue && MaxAmount < MinAmount)
        {
            errors.Add("Max amount must be >= min amount");
        }

        if (PageNumber < 1)
        {
            errors.Add("Page number must be >= 1");
        }

        if (PageSize < 1 || PageSize > 100)
        {
            errors.Add("Page size must be between 1 and 100");
        }

        return errors.Count == 0;
    }

    /// <summary>
    /// Calculates number of records to skip for pagination
    /// </summary>
    public int GetSkipCount() => (PageNumber - 1) * PageSize;
}
