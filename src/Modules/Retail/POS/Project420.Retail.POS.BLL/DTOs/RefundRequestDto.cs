using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Request DTO for processing a refund
/// </summary>
/// <remarks>
/// Cannabis Act Compliance:
/// - Original transaction must be traceable
/// - Refund reason required for audit trail
/// - Manager approval required for large refunds (> R1000)
///
/// Business Rules:
/// - 30-day refund window (can be overridden with manager approval)
/// - Cannot exceed original transaction amount
/// - Partial refunds supported
/// </remarks>
public class RefundRequestDto
{
    // ========================================
    // ORIGINAL TRANSACTION
    // ========================================

    /// <summary>
    /// Original transaction number being refunded
    /// </summary>
    /// <remarks>
    /// Format: "SALE-YYYYMMDD-XXX"
    /// Used to lookup the original sale
    /// </remarks>
    public string OriginalTransactionNumber { get; set; } = string.Empty;

    // ========================================
    // REFUND ITEMS
    // ========================================

    /// <summary>
    /// Items being refunded (subset or all of original items)
    /// </summary>
    /// <remarks>
    /// Can be partial refund (some items) or full refund (all items)
    /// Each item must have been in original transaction
    /// Quantities cannot exceed original quantities
    /// </remarks>
    public List<RefundItemDto> RefundItems { get; set; } = new();

    // ========================================
    // REFUND REASON & APPROVAL
    // ========================================

    /// <summary>
    /// Reason for refund (compliance requirement)
    /// </summary>
    /// <remarks>
    /// Cannabis Act: All refunds must be justified
    /// SARS: Refunds affect VAT calculations, must be documented
    /// </remarks>
    public RefundReason RefundReason { get; set; }

    /// <summary>
    /// Additional notes/explanation for refund
    /// </summary>
    /// <remarks>
    /// Required if:
    /// - Refund amount > R1000
    /// - Refund is outside 30-day window
    /// - Refund reason is "Other" or "ManagerOverride"
    /// </remarks>
    public string? Notes { get; set; }

    /// <summary>
    /// Manager user ID who approved this refund (if required)
    /// </summary>
    /// <remarks>
    /// Required when:
    /// - Refund amount > R1000
    /// - Refund is outside 30-day window
    /// - Refund reason is "ComplianceIssue" or "ManagerOverride"
    /// </remarks>
    public int? ManagerApprovalUserId { get; set; }

    // ========================================
    // PAYMENT INFORMATION
    // ========================================

    /// <summary>
    /// How to refund the customer
    /// </summary>
    /// <remarks>
    /// Usually same method as original payment
    /// Cash → Refund cash
    /// Card → Refund to card (may require card terminal)
    /// OnAccount → Credit customer account
    /// </remarks>
    public PaymentMethod RefundPaymentMethod { get; set; }

    /// <summary>
    /// Customer name (for receipt)
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// User ID processing this refund
    /// </summary>
    public int ProcessedByUserId { get; set; }
}

/// <summary>
/// Individual item being refunded
/// </summary>
public class RefundItemDto
{
    /// <summary>
    /// Product ID from original transaction
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
    /// <remarks>
    /// Cannot exceed quantity in original transaction
    /// </remarks>
    public int QuantityToRefund { get; set; }

    /// <summary>
    /// Original quantity purchased
    /// </summary>
    public int OriginalQuantity { get; set; }

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

    // ========================================
    // CALCULATED PROPERTIES
    // ========================================

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
}
