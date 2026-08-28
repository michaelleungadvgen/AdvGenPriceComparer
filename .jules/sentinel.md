## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Information Disclosure via Exception Messages in API Responses
**Vulnerability:** The API returned raw exception messages (`ex.Message`) to clients when an internal error occurred during data uploads.
**Learning:** Returning raw exception messages can inadvertently expose sensitive internal system details, such as database query structures, file paths, or third-party dependency specifics, which attackers can leverage.
**Prevention:** Always catch generic exceptions and return safe, standardized error messages to the client. Ensure detailed exceptions are only logged securely on the server side using the application logger.
