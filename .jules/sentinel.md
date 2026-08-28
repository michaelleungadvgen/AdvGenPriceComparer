## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Exception Message Leakage in API Endpoints
**Vulnerability:** Raw exception messages (`ex.Message`) were returned directly to API clients in the `UploadData` endpoint of `PricesController` and `PriceDataService`.
**Learning:** Returning `ex.Message` in HTTP responses is a common anti-pattern that can inadvertently expose sensitive internal details (like file paths, database constraints, or connection issues) to end users.
**Prevention:** Always catch exceptions, log the detailed error securely on the server-side, and return a sanitized, generic error message to the client.
