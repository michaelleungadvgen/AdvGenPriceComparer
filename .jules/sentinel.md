## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2023-10-25 - Path.Contains Authorization Bypass
**Vulnerability:** API key and rate-limiting middleware used `path.Contains("/swagger")` and `path.Contains("/health")` to skip validation. This allowed an attacker to bypass authentication by appending `/swagger` or `/health` to any API route (e.g., `/api/prices/upload/swagger`).
**Learning:** Substring matching on request paths in middleware is extremely dangerous because ASP.NET Core `Path.Value` includes the full path. If any segment contains the substring, the entire security layer can be bypassed for sensitive endpoints.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for routing exclusions in custom middleware.
