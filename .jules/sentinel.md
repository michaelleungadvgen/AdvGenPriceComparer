## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-05-24 - Insecure Middleware Path Matching and Auth Bypass
**Vulnerability:** Path checks using `.Contains()` or `.StartsWith("/api/prices")` (without trailing slash) allow authorization and rate limit bypasses. Missing environment check allowed anonymous access in production.
**Learning:** Using substring matching (`Contains`) creates bypass opportunities (e.g., `/api/malicious?q=/swagger`). Omitting environment checks on development-only bypasses opens them in production.
**Prevention:** Always use exact matching (`==`) or directory boundary prefix matching (`StartsWith("/path/")`). Explicitly verify `IWebHostEnvironment.IsDevelopment()` before skipping authentication.
