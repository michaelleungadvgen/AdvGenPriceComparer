using AdvGenPriceComparer.Server.Services;

namespace AdvGenPriceComparer.Server.Middleware;

/// <summary>
/// Middleware for rate limiting
/// </summary>
public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitMiddleware> _logger;

    public RateLimitMiddleware(RequestDelegate next, ILogger<RateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IRateLimitService rateLimitService)
    {
        // Skip rate limiting for Swagger and health endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.StartsWith("/swagger") || path.StartsWith("/health") || path == "/")
        {
            await _next(context);
            return;
        }

        // Get rate limit from API key or use default
        var rateLimit = context.Items.TryGetValue("ApiKeyRateLimit", out var limitObj) && limitObj is int limit
            ? limit
            : 100; // Default rate limit

        // Use API key ID or IP address as identifier
        var key = context.Items.TryGetValue("ApiKeyId", out var keyId) && keyId is int id
            ? $"apikey_{id}"
            : $"ip_{context.Connection.RemoteIpAddress}";

        if (!rateLimitService.IsAllowed(key, rateLimit, 60))
        {
            var retryAfter = rateLimitService.GetRetryAfter(key, rateLimit, 60);
            var remaining = rateLimitService.GetRemainingRequests(key, rateLimit, 60);

            _logger.LogWarning("Rate limit exceeded for {Key}", key);

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers["X-RateLimit-Limit"] = rateLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remaining.ToString();

            if (retryAfter.HasValue)
            {
                context.Response.Headers["Retry-After"] = ((int)retryAfter.Value.TotalSeconds).ToString();
            }

            await context.Response.WriteAsJsonAsync(new
            {
                error = "Rate limit exceeded",
                retryAfter = retryAfter?.TotalSeconds ?? 60
            });
            return;
        }

        // Add rate limit headers
        var remainingRequests = rateLimitService.GetRemainingRequests(key, rateLimit, 60);
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit"] = rateLimit.ToString();
            context.Response.Headers["X-RateLimit-Remaining"] = remainingRequests.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }
}
