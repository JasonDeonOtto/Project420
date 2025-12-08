namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Statistical summary of transactions for a date range
/// </summary>
/// <remarks>
/// Used for:
/// - Manager dashboards
/// - Daily/weekly/monthly reports
/// - Business intelligence
/// - Performance analysis
/// - Compliance reporting (SAHPRA, SARS)
/// </remarks>
public class TransactionStatistics
{
    /// <summary>
    /// Start of reporting period
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// End of reporting period
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    // ========================================
    // TRANSACTION COUNTS
    // ========================================

    /// <summary>
    /// Total number of transactions (all types, all statuses)
    /// </summary>
    public int TotalTransactionCount { get; set; }

    /// <summary>
    /// Number of sales transactions
    /// </summary>
    public int SalesCount { get; set; }

    /// <summary>
    /// Number of refund transactions
    /// </summary>
    public int RefundsCount { get; set; }

    /// <summary>
    /// Number of completed transactions
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// Number of cancelled/voided transactions
    /// </summary>
    public int CancelledCount { get; set; }

    // ========================================
    // FINANCIAL TOTALS
    // ========================================

    /// <summary>
    /// Total sales amount (excluding VAT)
    /// </summary>
    public decimal TotalSalesSubtotal { get; set; }

    /// <summary>
    /// Total VAT amount
    /// </summary>
    public decimal TotalVATAmount { get; set; }

    /// <summary>
    /// Total sales including VAT
    /// </summary>
    public decimal TotalSalesIncludingVAT { get; set; }

    /// <summary>
    /// Total refunds amount
    /// </summary>
    public decimal TotalRefundsAmount { get; set; }

    /// <summary>
    /// Net sales (sales - refunds)
    /// </summary>
    public decimal NetSalesAmount => TotalSalesIncludingVAT - TotalRefundsAmount;

    // ========================================
    // AVERAGES
    // ========================================

    /// <summary>
    /// Average transaction value
    /// </summary>
    public decimal AverageTransactionValue => CompletedCount > 0
        ? TotalSalesIncludingVAT / CompletedCount
        : 0;

    /// <summary>
    /// Average items per transaction
    /// </summary>
    public decimal AverageItemsPerTransaction { get; set; }

    // ========================================
    // PAYMENT METHOD BREAKDOWN
    // ========================================

    /// <summary>
    /// Total cash payments
    /// </summary>
    public decimal CashPaymentTotal { get; set; }

    /// <summary>
    /// Total card payments
    /// </summary>
    public decimal CardPaymentTotal { get; set; }

    /// <summary>
    /// Total EFT payments
    /// </summary>
    public decimal EFTPaymentTotal { get; set; }

    /// <summary>
    /// Total mobile payments
    /// </summary>
    public decimal MobilePaymentTotal { get; set; }

    /// <summary>
    /// Total on-account payments
    /// </summary>
    public decimal OnAccountPaymentTotal { get; set; }

    /// <summary>
    /// Total voucher payments
    /// </summary>
    public decimal VoucherPaymentTotal { get; set; }

    // ========================================
    // PAYMENT METHOD COUNTS
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
    // PEAK ANALYSIS
    // ========================================

    /// <summary>
    /// Highest transaction value in period
    /// </summary>
    public decimal HighestTransactionValue { get; set; }

    /// <summary>
    /// Lowest transaction value in period (excluding R0)
    /// </summary>
    public decimal LowestTransactionValue { get; set; }

    /// <summary>
    /// Most popular payment method by transaction count
    /// </summary>
    public string? MostPopularPaymentMethod { get; set; }

    // ========================================
    // USER/CASHIER BREAKDOWN
    // ========================================

    /// <summary>
    /// Number of unique users/cashiers who processed transactions
    /// </summary>
    public int UniqueCashierCount { get; set; }

    /// <summary>
    /// Top performing cashier by transaction count
    /// </summary>
    public string? TopCashierByCount { get; set; }

    /// <summary>
    /// Top performing cashier by sales value
    /// </summary>
    public string? TopCashierByValue { get; set; }

    // ========================================
    // HELPER METHODS
    // ========================================

    /// <summary>
    /// Get percentage of sales by payment method
    /// </summary>
    public decimal GetPaymentMethodPercentage(decimal methodTotal)
    {
        return TotalSalesIncludingVAT > 0
            ? (methodTotal / TotalSalesIncludingVAT) * 100
            : 0;
    }

    /// <summary>
    /// Get refund rate (refunds / sales)
    /// </summary>
    public decimal RefundRate => SalesCount > 0
        ? (decimal)RefundsCount / SalesCount * 100
        : 0;

    /// <summary>
    /// Get cancellation rate (cancelled / total)
    /// </summary>
    public decimal CancellationRate => TotalTransactionCount > 0
        ? (decimal)CancelledCount / TotalTransactionCount * 100
        : 0;
}
