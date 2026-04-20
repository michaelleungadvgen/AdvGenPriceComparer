## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Missing Installer Verification in Update Service
**Vulnerability:** The `UpdateService` downloaded a new application installer (`.msi`) to a temporary path and immediately executed it using `Process.Start` without any cryptographic verification. This created a critical supply-chain/MITM vulnerability where a compromised update server or intercepted network request could lead to arbitrary code execution.
**Learning:** Even if an update manifest is fetched over HTTPS, the downloaded binary itself must be cryptographically validated before execution to protect against compromised distribution channels or file tampering.
**Prevention:** Always require and validate cryptographic hashes (e.g., SHA-256) of downloaded executables against a trusted manifest before invoking `Process.Start`. Fail securely (delete the file and abort) if the hash does not match or is missing.
