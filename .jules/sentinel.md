## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Authorization and Rate Limit Bypass in Middleware
**Vulnerability:** Middleware used substring matching (`.Contains()`) for path exclusions, creating a bypass risk, and missing environment validation for development endpoints exposed them to production.
**Learning:** Using `Contains()` instead of precise `StartsWith()` or exact matching allows attackers to craft bypass URIs. `IWebHostEnvironment` can be injected directly into `InvokeAsync` in ASP.NET Core middleware to enable proper `IsDevelopment()` checks.
**Prevention:** Always use exact matching or `StartsWith()` for authorization/rate limit exclusions based on path, and verify environment before enabling development bypasses.
