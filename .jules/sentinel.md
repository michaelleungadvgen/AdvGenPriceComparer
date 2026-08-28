## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-29 - Fix Authentication and Rate Limit Bypasses in Middleware
**Vulnerability:** Path checking in middleware used `path.Contains("/swagger")` and `path.StartsWith("/api/prices")`, allowing bypasses via paths like `/api/prices/swagger` or missing trailing slash matching. A development-only bypass was also missing the `IWebHostEnvironment.IsDevelopment()` check.
**Learning:** `Contains()` should never be used for authorization bypass checks. `StartsWith()` must include a trailing slash to prevent bypassing via similarly named endpoints. Development bypasses must explicitly verify the environment.
**Prevention:** Always use exact matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. Inject and check `IWebHostEnvironment` for development-only logic.
