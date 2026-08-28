## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-28 - Authentication Bypass via Loose Path Matching
**Vulnerability:** API key middleware and rate limiting middleware used `Contains()` for path exclusions (e.g., `path.Contains("/swagger")`), allowing attackers to bypass authentication by appending `/swagger` or `/health` to the query string or URL of protected endpoints. Development bypass checks also lacked `IWebHostEnvironment.IsDevelopment()` checks.
**Learning:** In ASP.NET Core middleware, always use exact matching (`==`) or prefix matching (`StartsWith()`) for path-based authorization bypasses. Never use `Contains()`. Development-only logic must explicitly verify the environment.
**Prevention:** Use `StartsWith()` for sub-paths and `==` for exact paths in middleware logic. Add explicit `IsDevelopment()` checks for any feature intended only for local testing.
