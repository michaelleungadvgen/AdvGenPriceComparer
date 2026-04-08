## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Unverified Downloaded File Execution
**Vulnerability:** The auto-update mechanism downloaded an MSI installer over the network and executed it using `Process.Start` without any cryptographic verification of the file's integrity.
**Learning:** Downloading and executing binaries without verification allows for Man-in-the-Middle (MitM) attacks or server compromises to distribute malware to end-users. Even if downloaded over HTTPS, the server itself could be compromised.
**Prevention:** Always cryptographically verify downloaded installers using a strong hash algorithm (e.g., SHA-256) against a known-good hash before execution. Also, use `UseShellExecute = false` and explicitly invoke the installer executable (e.g., `msiexec.exe` for MSIs) rather than relying on shell associations.
