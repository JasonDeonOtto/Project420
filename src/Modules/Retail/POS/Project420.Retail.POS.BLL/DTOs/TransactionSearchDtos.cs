using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs;

// ========================================
// SEARCH CRITERIA DTOs
// ========================================

/// <summary>
/// DTO for transaction search criteria
/// </summary>
public class TransactionSearchCriteriaDto
{
    // ========================================
    // BASIC FILTERS
    // ========================================

    /// <summary>
    /// Transaction number (exact or partial match)
    /// </summary>
    public string? TransactionNumber { get; set; }

    /// <summary>
    /// Start date (inclusive)
    /// </summary>
    public DateTime? StartDate { get; set; }

    /// <summary>
    /// End date (inclusive)
    /// </summary>
    public DateTime? EndDate { get; set; }

    // ========================================
    // CUSTOMER FILTERS
    // ========================================

    /// <summary>
    /// Customer name (partial match)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Debtor ID (exact match)
    /// </summary>
    public int? DebtorId { get; set; }

    // ========================================
    // TRANSACTION FILTERS
    // ========================================

    /// <summary>
    /// Transaction type (Sale, Refund, etc.)
    /// </summary>
    public TransactionTypeCode? TransactionType { get; set; }

    /// <summary>
    /// Transaction status (Completed, Cancelled, etc.)
    /// </summary>
    public TransactionStatus? Status { get; set; }

    // ========================================
    // AMOUNT FILTERS
    // ========================================

    /// <summary>
    /// Minimum amount
    /// </summary>
    public decimal? MinAmount { get; set; }

    /// <summary>
    /// Maximum amount
    /// </summary>
    public decimal? MaxAmount { get; set; }

    // ========================================
    // PAYMENT FILTERS
    // ========================================

    /// <summary>
    /// Payment method (Cash, Card, OnAccount)
    /// </summary>
    public PaymentMethod? PaymentMethod { get; set; }

    // ========================================
    // PRODUCT FILTERS
    // ========================================

    /// <summary>
    /// Product ID (exact match)
    /// </summary>
    public int? ProductId { get; set; }

    /// <summary>
    /// Product SKU (exact match)
    /// </summary>
    public string? ProductSku { get; set; }

    /// <summary>
    /// Batch number (Cannabis traceability)
    /// </summary>
    public string? BatchNumber { get; set; }

    // ========================================
    // USER FILTERS
    // ========================================

    /// <summary>
    /// Cashier/user ID
    /// </summary>
    public int? CashierId { get; set; }

    // ========================================
    // PAGINATION & SORTING
    // ========================================

    /// <summary>
    /// Page number (1-based, default 1)
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size (default 50, max 500)
    /// </summary>
    public int PageSize { get; set; } = 50;

    /// <summary>
    /// Sort field (TransactionDate, TransactionNumber, TotalAmount, CustomerName)
    /// </summary>
    public string SortField { get; set; } = "TransactionDate";

    /// <summary>
    /// Sort direction (Ascending, Descending)
    /// </summary>
    public string SortDirection { get; set; } = "Descending";
}

// ========================================
// SEARCH RESULT DTOs
// ========================================

/// <summary>
/// DTO for transaction search results
/// </summary>
public class TransactionSearchResultDto
{
    /// <summary>
    /// Matching transactions
    /// </summary>
    public List<TransactionSummaryDto> Transactions { get; set; } = new();

    /// <summary>
    /// Total count of matching transactions (all pages)
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Total pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Current page number
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Page size
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Has previous page
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// Has next page
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;

    /// <summary>
    /// Summary statistics for search results
    /// </summary>
    public SearchSummaryDto? Summary { get; set; }
}

/// <summary>
/// Summary transaction DTO (lightweight for list views)
/// </summary>
public class TransactionSummaryDto
{
    /// <summary>
    /// Transaction ID
    /// </summary>
    public int TransactionId { get; set; }

    /// <summary>
    /// Transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Transaction type (Sale, Refund, etc.)
    /// </summary>
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>
    /// Transaction status
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Total amount (including VAT)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment method
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Item count
    /// </summary>
    public int ItemCount { get; set; }
}

/// <summary>
/// Detailed transaction DTO (full details for single transaction view)
/// </summary>
public class TransactionDetailDto
{
    /// <summary>
    /// Transaction header information
    /// </summary>
    public TransactionSummaryDto Header { get; set; } = new();

    /// <summary>
    /// Line items
    /// </summary>
    public List<TransactionLineItemDto> LineItems { get; set; } = new();

    /// <summary>
    /// Payment details
    /// </summary>
    public List<PaymentDetailDto> Payments { get; set; } = new();

    /// <summary>
    /// Subtotal (excluding VAT)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// VAT amount
    /// </summary>
    public decimal VATAmount { get; set; }

    /// <summary>
    /// Discount amount
    /// </summary>
    public decimal DiscountAmount { get; set; }

    /// <summary>
    /// Total amount (including VAT)
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Related transactions (e.g., original sale for refund)
    /// </summary>
    public List<RelatedTransactionDto> RelatedTransactions { get; set; } = new();
}

/// <summary>
/// Transaction line item DTO
/// </summary>
public class TransactionLineItemDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Unit price (including VAT)
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Line subtotal (excluding VAT)
    /// </summary>
    public decimal Subtotal { get; set; }

    /// <summary>
    /// VAT amount
    /// </summary>
    public decimal VATAmount { get; set; }

    /// <summary>
    /// Line total (including VAT)
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Batch number (Cannabis traceability)
    /// </summary>
    public string? BatchNumber { get; set; }
}

/// <summary>
/// Payment detail DTO
/// </summary>
public class PaymentDetailDto
{
    /// <summary>
    /// Payment method
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Payment amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Payment reference
    /// </summary>
    public string? PaymentReference { get; set; }

