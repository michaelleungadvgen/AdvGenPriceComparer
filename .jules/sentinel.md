## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Environment-Specific Bypass Escaping to Production
**Vulnerability:** The API key middleware contained a hardcoded bypass for GET requests to `/api/prices` intended for development, but it was not guarded by an environment check, inadvertently exposing the endpoint to public, unauthenticated access in production.
**Learning:** Security bypasses intended solely for local development must explicitly verify the environment context (e.g., using `IWebHostEnvironment.IsDevelopment()`) rather than relying on route path patterns or HTTP method conditions alone, which persist across all deployments.
**Prevention:** Always inject and utilize the hosting environment context when implementing development-only exceptions in security middleware or authorization filters.
