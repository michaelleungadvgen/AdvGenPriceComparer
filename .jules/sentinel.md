## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Insecure Path Matching in Middleware
**Vulnerability:** Middlewares were using `.Contains()` to bypass authentication and rate limits for endpoints like `/health` and `/swagger`. An attacker could exploit this by appending these strings to any URL (e.g., `/api/admin/delete?q=/swagger`), bypassing security checks.
**Learning:** Using substring matching (`Contains()`) for security exclusions creates authorization and rate limit bypass vulnerabilities.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for path exclusions.

## 2025-02-28 - Missing Environment Verification in Bypass Logic
**Vulnerability:** A block of code in `ApiKeyMiddleware.cs` intended to allow anonymous access to `/api/prices` during development was executing unconditionally in all environments because it was missing an environment check.
**Learning:** Comments indicating code is for development only are not enforced by the compiler.
**Prevention:** Always explicitly verify the environment using `IWebHostEnvironment.IsDevelopment()` before applying development-only security bypasses.
