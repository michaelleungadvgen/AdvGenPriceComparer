## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Downloaded Installer Execution
**Vulnerability:** The application update mechanism (`UpdateService.cs`) downloaded application installers over HTTP/HTTPS and executed them immediately without cryptographic verification of the file contents.
**Learning:** Any mechanism that downloads and automatically executes binaries is a prime target for supply chain attacks. Even if the download URL is HTTPS, attackers could compromise the host, use DNS spoofing, or perform a man-in-the-middle attack if TLS validation is bypassed or weak.
**Prevention:** Always cryptographically verify downloaded executable payloads against a known, trusted hash (e.g., SHA-256) before writing them to disk or executing them. `Process.Start()` should only be called if the hash perfectly matches the expected value from a trusted update manifest.
