## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Insecure File Download and Execution in Auto-Updater
**Vulnerability:** The application's auto-update mechanism (`UpdateService`) downloaded `.msi` or `.exe` installers from an external URL and immediately executed them via `Process.Start()` without any integrity verification. This makes the application highly vulnerable to man-in-the-middle (MITM) attacks (if the update JSON is spoofed or compromised) and supply chain attacks (if the hosted binary is replaced).
**Learning:** Any mechanism that downloads and executes code dynamically must treat the payload as untrusted. Simply relying on HTTPS for the download is insufficient because the source server itself might be compromised.
**Prevention:** Always cryptographically verify downloaded executables before calling `Process.Start()`. In this case, `SHA-256` hashing was added to verify the downloaded byte array against a known-good hash (`FileHash`) published in the update manifest.
