## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-24 - Middleware Path Exclusion Bypass
**Vulnerability:** API key authentication and rate limiting could be bypassed by including `swagger` or `health` anywhere in the URL path (e.g. `/api/admin?q=health`) due to the use of `Contains()`. Additionally, Development-only anonymous endpoints were exposed in production because `IWebHostEnvironment.IsDevelopment()` was not actually checked.
**Learning:** Substring matching (`Contains()`) or prefix matching without directory boundaries (`StartsWith("/api/prices")`) in middleware path exclusions creates critical authorization and rate limit bypass vulnerabilities. Development-only comments are not enforced by the compiler.
**Prevention:** Always use exact matching (`==`) or strict prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. In ASP.NET Core middleware, dependencies like `IWebHostEnvironment` can be injected directly into `InvokeAsync` to explicitly verify environment conditions before applying development-only overrides.
