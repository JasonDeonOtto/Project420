namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// DTO for refund preview calculations (before actual processing)
/// </summary>
/// <remarks>
/// Provides a "what-if" calculation of refund amounts
/// without creating any database records or modifying data
/// </remarks>
public class RefundPreviewDto
{
    // ========================================
    // VALIDATION
    // ========================================

    /// <summary>
    /// Indicates if refund request is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors (if IsValid = false)
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Warnings that don't prevent refund but need attention
    /// </summary>
    public List<string> Warnings { get; set; } = new();

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
    // ITEM BREAKDOWN
    // ========================================

    /// <summary>
    /// Detailed breakdown of refund amounts per item
    /// </summary>
    public List<RefundPreviewItemDto> ItemBreakdown { get; set; } = new();

    // ========================================
    // APPROVAL REQUIREMENTS
    // ========================================

    /// <summary>
    /// Indicates if manager approval is required for this refund
    /// </summary>
    public bool RequiresManagerApproval { get; set; }

    /// <summary>
    /// Reason manager approval is required
    /// </summary>
    public string? ManagerApprovalReason { get; set; }

    // ========================================
    // COMPARISON WITH ORIGINAL
    // ========================================

    /// <summary>
    /// Original transaction total amount
    /// </summary>
    public decimal OriginalTotalAmount { get; set; }

    /// <summary>
    /// Amount already refunded (from previous partial refunds)
    /// </summary>
    public decimal AlreadyRefundedAmount { get; set; }

    /// <summary>
    /// Percentage of original transaction being refunded
    /// </summary>
    public decimal RefundPercentage { get; set; }
}

/// <summary>
/// Refund preview for individual item
/// </summary>
public class RefundPreviewItemDto
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
    /// Quantity being refunded
    /// </summary>
    public int QuantityToRefund { get; set; }

    /// <summary>
    /// Unit price (including VAT)
    /// </summary>
    public decimal UnitPriceInclVAT { get; set; }

    /// <summary>
    /// Line subtotal for refund (excluding VAT)
    /// </summary>
    public decimal RefundSubtotal { get; set; }

    /// <summary>
    /// VAT amount for refund
    /// </summary>
    public decimal RefundVATAmount { get; set; }

    /// <summary>
    /// Total refund for this line (including VAT)
    /// </summary>
    public decimal RefundTotal { get; set; }

    /// <summary>
    /// Batch number (Cannabis traceability)
    /// </summary>
    public string? BatchNumber { get; set; }
}
