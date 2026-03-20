## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Execution of Auto-Updates
**Vulnerability:** The auto-update mechanism downloaded an MSI installer and blindly executed it via `Process.Start()` without verifying its cryptographic hash, creating a severe vector for supply chain attacks or Man-in-the-Middle (MITM) tampering.
**Learning:** Blindly trusting downloaded executables or relying solely on HTTPS is insufficient. A compromised update server or MITM attack could serve a malicious payload that the application would then execute with user privileges.
**Prevention:** Always cryptographically verify downloaded update artifacts before execution. Require an external manifest (e.g., `UpdateCheckResult`) to provide a pre-computed SHA-256 hash (`FileHash`), compute the hash of the downloaded payload, and strictly enforce a match before invoking `Process.Start()`.
