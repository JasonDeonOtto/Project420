using System.Collections.Concurrent;
using System.Net;

namespace Project420.API.WebApi.Middleware;

/// <summary>
/// Rate limiting middleware to prevent abuse
/// Limits requests per IP address
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;

    // Simple in-memory rate limiting (production should use Redis/distributed cache)
    private static readonly ConcurrentDictionary<string, (DateTime WindowStart, int RequestCount)> _requestCounts = new();

    // Configuration
    private const int MaxRequestsPerWindow = 100;
    private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(1);

    public RateLimitingMiddleware(RequestDelegate next, ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        // Check rate limit
        var (isAllowed, currentCount) = CheckRateLimit(ipAddress);

        if (!isAllowed)
        {
            _logger.LogWarning(
                "Rate limit exceeded for IP: {IP} - Requests: {Count}/{Max}",
                ipAddress, currentCount, MaxRequestsPerWindow);

            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync("{\"error\":\"Rate limit exceeded. Please try again later.\"}");
            return;
        }

        await _next(context);
    }

    private (bool IsAllowed, int CurrentCount) CheckRateLimit(string ipAddress)
    {
        var now = DateTime.UtcNow;

        var entry = _requestCounts.AddOrUpdate(
            ipAddress,
            (now, 1),
            (key, existing) =>
            {
                // Reset window if expired
                if (now - existing.WindowStart > WindowDuration)
                {
                    return (now, 1);
                }
                // Increment count
                return (existing.WindowStart, existing.RequestCount + 1);
            });

        var isAllowed = entry.RequestCount <= MaxRequestsPerWindow;
        return (isAllowed, entry.RequestCount);
    }

    // Cleanup old entries periodically (should be called from a background service)
    public static void CleanupOldEntries()
    {
        var now = DateTime.UtcNow;
        var keysToRemove = _requestCounts
            .Where(kvp => now - kvp.Value.WindowStart > WindowDuration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in keysToRemove)
        {
            _requestCounts.TryRemove(key, out _);
        }
    }
}
