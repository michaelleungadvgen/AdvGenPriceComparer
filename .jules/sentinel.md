## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-03-24 - Authorization and Rate Limiting Bypass via Substring Path Matching
**Vulnerability:** The API key middleware and rate limiting middleware used `path.Contains("/swagger")` and `path.Contains("/health")` to skip validation. This allowed an attacker to bypass authentication and rate limits entirely by appending those strings to any URL (e.g., `/api/admin?query=health`). Additionally, the `/api/prices` bypass was missing an `IsDevelopment()` check.
**Learning:** Using substring matching (`Contains`) for security boundaries is dangerous as paths or query strings can easily embed the string. `StartsWith` without a trailing slash (e.g., `/api/prices` matching `/api/prices_fake`) also poses risks. Development-only routes must explicitly check the environment.
**Prevention:** Always use exact matching (`==`) or precise prefix matching with directory boundaries (`StartsWith("/path/")`) for path-based authorization bypasses. Always wrap development overrides with `IWebHostEnvironment.IsDevelopment()` checks.
