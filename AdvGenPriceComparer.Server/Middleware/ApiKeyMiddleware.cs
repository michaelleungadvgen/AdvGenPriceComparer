using AdvGenPriceComparer.Server.Services;

namespace AdvGenPriceComparer.Server.Middleware;

/// <summary>
/// Middleware for API key authentication
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ApiKeyMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ApiKeyMiddleware(RequestDelegate next, ILogger<ApiKeyMiddleware> logger, IWebHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Skip API key validation for Swagger and health endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.Contains("/swagger") || path.Contains("/health") || path == "/")
        {
            await _next(context);
            return;
        }

        // 🛡️ SECURITY FIX: Explicitly restrict public read access to development environment ONLY.
        // Prevents unauthenticated data exposure in production environments.
        if (_env.IsDevelopment() && context.Request.Method == "GET" && path.StartsWith("/api/prices"))
        {
            // Public read access ONLY in development
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
