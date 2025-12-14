using Project420.Shared.Core.Enums;

namespace Project420.Retail.POS.BLL.DTOs;

/// <summary>
/// Request DTO for transaction cancellation
/// </summary>
/// <remarks>
/// Phase 9.6: Transaction Cancellation Workflow
/// - Before payment: Simply clear cart (no database changes)
/// - After payment: Void transaction with movement reversal
/// </remarks>
public class CancellationRequestDto
{
    /// <summary>
    /// Transaction number to cancel (for completed transactions)
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Reason for cancellation
    /// </summary>
    public CancellationReason Reason { get; set; }

    /// <summary>
    /// Additional notes/explanation for audit trail
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// User ID performing the cancellation
    /// </summary>
    public int ProcessedByUserId { get; set; }

    /// <summary>
    /// Manager User ID (required for completed transactions)
    /// </summary>
    public int? ManagerApprovalUserId { get; set; }

    /// <summary>
    /// Manager PIN for verification
    /// </summary>
    public string? ManagerPin { get; set; }

    /// <summary>
    /// Whether this is a pre-payment cancellation (cart clear)
    /// </summary>
    public bool IsPrePaymentCancellation { get; set; }
}

/// <summary>
/// DTO for transaction details when verifying cancellation eligibility
/// </summary>
public class CancellationEligibilityDto
{
    /// <summary>
    /// Transaction number being cancelled
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Current status of the transaction
    /// </summary>
    public TransactionStatus Status { get; set; }

    /// <summary>
    /// Transaction date
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Minutes since transaction completed
    /// </summary>
    public int MinutesSinceTransaction { get; set; }

    /// <summary>
    /// Total amount of the transaction
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Payment method used
    /// </summary>
    public string PaymentMethod { get; set; } = string.Empty;

    /// <summary>
    /// Number of items in the transaction
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Whether the transaction can be cancelled
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Whether manager approval is required
    /// </summary>
    public bool RequiresManagerApproval { get; set; }

    /// <summary>
    /// Reason why manager approval is required
    /// </summary>
    public string? ManagerApprovalReason { get; set; }

    /// <summary>
    /// Reasons why transaction cannot be cancelled
    /// </summary>
    public List<string> IneligibilityReasons { get; set; } = new();

    /// <summary>
    /// Warnings about the cancellation
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Items in the transaction
    /// </summary>
    public List<CancellationItemDto> Items { get; set; } = new();
}

/// <summary>
/// Item details for cancellation display
/// </summary>
public class CancellationItemDto
{
    public int ProductId { get; set; }
    public string ProductSku { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
}

/// <summary>
/// Result of cancellation operation
/// </summary>
public class CancellationResultDto
{
    /// <summary>
    /// Whether the cancellation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Transaction number that was cancelled
    /// </summary>
    public string TransactionNumber { get; set; } = string.Empty;

    /// <summary>
    /// Cancellation timestamp
    /// </summary>
    public DateTime CancellationDate { get; set; }

    /// <summary>
    /// Number of movements reversed
    /// </summary>
    public int MovementsReversed { get; set; }

    /// <summary>
    /// Reason for cancellation (for audit)
    /// </summary>
    public CancellationReason Reason { get; set; }

    /// <summary>
    /// User who performed the cancellation
    /// </summary>
    public int ProcessedByUserId { get; set; }

    /// <summary>
    /// Manager who approved (if applicable)
    /// </summary>
    public int? ManagerApprovalUserId { get; set; }

    /// <summary>
    /// Error messages if cancellation failed
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// Whether payment reversal is needed
    /// </summary>
    public bool RequiresPaymentReversal { get; set; }

    /// <summary>
    /// Amount to refund to customer
    /// </summary>
    public decimal? RefundAmount { get; set; }

    /// <summary>
    /// Payment method for reversal
    /// </summary>
    public string? OriginalPaymentMethod { get; set; }
}

/// <summary>
/// DTO for manager PIN validation
/// </summary>
public class ManagerValidationDto
{
    /// <summary>
    /// Whether the manager credentials are valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Manager's name (for display)
    /// </summary>
    public string? ManagerName { get; set; }

    /// <summary>
    /// Error message if validation failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
