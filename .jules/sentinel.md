## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-05-14 - Authorization and Rate Limit Bypass via Insecure Path Matching
**Vulnerability:** Custom middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `path.Contains("/swagger")` and `path.Contains("/health")` to skip authentication and rate limiting for specific endpoints. This allows an attacker to append `/swagger` or `/health` anywhere in the path (e.g., `/api/prices/upload/swagger`) to bypass these security controls.
**Learning:** Using substring matching (`Contains()`) for routing security exceptions is a common anti-pattern that leads to authorization and rate limit bypasses.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for middleware path exclusions to ensure the bypass only applies to the intended root paths.
