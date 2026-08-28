## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-04 - Path Matching Vulnerability in Middleware
**Vulnerability:** API Key and Rate Limit middleware used `.Contains("/swagger")` and `.Contains("/health")` for path exclusions, allowing authorization and rate limiting bypass if an attacker crafted a URL like `/api/prices/update?id=1&fake=/swagger`.
**Learning:** Using substring matching (`Contains`) for routing exclusions creates a bypass vulnerability where restricted endpoints can be accessed by injecting the excluded substring elsewhere in the path.
**Prevention:** Always use exact or prefix path matching (`StartsWith` or `Equals`) when validating paths for security-sensitive middleware exclusions.
