## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Auth Bypass via Substring Matching in Middleware [RESOLVED]
**Vulnerability:** The `ApiKeyMiddleware` and `RateLimitMiddleware` used `Contains()` and `StartsWith("/api/prices")` for path exclusions, which created authentication bypass vulnerabilities. For example, a request to `/api/prices-admin/delete` would bypass authentication.
**Learning:** Using substring matching (`Contains`) or loose prefix matching (`StartsWith` without a trailing slash) in authorization middleware allows attackers to craft paths that match the exclusion rule but route to sensitive endpoints.
**Prevention:** In custom ASP.NET Core middleware (e.g., `ApiKeyMiddleware`, `RateLimitMiddleware`), always use exact matching (`==`) or strict prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions.

## 2025-03-05 - Auth Bypass via Substring Matching in Middleware [RESOLVED]
**Vulnerability:** The `ApiKeyMiddleware` and `RateLimitMiddleware` used `Contains()` and `StartsWith("/api/prices")` for path exclusions, which created authentication bypass vulnerabilities. For example, a request to `/api/prices-admin/delete` would bypass authentication.
**Learning:** Using substring matching (`Contains`) or loose prefix matching (`StartsWith` without a trailing slash) in authorization middleware allows attackers to craft paths that match the exclusion rule but route to sensitive endpoints. Note: Ensure not to lock down public endpoints to development environments only when fixing these issues, as that breaks intended functionality.
**Prevention:** In custom ASP.NET Core middleware (e.g., `ApiKeyMiddleware`, `RateLimitMiddleware`), always use exact matching (`==`) or strict prefix matching with a directory boundary (`StartsWith("/path/")`) for path exclusions. Keep public endpoints public.
