## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-06 - Unverified Installer Execution in Auto-Update
**Vulnerability:** The WPF application's `UpdateService` downloaded updates and passed them to `Process.Start` without any cryptographic verification of the file. This allowed a Man-in-the-Middle (MitM) attacker or compromised update server to execute arbitrary code with the application's privileges by serving a malicious payload.
**Learning:** Even when using HTTPS, update manifests and installer binaries can be spoofed or compromised. A defense-in-depth approach is critical for auto-update mechanisms.
**Prevention:** Always include cryptographic hashes (like SHA-256) in the update manifest and explicitly verify the hash of the downloaded binary against the manifest's hash before execution. Furthermore, validate that the hash exists in the manifest to fail securely if an empty manifest is provided.
