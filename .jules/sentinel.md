## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-09 - Authorization and Rate Limit Bypass in Middleware
**Vulnerability:** Custom `ApiKeyMiddleware` and `RateLimitMiddleware` used `.Contains()` instead of `.StartsWith()` or exact matching for routing exclusions (e.g., bypassing auth for `/swagger` and `/health`). This allowed an attacker to bypass authentication and rate limits on protected API endpoints by appending `/swagger` or `/health` to the path (e.g., `/api/prices/upload/health`).
**Learning:** Substring matching for URL paths in middleware can be easily exploited to bypass security controls when developers attempt to create generic exclusion rules.
**Prevention:** Always use strict path matching (`==`) or prefix matching (`.StartsWith()`) when defining route exclusions in custom middleware.
