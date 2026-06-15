## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-15 - Fix Middleware Auth and Rate Limit Bypass
**Vulnerability:** Custom ASP.NET Core middleware used `Contains()` for path matching and failed to check `IWebHostEnvironment.IsDevelopment()` before bypassing auth, allowing production API endpoints to be accessed anonymously.
**Learning:** Substring matching for URL exclusions allows trivial bypasses (e.g., appending `?/swagger` to a protected route). Similarly, missing environment checks make development shortcuts dangerous in production.
**Prevention:** Always use exact matching (`==`) or directory boundaries (`StartsWith("/path/")`) for route exclusions. Never bypass auth without verifying `IWebHostEnvironment.IsDevelopment()`.
