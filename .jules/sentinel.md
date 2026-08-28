## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-06-14 - Middleware Path Exclusion Bypass via StartsWith
**Vulnerability:** `ApiKeyMiddleware` and `RateLimitMiddleware` used `.Contains()` or `.StartsWith("/api/prices")` for path exclusions, which allows an attacker to bypass authentication/rate limits by using paths like `/api/prices_fake` or `/swagger_bypass`.
**Learning:** Using substring matching for path exclusions in middleware creates an authorization bypass vulnerability.
**Prevention:** In custom ASP.NET Core middleware, always use exact matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions.
