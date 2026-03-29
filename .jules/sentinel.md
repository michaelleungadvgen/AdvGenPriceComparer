## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Auto-Updater
**Vulnerability:** The auto-updater implementation (`UpdateService`) was downloading and executing MSI update installer files without validating their integrity or authenticity.
**Learning:** Automatically starting external processes (`Process.Start()`) on downloaded executables over the internet (even over HTTPS) exposes users to severe supply chain attacks if the server or transit is compromised.
**Prevention:** Always cryptographically verify the integrity and source of downloaded executables before execution. Utilize the `FileHash` property (SHA-256) provided in the update manifest and compare it against the computed hash of the downloaded payload.
