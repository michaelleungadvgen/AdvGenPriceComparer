## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-05-18 - Unverified Auto-Update Installer Execution
**Vulnerability:** The auto-update mechanism downloaded the installer binary and executed it with `UseShellExecute = true` without verifying the integrity of the downloaded file.
**Learning:** This Time-of-Check to Time-of-Use (TOCTOU) vulnerability can allow a man-in-the-middle or malicious host to provide a compromised installer that gets blindly executed.
**Prevention:** Downloaded binaries must be verified using a cryptographic hash against an expected hash from a trusted source before saving to disk and executing. In addition, execution of the binary should be explicitly parameterized (e.g., calling msiexec) with `UseShellExecute = false`.
