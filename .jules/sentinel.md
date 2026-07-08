## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-07-08 - Middleware Authorization and Rate Limit Bypass via Path Substring Matching
**Vulnerability:** Substring matching (`Contains()`) or incomplete prefix matching (e.g. `StartsWith("/api/prices")` without trailing slash) in ASP.NET Core middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) allows attackers to bypass authentication and rate limits by appending suffixes (e.g., `/swagger.json/malicious` or `/api/prices_fake`). Also, bypassing auth for dev endpoints didn't verify `env.IsDevelopment()`.
**Learning:** Always use exact matching (`==`) or strict directory boundary matching (`StartsWith("/path/")`) for exclusion paths. Injected dependencies like `IWebHostEnvironment` can be added directly to the middleware's `InvokeAsync` parameter list.
**Prevention:** Ensure path exclusions use boundary-safe matching (`StartsWith` with trailing slash + exact match for the base route). Always verify the environment using `IWebHostEnvironment.IsDevelopment()` before allowing bypasses intended only for local development.
