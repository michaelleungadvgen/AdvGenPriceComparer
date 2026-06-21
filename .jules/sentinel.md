## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-21 - Middleware Path Matching & Environment Checks
**Vulnerability:** Middleware used `.Contains("/path")` for exclusions, allowing authentication and rate limiting bypass via appended URL segments. Furthermore, a development-only exception for an API endpoint lacked an environment check, making it public in production.
**Learning:** In ASP.NET Core, `.Contains()` in path matching is inherently insecure. Also, comments indicating "development-only" must be programmatically enforced with `IWebHostEnvironment.IsDevelopment()`, which can be injected directly into `InvokeAsync`.
**Prevention:** Always use exact matching (`==`) or directory-bound prefix matching (`.StartsWith("/path/")`). Always explicitly verify the environment for bypasses meant only for development.
