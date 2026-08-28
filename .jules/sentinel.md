## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-07-04 - Fixed Rate Limit and Authorization Bypass in Middleware
**Vulnerability:** Authorization and Rate Limiting bypass was possible via the  method matching trailing parameters (e.g., ).
**Learning:** Using  or similar substring matching in ASP.NET Core middleware creates an implicit bypass vulnerability, as the substring could occur anywhere in the request path.
**Prevention:** Always use strict path prefix matching (e.g., ) or exact string matches when attempting to bypass security middleware for specific routes.
## 2023-11-09 - Fixed Rate Limit and Authorization Bypass in Middleware
**Vulnerability:** Authorization and Rate Limiting bypass was possible via the `Contains` method matching trailing parameters (e.g., `?param=/swagger`).
**Learning:** Using `path.Contains("/swagger")` or similar substring matching in ASP.NET Core middleware creates an implicit bypass vulnerability, as the substring could occur anywhere in the request path.
**Prevention:** Always use strict path prefix matching (e.g., `path.StartsWith("/swagger/") || path == "/swagger"`) or exact string matches when attempting to bypass security middleware for specific routes.
