## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-27 - Authorization Bypass in Custom Middleware
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware`, `RateLimitMiddleware`) used `path.Contains("/swagger")` and `path.StartsWith("/api/prices")` (without trailing slash) to bypass authentication/rate limits. `ApiKeyMiddleware` also allowed anonymous GET access to `/api/prices` without verifying `IWebHostEnvironment.IsDevelopment()`. This allowed attackers to bypass authentication by embedding `/swagger` in paths (e.g., `/api/admin/swagger`) or using prefix overlaps (e.g., `/api/prices-admin`), and exposed production endpoints.
**Learning:** Using substring matching (`Contains`) or loose prefix matching (`StartsWith` without directory boundary) on request paths creates authorization bypass vulnerabilities. Development-only exceptions must explicitly verify the environment. `IWebHostEnvironment` can be injected directly into `InvokeAsync`.
**Prevention:** Always use exact matching (`==`) or strict prefix matching (`StartsWith("/path/")`) with trailing slashes for path exclusions. Always explicitly verify `IWebHostEnvironment.IsDevelopment()` before allowing development-only bypasses.
