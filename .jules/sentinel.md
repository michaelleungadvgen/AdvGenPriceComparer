## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-30 - Path Matching and Environment Check Bypasses in Middleware
**Vulnerability:** Middleware paths were checked using `Contains()` instead of exact matching, allowing arbitrary paths with substrings (e.g., `/api/data?q=/swagger`) to bypass authentication and rate limits. Additionally, a development-only exception for `/api/prices` lacked a check for `IWebHostEnvironment.IsDevelopment()`, exposing it in production.
**Learning:** In ASP.NET Core custom middleware, `Contains()` for path exclusions is inherently insecure. Also, dependency injection via the `InvokeAsync` parameter list can be used to inject services like `IWebHostEnvironment` directly without modifying the constructor.
**Prevention:** Always use exact matching (`==`) or strict prefix matching (`StartsWith("/path/")`) with directory boundaries for exclusions. Always explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()` when creating dev-only bypasses.
