## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-14 - Prevent TOCTOU in Update Hash Verification
**Vulnerability:** The application was downloading update executables and executing them without verifying their integrity, and susceptible to Time-of-Check to Time-of-Use (TOCTOU) if the hash was verified after writing to disk.
**Learning:** Hash verification of downloaded updates must occur in memory on the byte array before writing the file to disk to prevent malicious tampering.
**Prevention:** Always verify hashes in-memory directly on the downloaded `byte[]` payload prior to persisting to the filesystem.
