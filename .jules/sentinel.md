## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-24 - Fix Middleware Bypass & IsDevelopment Checks
**Vulnerability:** Path-based authentication and rate-limiting bypass via `path.Contains()` instead of `.StartsWith()`. Also, dev-only endpoints lacked `env.IsDevelopment()` checks, exposing them in production.
**Learning:** Using `Contains()` on HTTP paths allows query parameters or nested path manipulation (e.g., `/api/admin?x=/health`) to bypass security filters. Furthermore, comments about dev-only endpoints must be enforced with actual code checks.
**Prevention:** Always use `StartsWith()` or exact matching `==` for path exclusions in custom middleware. Inject `IWebHostEnvironment` into `InvokeAsync` to enforce development-only endpoints.
