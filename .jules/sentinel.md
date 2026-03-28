## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Unverified Executable Downloads
**Vulnerability:** The application was downloading update executables (`.msi`, `.exe`) via `HttpClient` and immediately calling `Process.Start()` to execute them without any cryptographic verification.
**Learning:** If an application silently executes downloaded files based only on a URL, a Man-in-the-Middle (MITM) attack or a compromised backend server could trick the application into executing a malicious payload, leading to full system compromise.
**Prevention:** Always cryptographically verify downloaded executables before execution. Include a file hash (like SHA-256) in the securely retrieved update manifest, and compute the hash of the downloaded byte array (`using var sha256 = System.Security.Cryptography.SHA256.Create()`) to ensure it perfectly matches the expected hash before calling `Process.Start()`. Reject the update securely if the hash is missing or incorrect.
