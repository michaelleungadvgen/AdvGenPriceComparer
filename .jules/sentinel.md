## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-18 - Unverified Auto-Update Downloads
**Vulnerability:** The application was downloading update payloads (MSI installers) over HTTPS and immediately executing them without verifying their integrity against the cryptographically signed hash provided in the update manifest.
**Learning:** Even if the update manifest is retrieved securely, an unverified download is vulnerable to Man-in-the-Middle (MitM) attacks or server-side compromise, leading to remote code execution (RCE) via malicious updates. Time-of-Check to Time-of-Use (TOCTOU) must also be avoided.
**Prevention:** Always compute a cryptographic hash (e.g., SHA-256) of the downloaded byte array in memory *before* writing it to disk or executing it. Validate this computed hash against a trusted hash provided in the signed update manifest.
