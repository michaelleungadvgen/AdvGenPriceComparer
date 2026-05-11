## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-11 - Middleware Exclusion Bypass [RESOLVED]
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `path.Contains()` instead of exact or prefix matching for routing exclusions (e.g., `/health`, `/swagger`).
**Learning:** Using `Contains()` for path exclusions allows attackers to bypass authorization and rate limiting by simply appending the excluded string to any protected path (e.g., `/api/protected/health`).
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for middleware routing exclusions to prevent path manipulation bypasses.
