## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-06-14 - Authorization and Rate Limit Bypass via Substring Path Matching
**Vulnerability:** The API key and rate-limiting middleware used `.Contains("/swagger")` and `.Contains("/health")` to skip authentication. This allowed attackers to bypass authentication on any endpoint by simply appending `?query=/swagger` to the URL. Additionally, `StartsWith("/api/prices")` could allow access to a hypothetical `/api/prices_admin` endpoint.
**Learning:** Using substring matching (`Contains`) or loose prefix matching (`StartsWith` without a trailing slash) for URL paths in middleware security checks creates trivial authorization bypass vulnerabilities.
**Prevention:** Always use exact matching (`==`) or exact prefix matching with directory boundaries (e.g., `.StartsWith("/path/")`) when exempting paths from security controls. Additionally, ensure development-only endpoints are protected by explicitly verifying `IWebHostEnvironment.IsDevelopment()`.
