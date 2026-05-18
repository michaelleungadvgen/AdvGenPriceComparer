## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authorization Bypass via Substring Path Matching [RESOLVED]
**Vulnerability:** The API key and rate limit middleware used `.Contains("/swagger")` and `.Contains("/health")` to skip validation for public endpoints. This allowed an attacker to bypass authentication and rate limits on protected routes simply by appending a matching string to the path.
**Status:** ✅ FIXED - Replaced `.Contains()` with `.StartsWith()` for path exclusion matching.
**Implementation:**
- Modified `ApiKeyMiddleware.cs` path checks.
- Modified `RateLimitMiddleware.cs` path checks.
**Learning:** Using substring matching (`Contains`) for access control rules on request paths is highly dangerous because it matches anywhere in the URI, often ignoring URL boundaries.
**Prevention:** In custom ASP.NET Core middleware (e.g., `ApiKeyMiddleware`, `RateLimitMiddleware`), always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions to prevent authorization and rate limit bypass vulnerabilities.
