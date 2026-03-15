## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Auto-Updater
**Vulnerability:** The application's auto-updater (`UpdateService.cs`) was downloading MSI installers over HTTPS and passing them directly to `Process.Start()` without any cryptographic verification, allowing for a potential supply chain attack if the update server or DNS were compromised.
**Learning:** TLS alone guarantees transport security, but it does not authenticate the integrity or origin of the downloaded file payload. An attacker who breaches the update server could serve a malicious MSI.
**Prevention:** Always verify downloaded executables against a pre-calculated, securely-fetched cryptographic hash (like SHA-256) before invoking them.
