## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-03-05 - Fix Authentication and Rate Limiting Bypass in Middleware
**Vulnerability:** The API key and rate limiting middlewares used `.Contains("/swagger")` and `.Contains("/health")` to skip validation, allowing attackers to bypass checks by appending `?foo=/swagger` to any endpoint. Additionally, the `ApiKeyMiddleware` skipped authentication for `/api/prices` without verifying if the environment was development, inadvertently allowing unauthenticated access in production.
**Learning:** Substring matching (`.Contains()`) on request paths creates critical bypass vulnerabilities. Development-only bypasses must explicitly check `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Always use exact matching (`==`) or directory boundary prefix matching (`.StartsWith("/path/")`) for path exclusions. Never rely on comments to enforce environment-specific logic; explicitly verify the environment using injected dependencies like `IWebHostEnvironment`.
