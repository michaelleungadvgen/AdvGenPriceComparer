## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authorization Bypass via Substring Matching & Missing Environment Check
**Vulnerability:**
1. Middleware path exclusion logic used `.Contains("/swagger")`, allowing attackers to bypass rate limits and authentication by appending `/swagger` to any path (e.g., `/api/prices/upload/swagger`).
2. `ApiKeyMiddleware.cs` allowed public read access to `/api/prices` endpoints based on a comment "// Allow anonymous access in development", but failed to actually verify `IWebHostEnvironment.IsDevelopment()`, inadvertently exposing production data.
**Learning:** Using substring matching for URL path authorization checks is fundamentally insecure. Furthermore, comments about environment-specific behavior must be strictly enforced with actual runtime checks.
**Prevention:** In custom ASP.NET Core middleware, always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions. When bypassing authentication for development endpoints, always explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()`.
