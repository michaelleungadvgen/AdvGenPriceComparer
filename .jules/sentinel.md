## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-08 - Path-based Authorization Bypass in Middleware [RESOLVED]
**Vulnerability:** API Key and Rate Limit middleware used `.Contains("/swagger")` and `.Contains("/health")` to skip validation for public endpoints. This allowed an attacker to bypass authentication on protected routes by simply appending `/swagger` or `/health` to the end of any valid API path (e.g., `/api/prices/upload/swagger`).
**Learning:** Substring matching (`Contains`) on URL paths for security exclusions creates dangerous bypass opportunities in ASP.NET Core middleware, as the routing system often ignores unrecognized trailing path segments.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for URL-based security routing rules to prevent unintended inheritance or manipulation of paths.
