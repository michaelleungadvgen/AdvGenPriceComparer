## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-18 - Path Substring Matching Auth/RateLimit Bypass
**Vulnerability:** Substring matching (`Contains()`) used for path exclusions in custom middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) allows bypassing security checks if a malicious path simply includes the excluded term anywhere in the route (e.g. `/api/admin/swagger`).
**Learning:** Checking for substrings like `/swagger` or `/health` in `context.Request.Path` creates easily exploitable bypasses when paths are dynamically resolved or parameterized by other endpoints.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) when excluding paths from security middleware validations.
