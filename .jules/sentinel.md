## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Unverified Executable Downloads
**Vulnerability:** Downloaded update installers (MSI files) were being executed blindly via `Process.Start` without any file integrity checks.
**Learning:** Even if the update manifest is downloaded over HTTPS, downloading and executing an unverified executable introduces severe risk. If the DNS is hijacked, or the update server/CDN is compromised, malicious payloads could be silently distributed and automatically run by users.
**Prevention:** Always verify the cryptographic hash (e.g., SHA-256) of a downloaded executable against a known-good value provided in a trusted manifest before triggering execution. Ensure validation fails securely if the manifest hash is missing.
