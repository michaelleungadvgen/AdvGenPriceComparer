## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-11 - Authentication Bypass via Substring Path Matching
**Vulnerability:** Middleware used `Contains()` instead of `StartsWith()` for path exclusions (e.g., `path.Contains("/swagger")`), allowing attackers to bypass API key validation and rate limiting by appending "/swagger" to any API request path. Additionally, public GET access intended for development did not explicitly verify `env.IsDevelopment()`.
**Learning:** Using substring matching for security exclusions creates critical authorization bypass vulnerabilities. Development-only rules must strictly enforce the environment context to prevent accidental production exposure.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in middleware. Always explicitly verify the environment (`IWebHostEnvironment.IsDevelopment()`) when conditionally bypassing authentication rules.
