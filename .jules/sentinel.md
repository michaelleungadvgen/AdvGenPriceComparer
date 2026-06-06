## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-06 - Middleware Authorization and Rate Limit Bypass via Substring Matching
**Vulnerability:** Middleware used `.Contains()` instead of `.StartsWith()` for path exclusions, allowing malicious paths like `/api/sensitive/swagger` to bypass auth and rate limits. Additionally, a development-only endpoint did not explicitly verify the environment, causing a public auth bypass in production.
**Learning:** Substring matching on paths in middleware creates bypass vulnerabilities. Development-only exemptions must explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Always use exact (`==`) or prefix (`StartsWith()`) matching for path exclusions. Inject `IWebHostEnvironment` into `InvokeAsync` for environment checks in middleware instead of relying on comments.
