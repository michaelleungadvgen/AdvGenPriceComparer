## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-07-07 - Fix Authorization and Rate Limiting Bypasses
**Vulnerability:** Middleware authorization (`ApiKeyMiddleware`) and rate limiting (`RateLimitMiddleware`) could be bypassed by exploiting insecure path exclusions using `Contains` and missing environment checks for development-only code.
**Learning:** Using substring matching (`Contains`) or unbounded prefix matching (`StartsWith` without a trailing slash) for URL path exclusions allows attackers to access restricted endpoints by appending the bypass string to their requests (e.g., `/api/admin/data?q=/swagger`). Additionally, development-only bypasses must always explicitly check the environment (`IWebHostEnvironment.IsDevelopment()`) to prevent leakage into production.
**Prevention:** Always use exact matching (`==`) or bounded prefix matching (`StartsWith("/path/")`) for path exclusions. Always verify the current environment before executing development-only logic in middleware.
