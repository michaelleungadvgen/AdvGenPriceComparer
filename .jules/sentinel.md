## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Path Validation Vulnerabilities in Middleware
**Vulnerability:** Insecure path validation using `.Contains()` or `.StartsWith()` without a trailing slash allows unintended authorization bypasses. For instance, `path.Contains("/swagger")` matches paths like `/api/swagger_bypass` and `path.StartsWith("/api/prices")` matches `/api/prices_bypass`.
**Learning:** Middleware often checks incoming requests against string patterns to apply specific logic, such as skipping API key validation or granting public access. Simple string matching methods are prone to prefix-matching bugs or substring vulnerabilities.
**Prevention:** Use exact matching `==` for fixed routes (e.g., `path == "/health"`). For directory patterns, explicitly match the directory boundary by including the trailing slash (e.g., `path.StartsWith("/swagger/")`).
