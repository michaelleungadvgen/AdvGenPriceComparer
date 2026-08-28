## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-22 - Authorization Bypass via Substring Path Matching in Middleware
**Vulnerability:** `ApiKeyMiddleware` and `RateLimitMiddleware` used `path.Contains("/health")` and `path.Contains("/swagger")` to bypass authentication and rate limits. An attacker could bypass these protections on sensitive endpoints by crafting a path that includes these strings (e.g., `/api/restricted/health`).
**Learning:** Substring matching (`Contains`) on request paths for authorization logic is dangerous as it matches anywhere in the path string, not just the base route.
**Prevention:** Always use prefix matching (`StartsWith`) or exact matching (`==`) for URL path-based exclusions in custom ASP.NET Core middleware to restrict matches strictly to the intended endpoints.
