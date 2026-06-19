## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-19 - Insecure Path Matching in Middleware and Missing Environment Checks
**Vulnerability:** Path matching logic in custom middleware (`ApiKeyMiddleware.cs` and `RateLimitMiddleware.cs`) used `Contains("/swagger")` and `Contains("/health")`, which allowed attackers to bypass authentication and rate limits by simply appending `?param=/swagger` or requesting an endpoint like `/api/data/swagger-bypass`. Additionally, `ApiKeyMiddleware` had a hardcoded bypass for GET requests to `/api/prices` intended for development but lacking the `IWebHostEnvironment.IsDevelopment()` check, exposing it in production.
**Learning:** Using `Contains` for URL path checking in custom routing/middleware is highly insecure. Bypassing authentication for development endpoints requires explicitly checking `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Use exact matching (`==`) or strict directory prefix matching (`StartsWith("/path/")`) for path exclusions. Always verify the hosting environment (`IsDevelopment()`) before granting anonymous access.
