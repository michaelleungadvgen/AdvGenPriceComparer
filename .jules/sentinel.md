## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-06-11 - [Middleware Path Matching Vulnerability]
**Vulnerability:** The custom API key and Rate Limit middlewares in `AdvGenPriceComparer.Server` used `.Contains()` or exact matches against prefixes (e.g., `StartsWith("/api/prices")`) to bypass authentication and rate limits for specific routes like `/swagger`, `/health`, or public GET endpoints. This allowed an attacker to bypass authentication/rate limiting entirely by simply appending the bypassed string to any protected path (e.g. `/api/secret-data/swagger` would be allowed) or spoofing prefixes (e.g. `/api/prices-admin`).
**Learning:** In ASP.NET Core custom middleware, path checking must be extremely precise to prevent bypasses. Methods like `.Contains()` look for the string anywhere in the path, leading to unintended matches. `.StartsWith()` without a trailing slash matches unrelated routes that happen to share a prefix.
**Prevention:** Always use exact matching (`==`) or prefix matching with a clear directory boundary (`StartsWith("/path/")`) when excluding routes in custom middleware to ensure only the intended endpoints are bypassed.
