## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Insecure Auto-Update Execution
**Vulnerability:** The application downloaded an `.msi` update installer from a remote URL via `UpdateService.DownloadUpdateAsync` and blindly executed it using `Process.Start()` without verifying the integrity or authenticity of the downloaded file.
**Learning:** Even if update metadata is fetched over HTTPS, failing to verify the cryptographic signature of the downloaded executable exposes the application to supply chain attacks or man-in-the-middle manipulation if the download source is compromised.
**Prevention:** Always verify downloaded executables against a known-good cryptographic hash (like SHA-256) provided securely in the update manifest before execution. If the hash does not match, abort execution and delete the tampered file.
