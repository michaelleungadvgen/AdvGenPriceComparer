## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-05-10 - Path Match Vulnerability in Middleware
**Vulnerability:** API key and rate-limiting middleware used `.Contains("/swagger")` and `.Contains("/health")` to skip authentication and rate-limiting. A malicious user could craft a request to a protected endpoint like `/api/prices?foo=/swagger` or `/api/prices/swagger` to bypass these critical security controls.
**Learning:** Substring matching for URL paths in middleware is a common authorization bypass vector. Any uncontrolled segment of the path or query string containing the target string will falsely trigger the exclusion rule.
**Prevention:** Always use precise path matching (`.StartsWith()`, `.Equals()`, or framework-provided routing constraints) rather than loose substring matching (`.Contains()`) when evaluating request paths for security exclusions.
