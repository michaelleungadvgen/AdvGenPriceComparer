## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Exception Information Leakage [RESOLVED]
**Vulnerability:** Exception details (`ex.Message`) were directly returned to API clients in error responses from controllers and services, potentially exposing internal system workings, database schemas, or file paths.
**Learning:** Catch blocks that package exceptions directly into HTTP responses or generic DTOs inadvertently bypass security boundaries.
**Prevention:** Always log the full exception (`ex`) server-side using a logging framework and return a generic, static error message to the client. Ensure all layers (services and controllers) sanitize their error responses.
