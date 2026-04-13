## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-20 - Unverified Update Downloads
**Vulnerability:** The auto-update mechanism downloaded and executed installers without verifying their cryptographic hash, making it susceptible to Man-in-the-Middle (MitM) attacks or compromised hosting servers.
**Learning:** Auto-updaters are a prime vector for supply chain attacks. Any executable downloaded from the internet must be verified against a known, trusted hash before execution. Furthermore, executing MSI/EXE files should be done directly without invoking the shell to prevent arbitrary command execution vulnerabilities.
**Prevention:** Always verify downloaded files using cryptographic hashing (e.g., SHA-256) directly on the in-memory byte array before saving to disk to prevent TOCTOU vulnerabilities. Explicitly invoke tools like `msiexec.exe` for MSI installers.
