## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-24 - Middleware Route Exclusion Auth Bypass
**Vulnerability:** APIKey and RateLimit custom middlewares used `.Contains()` to exclude Swagger and Health endpoints, allowing attackers to bypass authentication by appending those strings anywhere in the path. `ApiKeyMiddleware` also bypassed auth for `/api/prices` in "development" but failed to check `env.IsDevelopment()`.
**Learning:** Using `.Contains()` for route matching in custom middleware creates critical authorization bypass vulnerabilities. Also, comments indicating "development only" must be backed by explicit code checks.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions. Inject `IWebHostEnvironment` into `InvokeAsync` to explicitly verify the environment for development-only bypasses.
