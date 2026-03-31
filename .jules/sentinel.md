## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Auto-Updater
**Vulnerability:** The application's auto-updater downloaded an `.msi` or `.exe` over HTTP/HTTPS and passed it directly to `Process.Start` without any integrity validation, leading to a critical Remote Code Execution (RCE) vulnerability if the update payload or manifest is intercepted or compromised.
**Learning:** Developers often rely on TLS (HTTPS) for security, neglecting that DNS poisoning, malicious mirrors, or compromised upstream servers can still deliver malware. `Process.Start` on unverified binaries is extremely dangerous.
**Prevention:** Always cryptographically verify downloaded installers before execution. Require an explicit `FileHash` (e.g., SHA-256) in the remote update manifest, fail securely if the hash is missing or empty, and compare the computed hash of the downloaded payload against the manifest hash using a constant-time or ordinal ignore-case comparison before allowing the process to start.
