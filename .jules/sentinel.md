## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-18 - Middleware Path Matching and Environment Checking Bypass

**Vulnerability:** Path matching vulnerabilities in custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) allowed attackers to bypass authentication and rate limits. Specifically, `path.Contains("/swagger")` or `path.Contains("/health")` allowed bypasses by simply appending these strings to any URL (e.g., `/api/admin/swagger`). Additionally, an endpoint intended only for development (`/api/prices`) lacked the required `IWebHostEnvironment.IsDevelopment()` check, exposing it in production.
**Learning:** In ASP.NET Core middleware, loose substring matching (`.Contains()`) on request paths is dangerous and leads to authorization bypasses. Furthermore, development-only bypass rules must explicitly verify the hosting environment rather than relying on comments or implicit assumptions. Dependencies like `IWebHostEnvironment` can be injected directly into the `InvokeAsync` method.
**Prevention:** Always use exact matching (`==`) or strict directory boundary matching (`StartsWith("/path/")` with a trailing slash) for path-based authorization exceptions. Ensure environment-specific bypass logic explicitly checks `env.IsDevelopment()`.
