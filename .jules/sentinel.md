## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-09 - Middleware Auth and Rate Limit Bypass via Path Substring Matching
**Vulnerability:** API key and rate limit middlewares used `path.Contains("/swagger")` and `path.Contains("/health")`, allowing any request to bypass security simply by appending `?foo=/swagger` or including the string anywhere in the path. Furthermore, anonymous read access was granted to `/api/prices` without verifying if the environment was actually Development.
**Learning:** In custom ASP.NET Core middleware, using `Contains()` for path exclusions creates severe authorization bypass vulnerabilities. Development-only bypasses must explicitly check `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path-based routing/security decisions. Always inject and check `IWebHostEnvironment` before bypassing security for development endpoints.
