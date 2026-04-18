## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Missing Integrity Check on Auto-Update Installer
**Vulnerability:** The application's auto-updater downloaded executable installers directly to a temporary directory and executed them without performing any cryptographic hash validation, opening a critical Time-of-Check to Time-of-Use (TOCTOU) vulnerability where an MITM attacker could substitute a malicious binary.
**Learning:** Even when pulling update manifests via HTTPS, binary payloads can be tampered with or redirected. Verifying checksums prevents execution of compromised payloads.
**Prevention:** Always compute a cryptographic hash (e.g., SHA-256) of the downloaded byte array in-memory against a trusted manifest hash before writing to disk and executing.
