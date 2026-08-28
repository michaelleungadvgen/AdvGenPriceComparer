## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-27 - [HIGH] Fix authorization bypass in middleware
**Vulnerability:** Using `Contains()` for path exclusion in middleware (`ApiKeyMiddleware.cs` and `RateLimitMiddleware.cs`) allows attackers to bypass API key authentication and rate limiting by appending "/swagger" to any API request path (e.g., `/api/sensitive-data?x=/swagger`).
**Learning:** This is a classic path-based authorization bypass vulnerability. It existed because `Contains()` matches the substring anywhere in the URL path, whereas the intent was to match the beginning of the path (`StartsWith()`) to exclude specific routes like `/swagger` and `/health`.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions in custom ASP.NET Core middleware to prevent authorization and rate limit bypass vulnerabilities. Never use substring matching (`Contains()`).
