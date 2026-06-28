## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-06-28 - Substring Path Matching and Missing Environment Checks in Middleware [RESOLVED]
**Vulnerability:** Middleware authorization and rate-limiting could be bypassed because `path.Contains("/swagger")` matches paths like `/api/secret/swagger_bypass`. Furthermore, anonymous access intended only for development environments did not verify `IWebHostEnvironment.IsDevelopment()`.
**Learning:** Using substring matching (`Contains`) for request path validation introduces significant authorization bypass vulnerabilities. Development-only logic must explicitly check the environment to prevent leaking into production.
**Prevention:** Use exact path matching (`==`) or robust prefix matching with directory boundaries (`StartsWith("/path/")`) for exclusion lists. Always inject and check `IWebHostEnvironment` before bypassing authentication for development endpoints.
