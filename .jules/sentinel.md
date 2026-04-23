## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Auto-Update Mechanism
**Vulnerability:** The application's auto-update mechanism (`UpdateService`) downloaded `.msi` installers from a remote URL and executed them immediately via `Process.Start()` without any cryptographic verification.
**Learning:** Even if the update manifest is downloaded over HTTPS, executing unverified binaries is extremely dangerous. It leaves the application vulnerable to Man-in-the-Middle (MITM) attacks if the server is compromised or DNS is spoofed, allowing attackers to deploy malware as an "update."
**Prevention:** Always cryptographically verify downloaded executables before running them. Use the `FileHash` (e.g., SHA-256) provided in the update manifest to validate the downloaded file. Crucially, this validation must "fail securely": if the hash is missing from the manifest or the file doesn't match, the update MUST be aborted.
