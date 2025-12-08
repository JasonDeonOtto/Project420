using Project420.Shared.Infrastructure.DTOs;

namespace Project420.Shared.Infrastructure.Interfaces;

/// <summary>
/// Service for creating and managing audit log entries.
/// Tracks all important business operations for POPIA compliance, security, and regulatory requirements.
/// </summary>
/// <remarks>
/// Purpose:
/// - POPIA Compliance: Track who accessed/modified sensitive data (7-year retention)
/// - Cannabis Act Compliance: Track batch/product changes (seed-to-sale traceability)
/// - SARS Compliance: Track financial transaction changes (tax audits)
/// - Security: Detect unauthorized access attempts and suspicious activity
///
/// What to Audit:
/// - Customer data access/modification (POPIA)
/// - Product/price changes (SARS tax compliance)
/// - Cannabis batch updates (Cannabis Act)
/// - Financial transactions (SARS, FIC Act)
/// - Security events (login, failed login, permission changes)
/// - Stock adjustments (internal audit)
///
/// Immutability:
/// - Audit logs MUST NOT be updated or deleted (compliance requirement)
/// - Only INSERT operations allowed
/// - Use soft delete pattern if absolutely necessary
/// </remarks>
public interface IAuditLogService
{
    #region Create Audit Logs

    /// <summary>
    /// Creates a new audit log entry.
    /// </summary>
    /// <param name="dto">The audit log data to record.</param>
    /// <returns>The ID of the created audit log entry.</returns>
    /// <example>
    /// <code>
    /// await _auditLogService.LogAsync(new AuditLogDto
    /// {
    ///     ActionType = "CustomerCreated",
    ///     Module = "Management",
    ///     UserId = currentUserId,
    ///     Username = currentUsername,
    ///     EntityType = "Debtor",
    ///     EntityId = customer.Id.ToString(),
    ///     Description = $"Customer '{customer.Name}' created",
    ///     Severity = "Medium"
    /// });
    /// </code>
    /// </example>
    Task<long> LogAsync(AuditLogDto dto);

    /// <summary>
    /// Creates an audit log entry for a data modification (update operation).
    /// Automatically captures before/after values.
    /// </summary>
    /// <param name="entityType">The type of entity modified (e.g., "Product").</param>
    /// <param name="entityId">The ID of the entity modified.</param>
    /// <param name="action">The action performed (e.g., "PriceChanged").</param>
    /// <param name="oldValue">The value before modification.</param>
    /// <param name="newValue">The value after modification.</param>
    /// <param name="userId">The user who made the change (null for system).</param>
    /// <param name="username">The username for audit readability.</param>
    /// <param name="reason">Optional reason for the change.</param>
    /// <returns>The ID of the created audit log entry.</returns>
    /// <example>
    /// <code>
    /// await _auditLogService.LogDataChangeAsync(
    ///     entityType: "Product",
    ///     entityId: product.Id.ToString(),
    ///     action: "PriceChanged",
    ///     oldValue: "R250.00",
    ///     newValue: "R300.00",
    ///     userId: currentUserId,
    ///     username: currentUsername,
    ///     reason: "Market price adjustment"
    /// );
    /// </code>
    /// </example>
    Task<long> LogDataChangeAsync(
        string entityType,
        string entityId,
        string action,
        string oldValue,
        string newValue,
        int? userId = null,
        string? username = null,
        string? reason = null
    );

    /// <summary>
    /// Creates an audit log entry for a security event (login, failed login, etc.).
    /// </summary>
    /// <param name="action">The security action (e.g., "Login", "FailedLogin").</param>
    /// <param name="userId">The user ID (null if login failed/unknown).</param>
    /// <param name="username">The username attempted.</param>
    /// <param name="ipAddress">The IP address of the request.</param>
    /// <param name="success">Whether the action succeeded.</param>
    /// <param name="errorMessage">Error message if failed.</param>
    /// <returns>The ID of the created audit log entry.</returns>
    Task<long> LogSecurityEventAsync(
        string action,
        int? userId,
        string username,
        string? ipAddress,
        bool success,
        string? errorMessage = null
    );

    /// <summary>
    /// Creates an audit log entry for a business transaction (sale, refund, etc.).
    /// </summary>
    /// <param name="transactionType">The type of transaction (e.g., "Sale", "Refund").</param>
    /// <param name="transactionId">The transaction ID.</param>
    /// <param name="amount">The transaction amount.</param>
    /// <param name="userId">The user who processed the transaction.</param>
    /// <param name="username">The username for audit readability.</param>
    /// <param name="description">Additional description.</param>
    /// <returns>The ID of the created audit log entry.</returns>
    Task<long> LogTransactionAsync(
        string transactionType,
        string transactionId,
        decimal amount,
        int? userId,
        string? username,
        string? description = null
    );

    #endregion

    #region Query Audit Logs

    /// <summary>
    /// Gets all audit logs for a specific entity.
    /// </summary>
    /// <param name="entityType">The entity type (e.g., "Debtor", "Product").</param>
    /// <param name="entityId">The entity ID.</param>
    /// <param name="limit">Maximum number of records to return (default 100).</param>
    /// <returns>List of audit log DTOs ordered by most recent first.</returns>
    Task<List<AuditLogDto>> GetEntityHistoryAsync(string entityType, string entityId, int limit = 100);

    /// <summary>
    /// Gets all audit logs for a specific user.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="from">Start date (optional).</param>
    /// <param name="to">End date (optional).</param>
    /// <param name="limit">Maximum number of records to return (default 100).</param>
    /// <returns>List of audit log DTOs ordered by most recent first.</returns>
    Task<List<AuditLogDto>> GetUserActivityAsync(int userId, DateTime? from = null, DateTime? to = null, int limit = 100);

    /// <summary>
    /// Gets all failed security events (failed logins, unauthorized access, etc.).
    /// </summary>
    /// <param name="from">Start date (optional).</param>
    /// <param name="to">End date (optional).</param>
    /// <param name="limit">Maximum number of records to return (default 100).</param>
    /// <returns>List of audit log DTOs ordered by most recent first.</returns>
    Task<List<AuditLogDto>> GetFailedSecurityEventsAsync(DateTime? from = null, DateTime? to = null, int limit = 100);

    /// <summary>
    /// Gets audit logs by severity level.
    /// </summary>
    /// <param name="severity">The severity level ("Low", "Medium", "High", "Critical").</param>
    /// <param name="from">Start date (optional).</param>
    /// <param name="to">End date (optional).</param>
    /// <param name="limit">Maximum number of records to return (default 100).</param>
    /// <returns>List of audit log DTOs ordered by most recent first.</returns>
    Task<List<AuditLogDto>> GetBySeverityAsync(string severity, DateTime? from = null, DateTime? to = null, int limit = 100);

    #endregion
}
