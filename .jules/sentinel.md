## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Missing File Hash Verification on Auto-Update
**Vulnerability:** The application's auto-update mechanism (`UpdateService`) downloaded updates and executed them via `Process.Start` without cryptographically verifying the downloaded file against the `FileHash` provided in the update manifest.
**Learning:** Checking the version numbers and simply downloading a file is not enough; an attacker could use a Man-in-the-Middle (MITM) attack or compromise the update server to serve malicious installers. The downloaded artifact must be explicitly verified before execution.
**Prevention:** Always cryptographically verify (e.g., using SHA-256) any downloaded executable or installer against a trusted manifest or signature before launching it. Fail securely if the hash is missing or mismatched.
