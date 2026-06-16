## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-18 - Path Bypasses in Custom Middleware
**Vulnerability:** The custom ASP.NET Core middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) used `Contains("/health")` and `StartsWith("/api/prices")` for path exclusions, which created authorization and rate limit bypass vulnerabilities (e.g., `/health-check-fake`, `/api/prices-fake`).
**Learning:** In custom ASP.NET Core middleware, using substring matching (`Contains()`) or prefix matching without a trailing slash (e.g., `StartsWith("/api/prices")`) creates significant security gaps. Always use exact matching (`==`) or prefix matching with a directory boundary (`StartsWith("/path/")`).
**Prevention:** Always use strict path validation in middleware by explicitly checking `path == "/route"` or `path.StartsWith("/route/")` to prevent bypasses.
