## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-18 - Missing Hash Verification on Update Installer
**Vulnerability:** The application downloaded and executed installer files (`.msi`, `.exe`) based on remote URLs without verifying their cryptographic integrity, making it susceptible to man-in-the-middle attacks or compromised update servers.
**Learning:** Relying solely on HTTPS is insufficient for software updates; payloads must be cryptographically verified against known hashes to ensure authenticity before writing to disk and execution.
**Prevention:** Implement cryptographic hashing (e.g., SHA-256) on the downloaded byte array in memory *before* saving it to disk, and compare it against the expected hash provided in the update manifest.
