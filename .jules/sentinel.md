## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-30 - Authorization and Rate Limiting Bypass via Substring Path Matching
**Vulnerability:** Middleware used `path.Contains("/swagger")` which allows attackers to bypass API key validation and rate limiting by appending `/swagger` to any path (e.g., `/api/sensitive?q=/swagger`). Additionally, an intended development-only bypass lacked an `env.IsDevelopment()` check.
**Learning:** Using substring matching (`Contains()`) for URL path-based access control creates authorization bypass vulnerabilities. Also, relying on code comments rather than programmatic checks for environment-specific logic leads to security misconfigurations.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in custom ASP.NET Core middleware. Explicitly inject and use `IWebHostEnvironment.IsDevelopment()` when applying development-only security bypasses.
