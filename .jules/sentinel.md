## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-27 - Auth Bypass in Middleware
**Vulnerability:** Substring matching (`Contains`) and missing environment checks in middleware allowed bypasses.
**Learning:** The custom middleware used `Contains` for path exclusion, which allowed bypassing by simply appending `/swagger` to any path. Furthermore, the `ApiKeyMiddleware` failed to restrict the public read access solely to the development environment.
**Prevention:** Always use `StartsWith` or exact matches (`==`) for path exclusions and explicitly check `env.IsDevelopment()` before granting anonymous access intended only for development.
