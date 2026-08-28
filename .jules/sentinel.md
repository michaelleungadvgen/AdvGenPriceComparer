## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-07-03 - Fix Authorization and Rate Limit Path Bypass
**Vulnerability:** Path substring matching (e.g., `path.Contains("/swagger")`) in custom middleware allowed attackers to bypass authentication and rate limits on protected endpoints simply by appending `/swagger` to the URL. Additionally, development bypass logic was executing in production due to a missing environment check.
**Learning:** When developing custom routing or middleware exclusions in ASP.NET Core, path matching must be exact or use strict prefix matching with directory boundaries. Furthermore, dependencies like `IWebHostEnvironment` can be injected directly into middleware `InvokeAsync` parameters.
**Prevention:** Always use `StartsWith("/path/") || == "/path"` for middleware exclusions, never `Contains()`. Ensure development-only overrides explicitly check `IWebHostEnvironment.IsDevelopment()`.
