namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// DTO for refund eligibility validation results
/// </summary>
/// <remarks>
/// Used to determine if a transaction can be refunded
/// and what approval/warnings are needed
/// </remarks>
public class RefundEligibilityDto
{
    // ========================================
    // ELIGIBILITY STATUS
    // ========================================

    /// <summary>
    /// Indicates if transaction is eligible for refund
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Reasons why transaction is not eligible (if IsEligible = false)
    /// </summary>
    public List<string> IneligibilityReasons { get; set; } = new();

    /// <summary>
    /// Warnings that don't prevent refund but require special handling
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "Outside 30-day window - manager approval required"
    /// - "Partial refund already processed for R500"
    /// - "Original payment was cash - cash refund required"
    /// </remarks>
    public List<string> Warnings { get; set; } = new();

    // ========================================
    // APPROVAL REQUIREMENTS
    // ========================================

    /// <summary>
    /// Indicates if manager approval is required
    /// </summary>
    public bool RequiresManagerApproval { get; set; }

    /// <summary>
    /// Reason manager approval is required
    /// </summary>
    public string? ManagerApprovalReason { get; set; }

    // ========================================
    // ORIGINAL TRANSACTION INFO
    // ========================================

    /// <summary>
    /// Original transaction number
    /// </summary>
    public string? OriginalTransactionNumber { get; set; }

    /// <summary>
    /// Original transaction date
    /// </summary>
    public DateTime OriginalTransactionDate { get; set; }

    /// <summary>
    /// Original transaction total amount
    /// </summary>
    public decimal OriginalTotalAmount { get; set; }

    /// <summary>
    /// Amount already refunded (from previous partial refunds)
    /// </summary>
    public decimal AlreadyRefundedAmount { get; set; }

    /// <summary>
    /// Maximum amount still available for refund
    /// </summary>
    public decimal RemainingRefundableAmount { get; set; }

    /// <summary>
    /// Days since original transaction
    /// </summary>
    public int DaysSinceTransaction { get; set; }

    /// <summary>
    /// Original payment method
    /// </summary>
    public string? OriginalPaymentMethod { get; set; }

    // ========================================
    // REFUNDABLE ITEMS
    // ========================================

    /// <summary>
    /// Items from original transaction that are still refundable
    /// </summary>
    /// <remarks>
    /// Quantities adjusted for any previous partial refunds
    /// </remarks>
    public List<RefundableItemDto> RefundableItems { get; set; } = new();
}

/// <summary>
/// Represents an item that is available for refund
/// </summary>
public class RefundableItemDto
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
    /// Original quantity purchased
    /// </summary>
    public int OriginalQuantity { get; set; }

    /// <summary>
    /// Quantity already refunded (from previous refunds)
    /// </summary>
    public int AlreadyRefundedQuantity { get; set; }

    /// <summary>
    /// Remaining quantity available for refund
    /// </summary>
    public int RemainingRefundableQuantity { get; set; }

    /// <summary>
    /// Unit price (including VAT) from original transaction
    /// </summary>
    public decimal UnitPriceInclVAT { get; set; }

    /// <summary>
    /// Batch number (Cannabis traceability)
    /// </summary>
    public string? BatchNumber { get; set; }

    /// <summary>
    /// Cost price (for inventory adjustment)
    /// </summary>
    public decimal CostPrice { get; set; }
}
