## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Installer Execution
**Vulnerability:** The application was downloading and executing an installer (`.msi` or `.exe`) directly from a web URL during the update process without verifying its cryptographic signature or checksum (`FileHash`). This exposed the application to remote code execution via Man-in-the-Middle (MitM) attacks or compromised update servers.
**Learning:** Checking the version and downloading the file is not enough. Without validating the downloaded artifact against a trusted checksum manifest, any attacker controlling the download stream can easily replace the installer with malware.
**Prevention:** Always cryptographically verify downloaded update installers (e.g., using SHA-256) against a trusted `FileHash` before calling `Process.Start()`. Ensure the verification fails securely by rejecting updates if the hash is missing or empty.
