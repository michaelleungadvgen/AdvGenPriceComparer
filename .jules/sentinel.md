## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-04-14 - Unverified Execution of Downloaded Updates
**Vulnerability:** The application's auto-updater downloaded executable installers (`.msi`, `.exe`) and executed them blindly without validating their integrity via cryptographic hashes, leading to potential Remote Code Execution (RCE) if the download server was compromised or traffic was intercepted.
**Learning:** Relying purely on HTTPS for transport security is insufficient for software updates, as it does not protect against a compromised distribution server or CDN hosting malicious payloads.
**Prevention:** Always verify downloaded executables against known, trusted cryptographic signatures or hashes (e.g., SHA-256) directly in memory before writing to disk and executing them, to ensure both integrity and authenticity.
