## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-15 - Middleware Authorization Bypass via Path Substring Matching
**Vulnerability:** Custom API Key and Rate Limiting middlewares used `Contains("/swagger")` and `Contains("/health")` to skip validation. This allowed attackers to bypass authentication and rate limits entirely by requesting a protected endpoint and appending the excluded string (e.g., `/api/protected-resource/swagger`).
**Learning:** Using substring matching (`Contains`) for path exclusions in custom ASP.NET Core middleware creates trivial bypass vectors. Attackers control the requested path and can easily craft requests that include the target resource while still satisfying the substring condition.
**Prevention:** Always use exact matching (`==`) or strict prefix matching (`StartsWith()`) for path exclusions in security or routing middleware to prevent path-based authorization bypasses.
