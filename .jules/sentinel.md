## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-07-05 - Insecure Path Matching and Missing Environment Checks in ASP.NET Core Middleware
**Vulnerability:** Use of `.Contains("/swagger")` or `.StartsWith("/api/prices")` in middleware allowed unauthenticated/unrate-limited access to any path containing those strings (e.g., `/api/prices-bypass`). The `/api/prices` endpoint bypass also lacked an explicit environment check, making it accessible in production.
**Learning:** In custom ASP.NET Core middleware, path exclusions must use exact matching (`==`) or directory boundary prefix matching (`.StartsWith("/path/")`). Environment-specific bypasses (like dev-only endpoints) must explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()`. Dependencies like `IWebHostEnvironment` can be injected directly into `InvokeAsync`.
**Prevention:** Always use strict path matching logic and verify the application environment explicitly when implementing development-only features.
