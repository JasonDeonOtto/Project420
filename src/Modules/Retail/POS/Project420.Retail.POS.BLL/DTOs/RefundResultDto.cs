namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Result DTO for a processed refund
/// </summary>
/// <remarks>
/// Contains all information for displaying refund confirmation
/// and printing refund receipt
/// </remarks>
public class RefundResultDto
{
    // ========================================
    // OPERATION STATUS
    // ========================================

    /// <summary>
    /// Indicates if refund was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error messages if refund failed
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    // ========================================
    // REFUND TRANSACTION
    // ========================================

    /// <summary>
    /// Refund transaction number (new transaction)
    /// </summary>
    /// <remarks>
    /// Format: "REFUND-YYYYMMDD-XXX"
    /// Appears on refund receipt
    /// </remarks>
    public string? RefundTransactionNumber { get; set; }

    /// <summary>
    /// Original transaction number that was refunded
    /// </summary>
    public string? OriginalTransactionNumber { get; set; }

    /// <summary>
    /// Date/time refund was processed
    /// </summary>
    public DateTime RefundDate { get; set; }

    // ========================================
    // REFUND AMOUNTS
    // ========================================

    /// <summary>
    /// Refund subtotal (excluding VAT)
    /// </summary>
    public decimal RefundSubtotal { get; set; }

    /// <summary>
    /// VAT amount being refunded
    /// </summary>
    public decimal RefundVATAmount { get; set; }

    /// <summary>
    /// Total refund amount (including VAT)
    /// </summary>
    public decimal RefundTotalAmount { get; set; }

    // ========================================
    // REFUND ITEMS (for receipt)
    // ========================================

    /// <summary>
    /// Items that were refunded
    /// </summary>
    public List<RefundItemDto> RefundedItems { get; set; } = new();

    // ========================================
    // PAYMENT INFORMATION
    // ========================================

    /// <summary>
    /// How refund was issued (Cash, Card, OnAccount, etc.)
    /// </summary>
    public string? RefundPaymentMethod { get; set; }

    /// <summary>
    /// Payment reference number
    /// </summary>
    public string? PaymentReference { get; set; }

    // ========================================
    // USER INFORMATION (for receipt)
    // ========================================

    /// <summary>
    /// Customer name
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Cashier/user who processed refund
    /// </summary>
    public string? ProcessedByUserName { get; set; }

    /// <summary>
    /// Manager who approved refund (if applicable)
    /// </summary>
    public string? ApprovedByManagerName { get; set; }

    // ========================================
    // REFUND REASON (for audit)
    // ========================================

    /// <summary>
    /// Reason for refund
    /// </summary>
    public string? RefundReason { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }
}
