## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-28 - Authentication and Rate Limit Bypass via Path Substring Matching
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `path.Contains("/swagger")` and `path.Contains("/health")` to skip authentication and rate limiting for development endpoints. This allowed malicious actors to bypass security controls by appending those substrings to any restricted URL (e.g., `/api/admin/secrets?fake=/swagger`). Furthermore, development-only bypasses were active in production because `IWebHostEnvironment.IsDevelopment()` wasn't being checked.
**Learning:** Using substring matching (`Contains`) for routing or authorization logic is inherently insecure as it can be easily manipulated via query parameters or path segments. Environmental bypasses must always be explicitly gated.
**Prevention:** In custom ASP.NET Core middleware, always use exact matching (`==`) or strict prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. Additionally, always inject `IWebHostEnvironment` and explicitly verify `env.IsDevelopment()` before enabling development-only bypasses.
