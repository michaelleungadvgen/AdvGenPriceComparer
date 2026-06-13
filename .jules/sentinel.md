## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Insecure Path Matching in Middleware Causes Authentication/Rate Limit Bypass
**Vulnerability:** The application used insecure substring matching (`Contains()`) for authentication and rate limit exemptions (e.g., bypassing for any path containing `/swagger` or `/health`). It also missed environment checks for development-only endpoint exemptions (`/api/prices`).
**Learning:** Substring matching for URL routing allows trivial bypasses (e.g., `/api/admin?fake=/swagger` or `/api/prices-fake`). Additionally, development-only access rules must be explicitly gated by environment checks to prevent leakage in production.
**Prevention:** Always use exact matching (`==`) or strict prefix matching bounded by a directory separator (`StartsWith("/path/")`) when exempting routes from security controls. Always enforce `IWebHostEnvironment.IsDevelopment()` checks for development-only bypasses.
