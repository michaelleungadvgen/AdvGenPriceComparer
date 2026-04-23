## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Arbitrary Code Execution via Installer Updates
**Vulnerability:** Downloaded update installers were being directly executed using `Process.Start` without any cryptographic verification to ensure the file matched the expected update payload from the update manifest.
**Learning:** Directly trusting files downloaded from the internet for execution, even over HTTPS, opens the application up to severe supply chain attacks and MITM vulnerabilities where a malicious payload could completely compromise a user's machine with local privileges.
**Prevention:** Always enforce a cryptographic handshake when downloading executable files. Calculate a strong cryptographic hash (e.g., SHA-256) of the downloaded byte stream and verify it exactly matches the expected hash declared in a secure update manifest before writing it to disk or invoking `Process.Start`.
