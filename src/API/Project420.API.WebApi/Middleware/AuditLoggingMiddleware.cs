namespace Project420.API.WebApi.Middleware;

/// <summary>
/// Audit logging middleware for POPIA compliance
/// Logs all API requests with user details and timestamps
/// </summary>
public class AuditLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditLoggingMiddleware> _logger;

    public AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var requestPath = context.Request.Path;
        var requestMethod = context.Request.Method;
        var userId = context.User?.Identity?.Name ?? "Anonymous";
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        try
        {
            await _next(context);

            var duration = DateTime.UtcNow - startTime;
            var statusCode = context.Response.StatusCode;

            // Log successful request
            _logger.LogInformation(
                "AUDIT: {Method} {Path} - User: {User} - IP: {IP} - Status: {Status} - Duration: {Duration}ms",
                requestMethod, requestPath, userId, ipAddress, statusCode, duration.TotalMilliseconds);

            // TODO: Store audit log in database for POPIA compliance
            // - 7-year retention requirement
            // - Must track: WHO, WHAT, WHEN, WHERE
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            // Log failed request
            _logger.LogError(ex,
                "AUDIT: {Method} {Path} - User: {User} - IP: {IP} - Error: {Error} - Duration: {Duration}ms",
                requestMethod, requestPath, userId, ipAddress, ex.Message, duration.TotalMilliseconds);

            throw; // Re-throw to be handled by ExceptionHandlingMiddleware
        }
    }
}
