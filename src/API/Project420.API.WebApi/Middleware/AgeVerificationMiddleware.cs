using System.Net;

namespace Project420.API.WebApi.Middleware;

/// <summary>
/// Age verification middleware for Cannabis Act compliance
/// Ensures customers are age-verified before accessing restricted endpoints
/// </summary>
public class AgeVerificationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AgeVerificationMiddleware> _logger;

    // Endpoints that require age verification
    private readonly string[] _restrictedEndpoints = new[]
    {
        "/api/orders",
        "/api/products"
    };

    public AgeVerificationMiddleware(RequestDelegate next, ILogger<AgeVerificationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Check if this endpoint requires age verification
        var requiresAgeVerification = _restrictedEndpoints.Any(endpoint =>
            path.StartsWith(endpoint.ToLower()));

        if (requiresAgeVerification && context.User.Identity?.IsAuthenticated == true)
        {
            // TODO: Check if user has age verification claim in JWT token
            // For now, we'll allow through - implement proper check later
            var ageVerifiedClaim = context.User.Claims
                .FirstOrDefault(c => c.Type == "ageVerified")?.Value;

            if (ageVerifiedClaim != "true")
            {
                _logger.LogWarning(
                    "Age verification required for {Path} - User: {User}",
                    path, context.User.Identity.Name);

                // TODO: Return proper error response
                // For now, we'll allow through for scaffolding purposes
            }
        }

        await _next(context);
    }
}
