## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2023-10-27 - Insecure Path Matching in Middleware Exceptions [RESOLVED]
**Vulnerability:** Custom authentication and rate limiting middleware used `Contains("/swagger")` instead of `StartsWith("/swagger")` when checking the request path. This allowed an attacker to bypass API key validation and rate limiting by appending `/swagger` anywhere in the URL (e.g., `/api/protected/data?bypass=/swagger`).
**Learning:** Substring matching (`Contains`) on request paths for security bypass rules is inherently flawed because it can match user-controlled segments of the URL, such as path parameters or query strings.
**Prevention:** Always use prefix matching (`StartsWith`) or exact matching (`==`) when defining path-based exclusions for security middleware. Ensure the path being checked is correctly normalized before comparison.
