## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-09 - File verification in auto-updater
**Vulnerability:** The auto-update mechanism downloaded and executed binaries without verifying their integrity, and was vulnerable to Time-of-Check to Time-of-Use (TOCTOU) if the hash was computed after writing to disk.
**Learning:** Cryptographic hashes must be checked against the byte array in memory *before* persisting to the local filesystem to prevent malicious replacement between check and execution. Also, the expected file hash often contains prefixes (e.g. `sha256:`) that must be stripped before comparing.
**Prevention:** Always require full update manifests (like `UpdateCheckResult`) instead of just a URL when downloading, and compute hashes on the in-memory array before writing to `Path.GetTempPath()`.
