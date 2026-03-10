## 2024-05-23 - Fix Environment-Specific Security Bypass in Middleware
**Vulnerability:** API endpoints intended to be bypassed only in local development were accessible globally due to incomplete path conditions.
**Learning:** `if (context.Request.Method == "GET" && path.StartsWith("/api/prices"))` does not actually check if the environment is development, exposing the endpoint to anonymous requests in all environments.
**Prevention:** Always explicitly use `IWebHostEnvironment.IsDevelopment()` to verify the execution environment before applying security bypasses.
