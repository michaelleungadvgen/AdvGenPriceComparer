## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authorization and Rate Limit Bypass via Path Substring Matching
**Vulnerability:** The API key authentication and rate limit middlewares used `path.Contains("/health")` and `path.Contains("/swagger")` to bypass checks. This allowed an attacker to bypass all authentication and rate limits on any endpoint by simply appending `?param=/health` or using path traversal tricks (like `/api/prices/upload/health`).
**Learning:** Using substring matching (`Contains()`) for routing exclusions is highly insecure and creates trivial bypasses.
**Prevention:** Always use exact or prefix matching (e.g., `StartsWith()`) for middleware path exclusions.
