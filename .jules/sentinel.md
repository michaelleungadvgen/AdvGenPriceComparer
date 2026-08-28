## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-07-08 - Path Matching Authorization and Rate Limit Bypass
**Vulnerability:** Middleware used `Contains()` and boundary-less `StartsWith()` for path exclusions, creating authorization and rate limiting bypasses (e.g. `/api/prices_fake` bypassing auth). Additionally, the development bypass was active in production due to a missing `IWebHostEnvironment.IsDevelopment()` check.
**Learning:** Using substring matching or prefix matching without a trailing slash for path exclusions is dangerous. Development-only code must be explicitly gated.
**Prevention:** Always use exact matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`). Ensure `IWebHostEnvironment.IsDevelopment()` is explicitly verified.
