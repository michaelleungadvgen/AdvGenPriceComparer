## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-07-05 - Insecure Path Matching in Middleware
**Vulnerability:** API key authorization and rate-limiting middleware used loose `.Contains("/swagger")` path matching, allowing attackers to bypass protections by appending `?q=/swagger` or using paths like `/api/prices/secret/swagger`. Also, the `/api/prices` development bypass lacked an environment check, allowing unauthorized access in production.
**Learning:** Using substring matching (`.Contains()`) or missing trailing slashes in prefix matching (`.StartsWith("/api/prices")` instead of `.StartsWith("/api/prices/")`) for route exclusions in ASP.NET Core middleware creates trivial bypasses.
**Prevention:** Always use exact matching (`==`) or explicit prefix matching with a directory boundary (`.StartsWith("/path/")`) for path exclusions. Additionally, always explicitly check `IWebHostEnvironment.IsDevelopment()` when implementing development-only bypasses.
