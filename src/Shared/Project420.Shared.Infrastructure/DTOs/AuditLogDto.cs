namespace Project420.Shared.Infrastructure.DTOs;

/// <summary>
/// DTO for creating audit log entries.
/// Used to pass audit data to the AuditLogService without exposing the entity directly.
/// </summary>
public class AuditLogDto
{
    /// <summary>
    /// Type/category of audited action (required).
    /// </summary>
    /// <example>
    /// "CustomerCreated", "PriceChanged", "Login", "TransactionCancelled"
    /// </example>
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Which module performed the action.
    /// </summary>
    /// <example>
    /// "Management", "Retail.POS", "Security", "System"
    /// </example>
    public string? Module { get; set; }

    /// <summary>
    /// User ID who performed the action (null for system actions).
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// Username for audit readability.
    /// </summary>
    /// <example>
    /// "admin", "cashier01", "SYSTEM"
    /// </example>
    public string? Username { get; set; }

    /// <summary>
    /// Entity type affected by this action.
    /// </summary>
    /// <example>
    /// "Debtor", "Product", "TransactionHeader", "Payment"
    /// </example>
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of the specific entity affected (as string).
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// Human-readable description of what happened.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Old value before change (for update operations).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after change (for update operations).
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// IP address of user who performed action.
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Success or failure indicator.
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if action failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Severity level for this audit event.
    /// </summary>
    /// <example>
    /// "Low", "Medium", "High", "Critical"
    /// </example>
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Additional context data in JSON format.
    /// </summary>
    public string? AdditionalData { get; set; }
}
