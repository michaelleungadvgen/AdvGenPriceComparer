## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-20 - Auth and Rate Limit Bypass due to Path Contains
**Vulnerability:** API key and rate limiting middleware used `path.Contains("/swagger")` and `path.Contains("/health")`, which allowed attackers to bypass authentication and rate limits on any endpoint by simply appending `/swagger` to the URL. Also, a development bypass for `/api/prices` lacked an environment check.
**Learning:** Using substring matching (`Contains()`) for routing exclusions is insecure. Prefix matching without directory boundary is also insecure. Always explicitly inject `IWebHostEnvironment` to check `env.IsDevelopment()` for dev bypasses.
**Prevention:** Use exact path matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`). Always explicitly check `IWebHostEnvironment.IsDevelopment()` before granting dev-only access.
