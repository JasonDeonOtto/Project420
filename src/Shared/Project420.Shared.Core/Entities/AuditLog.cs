using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Represents a centralized audit log entry for tracking important business operations across all modules.
/// </summary>
/// <remarks>
/// Purpose:
/// - POPIA compliance: Track who accessed/modified sensitive data
/// - Security: Detect unauthorized access attempts
/// - Compliance: Meet regulatory requirements (cannabis tracking, financial auditing)
/// - Business intelligence: Understand user behavior and system usage
///
/// What to Audit (Examples):
/// - User login/logout events
/// - Price changes (SARS tax compliance)
/// - Customer data access (POPIA compliance)
/// - Product batch updates (Cannabis Act compliance)
/// - Financial transactions
/// - Permission/role changes
/// - Failed login attempts (security)
///
/// Note: This is supplementary to AuditableEntity (which tracks entity-level changes).
/// Use AuditLog for important operations and security events.
///
/// POPIA Compliance:
/// - Audit logs must be immutable (no updates/deletes)
/// - 7-year retention required for financial/PII access logs
/// - Audit who accessed customer records (not just who modified them)
/// </remarks>
public class AuditLog
{
    /// <summary>
    /// Unique identifier for this audit log entry
    /// </summary>
    [Key]
    public long AuditLogId { get; set; }

    /// <summary>
    /// When the audited action occurred (UTC)
    /// </summary>
    [Required]
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type/category of audited action
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "Login", "Logout", "Failed Login"
    /// - "CustomerCreated", "CustomerUpdated", "CustomerViewed"
    /// - "PriceChanged", "ProductDeleted"
    /// - "TransactionCancelled", "RefundIssued"
    /// - "PermissionGranted", "UserDeactivated"
    /// </remarks>
    [Required]
    [MaxLength(100)]
    public string ActionType { get; set; } = string.Empty;

    /// <summary>
    /// Which module performed the action
    /// </summary>
    /// <remarks>
    /// Examples: "Management", "Retail.POS", "Security", "System"
    /// </remarks>
    [MaxLength(100)]
    public string? Module { get; set; }

    /// <summary>
    /// User ID who performed the action
    /// </summary>
    /// <remarks>
    /// Null = System/automated action
    /// Store user ID, not username (for POPIA compliance - avoid storing unnecessary PII)
    /// </remarks>
    public int? UserId { get; set; }

    /// <summary>
    /// Username or identifier at time of action (for audit trail clarity)
    /// </summary>
    /// <remarks>
    /// This is for audit readability (e.g., "admin", "cashier01", "SYSTEM")
    /// If user gets renamed, this preserves historical record
    /// </remarks>
    [MaxLength(100)]
    public string? Username { get; set; }

    /// <summary>
    /// Entity type affected by this action
    /// </summary>
    /// <remarks>
    /// Examples: "Debtor", "Product", "TransactionHeader", "Payment", "User"
    /// </remarks>
    [MaxLength(100)]
    public string? EntityType { get; set; }

    /// <summary>
    /// ID of the specific entity affected
    /// </summary>
    /// <remarks>
    /// Example: If viewing Debtor with ID 12345, store "12345"
    /// Stored as string to accommodate different ID types (int, guid, etc.)
    /// </remarks>
    [MaxLength(100)]
    public string? EntityId { get; set; }

    /// <summary>
    /// Human-readable description of what happened
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "User john.doe logged in from 192.168.1.100"
    /// - "Product 'Blue Dream 3.5g' price changed from R250 to R300"
    /// - "Customer DBT-00123 'John Smith' accessed by cashier02"
    /// - "Transaction TXN-2024-001 cancelled by manager"
    /// - "Failed login attempt for user 'admin' from suspicious IP"
    /// </remarks>
    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Old value before change (if applicable)
    /// </summary>
    /// <remarks>
    /// For update operations, store the previous value
    /// Example: Price change - OldValue: "R250.00"
    /// For POPIA: Be careful not to store full PII unnecessarily
    /// </remarks>
    [MaxLength(2000)]
    public string? OldValue { get; set; }

    /// <summary>
    /// New value after change (if applicable)
    /// </summary>
    /// <remarks>
    /// For update operations, store the new value
    /// Example: Price change - NewValue: "R300.00"
    /// </remarks>
    [MaxLength(2000)]
    public string? NewValue { get; set; }

    /// <summary>
    /// IP address of user who performed action
    /// </summary>
    /// <remarks>
    /// Security: Track where actions originated
    /// POPIA Note: IP addresses may be considered PII
    /// </remarks>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// Success or failure indicator
    /// </summary>
    /// <remarks>
    /// True = Action succeeded
    /// False = Action failed (e.g., failed login, validation error)
    /// Tracking failures helps detect security issues
    /// </remarks>
    public bool Success { get; set; } = true;

    /// <summary>
    /// Error message if action failed
    /// </summary>
    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Severity level for this audit event
    /// </summary>
    /// <remarks>
    /// - Critical: Security breach attempt, data loss, unauthorized admin action
    /// - High: Important business operation (large transaction, customer deletion)
    /// - Medium: Normal business operations (sale, customer update)
    /// - Low: Routine operations (login, product view)
    /// </remarks>
    [MaxLength(20)]
    public string Severity { get; set; } = "Medium";

    /// <summary>
    /// Additional context data in JSON format
    /// </summary>
    /// <remarks>
    /// Store extra debugging/context information
    /// Example: { "ProductId": 123, "OldPrice": 250.00, "NewPrice": 300.00, "Reason": "Market adjustment" }
    ///
    /// POPIA Warning: Do NOT store full customer records or sensitive PII
    /// </remarks>
    public string? AdditionalData { get; set; }
}
