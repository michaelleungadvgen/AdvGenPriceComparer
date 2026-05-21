## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-03-01 - Middleware Path Exclusion Bypass
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) used substring matching (`.Contains()`) for path exclusions (e.g., `/swagger`, `/health`), allowing attackers to bypass authentication and rate limiting by appending those strings to any request path.
**Learning:** Using substring matching for URL path routing or authorization exclusions creates critical bypass vulnerabilities, as an attacker controls the full path and can embed the excluded string anywhere.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in custom middleware.
