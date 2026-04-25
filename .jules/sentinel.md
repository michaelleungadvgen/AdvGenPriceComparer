## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-10-25 - Missing Cryptographic Hash Verification on Auto-Updates
**Vulnerability:** The auto-update mechanism downloaded and executed MSI installers without verifying their cryptographic signatures, allowing potential execution of tampered payloads.
**Learning:** Checking auto-update file hashes must be performed in memory before writing to disk to prevent Time-of-Check to Time-of-Use (TOCTOU) exploits where the file could be replaced between validation and execution.
**Prevention:** Always compute hashes on the downloaded byte arrays in memory and verify them against expected values provided in the secure update manifest before writing to the local filesystem.
