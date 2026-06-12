## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-12 - Prevent Path Injection Authorization Bypass in ASP.NET Middleware
**Vulnerability:** API Key and Rate Limit middleware used `Contains()` for path exclusions (e.g. `/swagger`) instead of exact or prefix matching, allowing attackers to bypass authentication by appending `?q=/swagger` to any endpoint. The `/api/prices` bypass lacked environment checks and used partial prefix matching.
**Learning:** Substring matching (`Contains`) and prefix matching without a trailing slash (e.g. `StartsWith("/api/prices")`) create authorization and rate limit bypass vulnerabilities in ASP.NET custom middleware.
**Prevention:** Always use exact matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. When bypassing authentication for development-only endpoints, always explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()`.
