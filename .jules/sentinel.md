## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-07-01 - Prevent Path-Based Security Bypasses in Middleware

**Vulnerability:** Middleware bypass logic used `.Contains("/path")` and `.StartsWith("/path")` without environment checks, allowing attackers to bypass authentication and rate limits by appending the bypass string (e.g., `/api/prices/sensitive-path/health`) or accessing development endpoints in production.
**Learning:** Substring matching for path exclusions is inherently insecure as it matches anywhere in the path. Furthermore, development-only bypasses must explicitly verify the environment.
**Prevention:** Always use exact matching (`==`) or prefix matching with a trailing slash (`StartsWith("/path/")`) for path exclusions. Additionally, explicitly verify `IWebHostEnvironment.IsDevelopment()` for development-only bypasses.
