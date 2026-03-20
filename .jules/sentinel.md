## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Update Service
**Vulnerability:** The `UpdateService` downloaded application updates (.msi files) and executed them immediately via `Process.Start()` without any integrity verification. This creates a severe supply chain vulnerability if the update server or DNS is compromised.
**Learning:** Automatically running downloaded executables without verification is a critical security risk. Relying solely on HTTPS is insufficient if the hosting provider or update manifest itself is altered.
**Prevention:** Always cryptographically verify downloaded update installers before execution. Implement a SHA-256 hash check comparing the downloaded file's hash against a known, trusted hash provided in the update manifest (e.g., `UpdateCheckResult.FileHash`). Abort execution and delete the file if validation fails.
