## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-07-01 - Fix Authorization and Rate Limit Bypass in Middleware
**Vulnerability:** Middleware used `Contains()` for path matching (e.g., `path.Contains("/swagger")`), allowing attackers to bypass authentication and rate limits by appending the string anywhere in the path (e.g., `/api/secure-endpoint?fake=/swagger`). Additionally, a development-only endpoint bypass lacked an `env.IsDevelopment()` check, exposing it in production.
**Learning:** In custom ASP.NET Core middleware, using substring matching or prefix matching without directory boundaries (e.g., `StartsWith("/api/prices")` instead of `StartsWith("/api/prices/")`) creates severe authorization bypass vulnerabilities. Furthermore, development-only bypasses must always explicitly verify the environment.
**Prevention:** Always use exact matching (`==`) or strict prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. Always verify `IWebHostEnvironment.IsDevelopment()` when bypassing security controls for development.
