## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - Middleware Path Matching & Env Checks
**Vulnerability:** Authorization and Rate Limit bypasses via `Contains()` path matching, and missing environment checks for development endpoints.
**Learning:** Using substring matching (`Contains`) for path exclusions allows attackers to bypass security middleware by appending strings to sensitive endpoints (e.g., `/api/admin?fake=/swagger`). Additionally, comments claiming endpoints are "for development" must be backed by explicit `IsDevelopment()` checks.
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) for URL path exclusions. Inject and enforce `IWebHostEnvironment.IsDevelopment()` before allowing bypasses intended for development.
