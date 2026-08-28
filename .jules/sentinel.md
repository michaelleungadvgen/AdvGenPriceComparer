## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-05-20 - Path Exclusion Bypass in ASP.NET Core Middleware [RESOLVED]
**Vulnerability:** API key and Rate Limit middlewares used `path.Contains("/swagger")` and `path.Contains("/health")` to skip checks. An attacker could bypass authentication and rate limits by appending `/swagger` to any URL (e.g., `/api/prices/upload/swagger`).
**Learning:** Using substring matching (`.Contains()`) for authorization exclusions is a common anti-pattern that creates critical bypass vulnerabilities.
**Prevention:** Always use exact matching (`==`) or prefix matching (`.StartsWith()`) for path exclusions in custom ASP.NET Core middleware.
