## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2023-10-27 - Middleware Path Exclusion Bypass [RESOLVED]
**Vulnerability:** Path exclusions in `ApiKeyMiddleware` and `RateLimitMiddleware` used `Contains()` (e.g. `path.Contains("/health")`). Attackers could bypass authentication and rate limits by appending an excluded string to a protected endpoint (e.g. `/api/prices/upload/health`).
**Learning:** Using substring matching for security routing creates severe vulnerabilities, as the matching string can appear anywhere in the path.
**Prevention:** Always use exact matching (`==`) or strict prefix matching (`StartsWith()`) for middleware path exclusions.
