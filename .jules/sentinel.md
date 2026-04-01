## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-04-01 - Critical Security Issue: Missing Hash Validation on Installer Download
**Vulnerability:** The auto-update mechanism (`UpdateService.DownloadUpdateAsync`) was downloading MSI installers without verifying their cryptographic signatures against the manifest's `FileHash` before executing them (`Process.Start`).
**Learning:** This exposes the application to Man-in-the-Middle (MITM) attacks and allows attackers to substitute malicious payloads for legitimate updates. Even with HTTPS, host compromise or intercept could result in Remote Code Execution (RCE) as the installer was blindly trusted.
**Prevention:** Auto-update mechanisms must always cryptographically verify downloaded artifacts using a strong hash (e.g., SHA-256) defined in a secure manifest prior to execution. If validation fails securely (e.g., missing hash, incorrect hash), the update must be rejected.
