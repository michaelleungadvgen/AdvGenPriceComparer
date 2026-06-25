## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-27 - Fix Authentication & Rate Limit Bypass Vulnerability
**Vulnerability:** The `ApiKeyMiddleware` and `RateLimitMiddleware` used `.Contains("/path")` for path exclusions and `StartsWith("/api/prices")` for development bypasses, which could be exploited to bypass authentication and rate limits completely (e.g., `/api/prices-admin` or `/swagger-bypass`). Furthermore, the development bypass did not check if the environment was actually development.
**Learning:** Using overly broad substring or prefix matching without checking environment bounds creates severe authorization and DoS vulnerabilities. Always use exact matches or strict directory prefixes.
**Prevention:** Use exact path matching (`==`) or strict directory prefix matching (`StartsWith("/path/")` with trailing slash). For development-only routes, always explicitly check `IWebHostEnvironment.IsDevelopment()`.
