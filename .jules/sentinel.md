## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Insecure Path Matching in Middleware [RESOLVED]
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `Contains()` instead of `StartsWith()` for path exclusions. This allowed attackers to bypass API key validation and rate limiting by appending `/health` or `/swagger` to any URL (e.g., `/api/prices/upload/health`).
**Learning:** Substring matching for routing exclusions is highly susceptible to path traversal or parameter manipulation attacks. Attackers can easily craft URLs that satisfy the substring condition while accessing protected endpoints.
**Prevention:** Always use exact matching or prefix path matching (`StartsWith()`) for routing exclusions in custom middleware to ensure robust security and prevent unauthorized access or rate limiting bypasses.
