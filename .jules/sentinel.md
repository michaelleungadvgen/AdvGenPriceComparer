## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authentication and Rate Limit Bypass via Loose Path Matching
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) used `path.Contains("/health")` and missing environment checks (`env.IsDevelopment()`) for "development-only" endpoint bypasses, allowing attackers to access protected endpoints without authentication by appending `/health` to paths or exploiting the missing `IsDevelopment()` check in production.
**Learning:** `Contains()` should never be used for path-based authorization bypasses, as it matches anywhere in the string. Missing `IsDevelopment()` checks on "development-only" logic inadvertently exposes features in production environments.
**Prevention:** Always use exact matching (`==`) or strict prefix matching (`StartsWith("/path/")`) with directory boundaries for exclusions. Always explicitly inject and verify `IWebHostEnvironment.IsDevelopment()` before bypassing security checks intended only for development.
