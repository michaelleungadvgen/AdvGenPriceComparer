## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Auth Bypass via Substring Path Matching
**Vulnerability:** Path exclusion in custom middleware used `.Contains()` instead of `.StartsWith()`, allowing authentication and rate limit bypass by appending `/swagger` or `/health` to any API route. Missing environment check allowed public unauthenticated access to `/api/prices` in production.
**Learning:** In custom ASP.NET Core middleware (e.g., `ApiKeyMiddleware`, `RateLimitMiddleware`), always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions. Using substring matching (`Contains()`) creates authorization and rate limit bypass vulnerabilities.
**Prevention:** Always explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()` to prevent unauthorized access in production.
