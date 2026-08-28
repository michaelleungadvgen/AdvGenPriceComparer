## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-05-15 - Middleware Path Matching Bypass
**Vulnerability:** Authorization and rate-limiting middleware were using `.Contains("/health")` and `.Contains("/swagger")` to bypass checks on specific endpoints. This allowed attackers to bypass security by appending these strings to any protected URL (e.g., `/api/secure-data/health`).
**Learning:** Using substring matching for security routing exclusions is dangerous and prone to manipulation.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for middleware routing exclusions.
