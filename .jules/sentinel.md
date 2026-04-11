## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - Cryptographic Verification and Secure Execution for Auto-Updates
**Vulnerability:** Auto-update mechanism executed downloaded installers without verifying cryptographic signatures/hashes and used UseShellExecute = true, opening the door to Man-in-the-Middle (MitM) and Time-of-Check to Time-of-Use (TOCTOU) attacks.
**Learning:** Checking hashes is necessary to ensure update integrity. TOCTOU vulnerabilities can occur if file is verified after writing to disk. UseShellExecute=true delegates to the shell which can introduce unexpected execution paths.
**Prevention:** Always verify downloaded file hashes (like SHA-256) directly on the byte array in memory before writing to disk. Use UseShellExecute=false and specifically invoke the expected executable (e.g., msiexec.exe for .msi) to maintain execution boundaries.
