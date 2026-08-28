## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-30 - Path Traversal & Auth Bypass in Middleware
**Vulnerability:** API endpoints were vulnerable to authentication and rate limit bypass due to using `Contains()` for path matching, and development-only anonymous access lacked actual environment checks.
**Learning:** Using `Contains()` for path-based authorization allows attackers to append bypass strings (like `/swagger`) to protected URLs. Similarly, missing `IsDevelopment()` checks on developer-only endpoints inadvertently exposes them in production.
**Prevention:** Always use exact matching (`==`) or directory-bounded prefix matching (`StartsWith("/path/")`) for route authorization exclusions. Explicitly verify the environment (`env.IsDevelopment()`) before granting development-specific privileges.
