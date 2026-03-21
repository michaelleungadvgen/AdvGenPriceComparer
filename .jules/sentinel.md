## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Unverified Auto-Update Installer Execution
**Vulnerability:** The `UpdateService` downloaded and executed MSI/EXE installers directly from a remote URL without cryptographically verifying the file's integrity against an expected hash, exposing the application to supply chain attacks and man-in-the-middle tampering.
**Learning:** Even if the update manifest is retrieved over HTTPS, relying solely on network transport security is insufficient. The downloaded binary artifact itself must be explicitly verified before being passed to `Process.Start()`.
**Prevention:** Always implement explicit cryptographic verification (e.g., SHA-256) of downloaded executables against a trusted manifest hash before execution.