    /// <summary>
    /// Payment date
    /// </summary>
    public DateTime PaymentDate { get; set; }
}

/// <summary>
/// Related transaction DTO
/// </summary>
public class RelatedTransactionDto
{
    /// <summary>
    /// Related transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Relationship type (OriginalSale, Refund, Void)
    /// </summary>
    public string RelationshipType { get; set; } = string.Empty;

    /// <summary>
    /// Related amount
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Related transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }
}

/// <summary>
/// Search summary statistics
/// </summary>
public class SearchSummaryDto
{
    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Total refund amount
    /// </summary>
    public decimal TotalRefunds { get; set; }

    /// <summary>
    /// Net amount (sales - refunds)
    /// </summary>
    public decimal NetAmount { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue { get; set; }

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Unique customer count
    /// </summary>
    public int UniqueCustomerCount { get; set; }
}

// ========================================
// BATCH TRACEABILITY DTOs
// ========================================

/// <summary>
/// DTO for batch traceability search results
/// </summary>
public class BatchTraceabilityDto
{
    /// <summary>
    /// Batch number
    /// </summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>
    /// Product information
    /// </summary>
    public ProductInfoDto? ProductInfo { get; set; }

    /// <summary>
    /// Transactions containing this batch
    /// </summary>
    public List<BatchTransactionDto> Transactions { get; set; } = new();

    /// <summary>
    /// Total quantity sold from this batch
    /// </summary>
    public int TotalQuantitySold { get; set; }

    /// <summary>
    /// Total sales amount from this batch
    /// </summary>
    public decimal TotalSalesAmount { get; set; }

    /// <summary>
    /// First sale date
    /// </summary>
    public DateTime? FirstSaleDate { get; set; }

    /// <summary>
    /// Last sale date
    /// </summary>
    public DateTime? LastSaleDate { get; set; }

    /// <summary>
    /// Unique customer count
    /// </summary>
    public int UniqueCustomerCount { get; set; }
}

/// <summary>
/// Product info for batch traceability
/// </summary>
public class ProductInfoDto
{
    /// <summary>
    /// Product ID
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product SKU
    /// </summary>
    public string ProductSku { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Product category
    /// </summary>
    public string? Category { get; set; }
}

/// <summary>
/// Transaction containing a specific batch
/// </summary>
public class BatchTransactionDto
{
    /// <summary>
    /// Transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Quantity sold from batch in this transaction
    /// </summary>
    public int QuantitySold { get; set; }

    /// <summary>
    /// Amount for items from this batch
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;
}

// ========================================
// STATISTICS DTOs
// ========================================

/// <summary>
/// DTO for transaction statistics
/// </summary>
public class TransactionStatisticsDto
{
    /// <summary>
    /// Date range
    /// </summary>
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    /// <summary>
    /// Total transaction count
    /// </summary>
    public int TotalTransactionCount { get; set; }

    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal TotalSalesAmount { get; set; }

    /// <summary>
    /// Total refund amount
    /// </summary>
    public decimal TotalRefundAmount { get; set; }

    /// <summary>
    /// Net sales (sales - refunds)
    /// </summary>
    public decimal NetSales { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue { get; set; }

    /// <summary>
    /// Average items per transaction
    /// </summary>
    public decimal AverageItemsPerTransaction { get; set; }

    /// <summary>
    /// Statistics by payment method
    /// </summary>
    public Dictionary<string, PaymentMethodStats> PaymentMethodStatistics { get; set; } = new();

    /// <summary>
    /// Statistics by cashier (if grouped)
    /// </summary>
    public Dictionary<string, CashierStats>? CashierStatistics { get; set; }

    /// <summary>
    /// Daily/weekly/monthly statistics (if grouped)
    /// </summary>
    public List<PeriodStats>? PeriodStatistics { get; set; }
}

/// <summary>
/// Payment method statistics
/// </summary>
public class PaymentMethodStats
{
    /// <summary>
    /// Payment method name
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageValue { get; set; }

    /// <summary>
    /// Percentage of total
    /// </summary>
    public decimal PercentageOfTotal { get; set; }
}

/// <summary>
/// Cashier statistics
/// </summary>
public class CashierStats
{
    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue { get; set; }
}

/// <summary>
/// Period statistics (day/week/month)
/// </summary>
public class PeriodStats
{
    /// <summary>
    /// Period start date
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// Period end date
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// Period label (e.g., "2025-12-06", "Week 49", "December 2025")
    /// </summary>
    public string PeriodLabel { get; set; } = string.Empty;

    /// <summary>
    /// Transaction count
    /// </summary>
    public int TransactionCount { get; set; }

    /// <summary>
    /// Total sales amount
    /// </summary>
    public decimal TotalSales { get; set; }

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue { get; set; }
}

// ========================================
// SUSPICIOUS TRANSACTION DTOs
// ========================================

/// <summary>
/// DTO for suspicious transaction detection
/// </summary>
public class SuspiciousTransactionDto
{
    /// <summary>
    /// Transaction number
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Suspicion reasons
    /// </summary>
    public List<string> SuspicionReasons { get; set; } = new();

    /// <summary>
    /// Risk level (Low, Medium, High)
    /// </summary>
    public string RiskLevel { get; set; } = string.Empty;

    /// <summary>
    /// Cashier name
    /// </summary>
    public string CashierName { get; set; } = string.Empty;

    /// <summary>
    /// Customer name
    /// </summary>
    public string CustomerName { get; set; } = string.Empty;

    /// <summary>
    /// Total amount
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Related transaction numbers (potential duplicates)
    /// </summary>
    public List<string> RelatedTransactions { get; set; } = new();
}
