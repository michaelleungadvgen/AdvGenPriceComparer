## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authentication and Rate Limit Bypass via Path Substrings
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used `Contains("/swagger")` and `Contains("/health")` to skip validation. This allows an attacker to bypass authentication and rate limits by simply appending `?q=/swagger` or creating paths like `/api/prices/swagger` that contain the substring. Furthermore, `ApiKeyMiddleware` allowed anonymous access to `/api/prices` in "development" based solely on a comment without actual `IWebHostEnvironment.IsDevelopment()` verification, exposing the API in production.
**Learning:** Substring matching (`Contains()`) on request paths is a common and critical vulnerability pattern in custom middleware designed to exclude paths from security checks. Developers also often rely on verbal/commented assumptions (e.g., "Allow in development") without programmatic enforcement.
**Prevention:** Always use strict path matching (`StartsWith()` or exact `==`) for security exclusions. Always explicitly inject and verify `IWebHostEnvironment.IsDevelopment()` for any development-only bypass logic.
