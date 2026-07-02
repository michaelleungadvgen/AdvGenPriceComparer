## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-07-02 - Path matching and Environment check bypass in Middleware
**Vulnerability:** Authorization and rate limit bypass in ASP.NET Core middleware caused by using insecure substring path matching (path.Contains) and lack of environment checks for anonymous endpoints.
**Learning:** Using `Contains()` allows attackers to bypass middleware by appending specific strings like `?path=/swagger`. Omitting explicit environment checks can accidentally expose debug or anonymous endpoints in production.
**Prevention:** Always use exact matching (`==`) or strict prefix matching with directory boundaries (`StartsWith("/path/")`) for path exclusions. When bypassing authentication for dev endpoints, explicitly inject and verify `IWebHostEnvironment.IsDevelopment()`.
