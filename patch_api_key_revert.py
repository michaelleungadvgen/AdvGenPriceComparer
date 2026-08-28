import sys

file_path = "AdvGenPriceComparer.Server/Middleware/ApiKeyMiddleware.cs"
with open(file_path, "r") as f:
    content = f.read()

search = """    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService, IWebHostEnvironment env)
    {
        // Skip API key validation for Swagger and health endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.StartsWith("/swagger/") || path == "/swagger" || path.StartsWith("/health/") || path == "/health" || path == "/")
        {
            await _next(context);
            return;
        }

        // Allow anonymous access in development for certain endpoints
        if (env.IsDevelopment() && context.Request.Method == "GET" && (path.StartsWith("/api/prices/") || path == "/api/prices"))
        {
            // Public read access
            await _next(context);
            return;
        }"""

replace = """    public async Task InvokeAsync(HttpContext context, IApiKeyService apiKeyService)
    {
        // Skip API key validation for Swagger and health endpoints
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
        if (path.StartsWith("/swagger/") || path == "/swagger" || path.StartsWith("/health/") || path == "/health" || path == "/")
        {
            await _next(context);
            return;
        }

        // Allow anonymous access to public endpoints
        if (context.Request.Method == "GET" && (path.StartsWith("/api/prices/") || path == "/api/prices"))
        {
            // Public read access
            await _next(context);
            return;
        }"""

if search in content:
    with open(file_path, "w") as f:
        f.write(content.replace(search, replace))
    print("Reverted IsDevelopment check in ApiKeyMiddleware.cs")
else:
    print("Could not find search block in ApiKeyMiddleware.cs")
