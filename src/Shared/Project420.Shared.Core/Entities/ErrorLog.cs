using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Shared.Core.Entities;

/// <summary>
/// Represents an application error log entry for centralized error tracking and diagnostics.
/// </summary>
/// <remarks>
/// Purpose:
/// - Centralized error logging across all modules (Management, Retail POS, etc.)
/// - Production error tracking and debugging
/// - Compliance and audit trail for system failures
/// - Performance issue identification
///
/// This entity is ready for implementation but not yet integrated codebase-wide.
/// Future implementation will use dependency injection for IErrorLogger service.
///
/// POPIA Compliance:
/// - Be careful not to log sensitive PII (customer names, ID numbers, etc.)
/// - Log only what's necessary for debugging (user IDs, not personal details)
/// - Consider data retention policies for error logs
/// </remarks>
public class ErrorLog
{
    /// <summary>
    /// Unique identifier for this error log entry
    /// </summary>
    [Key]
    public int ErrorLogId { get; set; }

    /// <summary>
    /// When the error occurred (UTC)
    /// </summary>
    [Required]
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Error severity level
    /// </summary>
    /// <remarks>
    /// - Critical: System failure, requires immediate attention
    /// - Error: Operation failed but system continues
    /// - Warning: Potential issue, operation succeeded with concerns
    /// - Info: Informational message (not really an error)
    /// </remarks>
    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = "Error";

    /// <summary>
    /// Which module/area of the application the error occurred in
    /// </summary>
    /// <remarks>
    /// Examples: "Management", "Retail.POS", "Shared.Infrastructure", "Database"
    /// </remarks>
    [MaxLength(100)]
    public string? Source { get; set; }

    /// <summary>
    /// Exception type or error code
    /// </summary>
    /// <remarks>
    /// Examples: "NullReferenceException", "SqlException", "ValidationException", "ERR_001"
    /// </remarks>
    [MaxLength(200)]
    public string? ErrorType { get; set; }

    /// <summary>
    /// Short error message (what went wrong)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Full stack trace (for debugging)
    /// </summary>
    /// <remarks>
    /// WARNING: Stack traces can be very long. Consider truncating if necessary.
    /// Contains technical details for developers to diagnose the issue.
    /// </remarks>
    public string? StackTrace { get; set; }

    /// <summary>
    /// Inner exception message (if applicable)
    /// </summary>
    [MaxLength(1000)]
    public string? InnerException { get; set; }

    /// <summary>
    /// User ID who was logged in when error occurred (if applicable)
    /// </summary>
    /// <remarks>
    /// POPIA Note: Store user ID (integer), NOT username or personal details
    /// This helps identify user-specific issues without storing PII in error logs
    /// </remarks>
    public int? UserId { get; set; }

    /// <summary>
    /// Request URL or operation being performed when error occurred
    /// </summary>
    /// <remarks>
    /// Examples: "/api/products/create", "ProcessSale", "/debtor/12345/edit"
    /// Helps identify which operation failed
    ///
    /// POPIA Warning: URLs may contain sensitive data - sanitize before logging!
    /// Example: Log "/api/debtor/edit" NOT "/api/debtor/edit?idnumber=1234567890123"
    /// </remarks>
    [MaxLength(500)]
    public string? RequestPath { get; set; }

    /// <summary>
    /// HTTP method (if web request)
    /// </summary>
    /// <remarks>
    /// Examples: "GET", "POST", "PUT", "DELETE"
    /// </remarks>
    [MaxLength(10)]
    public string? HttpMethod { get; set; }

    /// <summary>
    /// User's IP address (if applicable)
    /// </summary>
    /// <remarks>
    /// POPIA Note: IP addresses are considered PII under some interpretations
    /// Consider anonymizing or not storing if not needed for security
    /// </remarks>
    [MaxLength(50)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string (browser/device info)
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// Additional context data (JSON format)
    /// </summary>
    /// <remarks>
    /// Store additional debugging information as JSON
    /// Example: { "ProductId": 123, "Quantity": 5, "PricelistId": 2 }
    ///
    /// CRITICAL: Do NOT store PII in this field!
    /// - OK: Product IDs, quantities, prices, timestamps
    /// - NOT OK: Names, ID numbers, addresses, phone numbers
    /// </remarks>
    public string? AdditionalData { get; set; }

    /// <summary>
    /// Whether this error has been resolved/acknowledged
    /// </summary>
    /// <remarks>
    /// Use for error tracking workflow:
    /// - False (default): New error, needs investigation
    /// - True: Error acknowledged by developer/admin
    /// </remarks>
    public bool IsResolved { get; set; } = false;

    /// <summary>
    /// When the error was marked as resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Who resolved the error
    /// </summary>
    [MaxLength(100)]
    public string? ResolvedBy { get; set; }

    /// <summary>
    /// Notes about the resolution
    /// </summary>
    [MaxLength(1000)]
    public string? ResolutionNotes { get; set; }
}
