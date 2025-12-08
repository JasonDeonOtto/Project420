using Project420.API.WebApi.Middleware;

namespace Project420.API.WebApi.Extensions;

/// <summary>
/// Extension methods for registering middleware
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Add all compliance middleware to the pipeline
    /// </summary>
    public static IApplicationBuilder UseComplianceMiddleware(this IApplicationBuilder app)
    {
        // Order matters! Exception handling should be first
        app.UseMiddleware<ExceptionHandlingMiddleware>();
        app.UseMiddleware<RateLimitingMiddleware>();
        app.UseMiddleware<AuditLoggingMiddleware>();
        app.UseMiddleware<AgeVerificationMiddleware>();

        return app;
    }
}
