## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - Path Exclusion Vulnerabilities in Middleware
**Vulnerability:** API Key and Rate Limit bypass vulnerabilities due to using `.Contains()` in path exclusions and missing `env.IsDevelopment()` checks.
**Learning:** Using `path.Contains("/swagger")` allows bypassing authentication by requesting paths like `/api/sensitive?q=/swagger`. Additionally, development-only bypasses must explicitly check `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions. Always verify the environment for development-only bypasses.
