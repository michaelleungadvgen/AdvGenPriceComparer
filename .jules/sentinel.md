## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-29 - Unverified Update Execution
**Vulnerability:** The application's auto-update mechanism (`UpdateService`) was downloading MSI installers directly to a temporary directory and executing them via `Process.Start()` without any cryptographic validation of the downloaded payload.
**Learning:** Automatically executing downloaded binaries without verification opens the application to Man-in-the-Middle (MitM) attacks or compromised update servers. An attacker could replace the legitimate installer with a malicious payload, leading to remote code execution (RCE) with the privileges of the running application.
**Prevention:** Always mandate cryptographic verification (e.g., SHA-256) of downloaded updates against a trusted manifest before execution. Fail securely: if the hash is missing or mismatched, the update must be rejected.
