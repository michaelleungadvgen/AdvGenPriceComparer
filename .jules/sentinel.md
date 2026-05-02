## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-02 - Substring Path Matching Auth/Rate Limit Bypass [RESOLVED]
**Vulnerability:** Middleware exclusions used `path.Contains("/swagger")` and `path.Contains("/health")` instead of prefix matching.
**Learning:** Using substring matching for URL path rules allows attackers to bypass authentication or rate limiting by appending the excluded string to protected endpoints (e.g., `/api/prices/upload?bypass=/health`).
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for route-based security rules.
