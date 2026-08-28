## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-10-24 - Authorization and Rate Limit Bypass via Path Substring Matching [RESOLVED]
**Vulnerability:** The API Key and Rate Limit Middlewares used `Contains()` to exclude the Swagger UI and health check endpoints. This allowed authorization and rate limit bypasses by requesting paths like `/api/prices/swagger` or `/?x=/health`.
**Status:** ✅ FIXED - Path exclusions now use `StartsWith()`
**Implementation:**
- `ApiKeyMiddleware` updated from `path.Contains("/swagger")` to `path.StartsWith("/swagger")`.
- `RateLimitMiddleware` updated from `path.Contains("/health")` to `path.StartsWith("/health")`.
**Learning:** Using substring matching for URL path authorization exclusions is a frequent cause of security bypasses as attackers can inject the matched substring anywhere in the path.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in security-critical middleware to prevent arbitrary substring injection vulnerabilities.
