## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - API Authorization and Rate Limit Bypass in Middleware
**Vulnerability:** API Key and Rate Limit Middlewares used substring matching (`Contains("/swagger")`) for path exclusions, allowing attackers to bypass authentication and rate limits by including the string anywhere in the path (e.g., `/api/sensitive/swagger`).
**Learning:** In ASP.NET Core middleware, using `Contains()` for path exclusions is inherently insecure and creates bypass vulnerabilities.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in custom middleware.
