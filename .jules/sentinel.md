## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Update Execution Vulnerability
**Vulnerability:** The `UpdateService` downloaded and executed MSI installers without verifying their cryptographic integrity, making the application highly susceptible to supply chain attacks or Man-in-the-Middle (MITM) tampering.
**Learning:** Automatically starting external executables or installers sourced from HTTP downloads is inherently risky. Even if the update manifest is downloaded over HTTPS, the downloaded binary must be verified against an expected hash to prevent malicious replacements.
**Prevention:** Always perform a cryptographic hash verification (e.g., SHA-256) on downloaded binaries against a trusted manifest before invoking `Process.Start()`.
