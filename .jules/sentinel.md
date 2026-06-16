## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-16 - Authentication & Rate Limit Bypass via Loose Path Matching
**Vulnerability:** Middleware used `.Contains("/swagger")` and `.StartsWith("/api/prices")` for path exclusion and bypassed authentication for `/api/prices` without verifying the environment.
**Learning:** Using loose substring matching allows attackers to bypass security middleware by appending or prepending strings (e.g., `/api/prices/sensitive` or `/my-secure-path?param=/swagger`). Failing to check `IsDevelopment()` exposes development-only endpoints in production.
**Prevention:** Always use exact matching (`==`) or strict boundary prefixing (`StartsWith("/path/")`) for route exclusions. Always verify `IWebHostEnvironment.IsDevelopment()` before allowing anonymous access to development endpoints.
