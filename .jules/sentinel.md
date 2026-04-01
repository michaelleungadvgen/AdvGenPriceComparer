## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Downloaded Executable Payloads
**Vulnerability:** The application was downloading `.msi` application updates from a remote URL over HTTP/HTTPS and immediately executing them using `Process.Start()` without cryptographically validating the payload against the manifest file hash.
**Learning:** Even if the update manifest is downloaded over HTTPS, DNS hijacking, local proxy interception (MitM), or server-side compromise could allow an attacker to substitute a malicious executable for the application update.
**Prevention:** Always mandate cryptographic validation (e.g., SHA-256) of dynamically downloaded executable payloads before transferring control to them. Ensure the validation fails securely (i.e., reject the update if the manifest hash is missing, empty, or fails to match the downloaded data).
