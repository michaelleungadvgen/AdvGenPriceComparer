## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-20 - Unverified Execution of Downloaded Installers
**Vulnerability:** The `UpdateService` downloaded application updates and executed them directly via `Process.Start` without any integrity checking, opening the application to supply chain or Man-in-the-Middle attacks if the download link was compromised.
**Learning:** Automatically downloaded files must never be blindly executed. The presence of a `FileHash` property in the update manifest indicates an intention for verification that was dangerously left unimplemented.
**Prevention:** Always verify downloaded installer files against a known, trusted cryptographic hash (like SHA-256) before execution to ensure the file has not been tampered with or replaced with malware.
