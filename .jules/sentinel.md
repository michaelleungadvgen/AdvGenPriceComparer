## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Middleware Authorization and Rate Limit Bypass [RESOLVED]
**Vulnerability:** Path exclusion checks in custom middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `Contains()` instead of exact or prefix matching, allowing attackers to bypass authentication and rate limits by appending the excluded string (like `/swagger`) to any URL (e.g., `/api/admin/swagger`).
**Learning:** Substring matching (`Contains()`) for routing or authorization exclusions creates critical vulnerabilities. A request to `/api/sensitive/health` might bypass security checks completely.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) when excluding paths from security middleware.
