## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-17 - Unverified Execution of Auto-Updates
**Vulnerability:** The application's `UpdateService` automatically downloaded MSI installers and executed them via `Process.Start()` without verifying the file's integrity or authenticity, exposing users to severe supply chain attacks.
**Learning:** Any auto-update mechanism that blindly trusts external URLs, even over HTTPS, risks executing malicious payloads if the CDN or domain is compromised.
**Prevention:** Always compute a strong cryptographic hash (e.g., SHA-256) of downloaded update binaries and verify it against a trusted, securely delivered manifest (e.g., `updateResult.FileHash`) *before* writing the payload to disk or executing it.
