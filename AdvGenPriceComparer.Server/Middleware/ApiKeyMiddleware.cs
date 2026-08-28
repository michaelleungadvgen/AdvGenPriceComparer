using AdvGenPriceComparer.Server.Services;

namespace AdvGenPriceComparer.Server.Middleware;

/// <summary>
/// Middleware for API key authentication
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService, IConfiguration configuration)
    {
        // Allow bypassing API key authentication entirely if configured
        if (!configuration.GetValue<bool>("ApiSettings:RequireApiKey", true))
        {
            await _next(context);
            return;
        }

        // Skip API key validation for Swagger and health endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.Contains("/swagger") || path.Contains("/health") || path == "/")
        {
            await _next(context);
            return;
        }

        // Allow anonymous access for certain endpoints if configured
        if (context.Request.Method == "GET" && path.StartsWith("/api/prices") && configuration.GetValue<bool>("ApiSettings:AllowPublicReadAccess", false))
        {
            // Public read access
            await _next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "API key is required" });
            return;
        }

        var apiKey = apiKeyHeader.ToString();
        var keyInfo = await apiKeyService.ValidateKeyAsync(apiKey);

        if (keyInfo == null)
        {
            _logger.LogWarning("Invalid API key used from IP: {Ip}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // Store key info in context for later use
        context.Items["ApiKeyId"] = keyInfo.Id;
        context.Items["ApiKeyName"] = keyInfo.Name;
        context.Items["ApiKeyRateLimit"] = keyInfo.RateLimit;

        await _next(context);
    }
}
