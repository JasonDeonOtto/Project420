namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// Result of validating a refund request
/// </summary>
/// <remarks>
/// Used by RefundService to validate refunds before processing.
/// Ensures business rules are enforced (time limits, manager approval, etc.)
/// </remarks>
public class RefundValidationResult
{
    /// <summary>
    /// Indicates if the refund is valid and can proceed
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation error messages (if invalid)
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "Original transaction not found"
    /// - "Refund window expired (must be within 30 days)"
    /// - "Product quantity exceeds original sale quantity"
    /// - "Transaction already refunded"
    /// </remarks>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Indicates if manager approval is required for this refund
    /// </summary>
    /// <remarks>
    /// Manager approval required when:
    /// - Refund is outside normal return window
    /// - Refund amount exceeds threshold (e.g., > R1000)
    /// - Refund reason is "ComplianceIssue"
    /// - Refund reason is "ManagerOverride"
    /// </remarks>
    public bool RequiresManagerApproval { get; set; }

    /// <summary>
    /// Maximum refund amount allowed based on original transaction
    /// </summary>
    /// <remarks>
    /// Calculated from original transaction:
    /// - Cannot exceed original transaction amount
    /// - Reduced if partial refund already processed
    /// - Includes VAT calculation
    /// </remarks>
    public decimal MaxRefundAmount { get; set; }

    /// <summary>
    /// Number of days since original transaction
    /// </summary>
    /// <remarks>
    /// Used to determine if refund is within acceptable timeframe.
    /// Standard retail: 30 days
    /// Defective products: May extend to 90 days
    /// </remarks>
    public int DaysSinceOriginalTransaction { get; set; }

    /// <summary>
    /// Original transaction number being refunded
    /// </summary>
    public string? OriginalTransactionNumber { get; set; }

    /// <summary>
    /// Amount already refunded from this transaction (if partial refunds exist)
    /// </summary>
    public decimal PreviousRefundAmount { get; set; }

    /// <summary>
    /// Helper method to add validation error
    /// </summary>
    public void AddError(string errorMessage)
    {
        IsValid = false;
        ValidationErrors.Add(errorMessage);
    }
}
