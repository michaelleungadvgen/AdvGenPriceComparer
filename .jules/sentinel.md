## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-18 - Execution of Unverified Binaries in Auto-Updater
**Vulnerability:** The application downloaded and immediately executed MSI/EXE files via `Process.Start` without cryptographically verifying the file's integrity against the provided manifest hash. This could allow execution of tampered binaries if the download server was compromised or traffic was intercepted.
**Learning:** Checking response status codes during downloads is insufficient for security; downloaded executables must always have their integrity validated before writing to disk.
**Prevention:** Compute the cryptographic hash (e.g., SHA-256) of the downloaded byte array in memory and verify it matches the expected hash from a trusted source before writing the bytes to the filesystem to prevent Time-of-Check to Time-of-Use (TOCTOU) vulnerabilities.
