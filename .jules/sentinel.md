## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-03-05 - Insecure Middleware Path Matching Authorization Bypass
**Vulnerability:** API key middleware and rate limit middleware used substring matching (`path.Contains()`) instead of prefix matching (`path.StartsWith()`) for endpoint exclusions. This enabled authorization bypass (e.g., hitting `/api/prices/upload/swagger` would incorrectly skip security checks because the path contained `/swagger`).
**Learning:** Middleware exclusions using loose string matching create trivial bypass vulnerabilities, as attackers can easily append excluded strings to paths intended to be protected.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for middleware path exclusions in ASP.NET Core applications.
