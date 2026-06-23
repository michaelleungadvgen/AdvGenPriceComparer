## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-23 - Fix Path Matching Vulnerabilities in Middleware
**Vulnerability:** Middleware authorization and rate limiting used `Contains("/swagger")`, `Contains("/health")`, and `StartsWith("/api/prices")` (without trailing slash or exact matching) allowing for authentication and rate limit bypasses on endpoints containing these strings but lacking proper constraints. In addition, development environment checks were completely omitted on the anonymous access bypass on `/api/prices` routes.
**Learning:** Partial path matching functions (`Contains` and `StartsWith` on incomplete route bounds) combined with missing environment validation (e.g. missing `env.IsDevelopment()`) in custom ASP.NET Core middleware introduces severe risks of authorization, rate-limit bypasses, and data exposure in production environments.
**Prevention:** Always use precise path matching (`==` or `StartsWith` with trailing slashes representing directory boundaries) for path exclusions. In addition, always enforce `IWebHostEnvironment.IsDevelopment()` checks before permitting development-only unauthenticated bypass routes.
