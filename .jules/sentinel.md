## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Missing Cryptographic Verification on Downloaded Updates
**Vulnerability:** The application's `UpdateService` downloaded installer files (MSIs) over the network and executed them using `Process.Start()` without verifying the file's integrity or authenticity.
**Learning:** Automatically executing downloaded files exposes the application to supply chain or man-in-the-middle attacks where a malicious payload could be substituted. The update manifest already contained a `FileHash` that wasn't being used.
**Prevention:** Always verify downloaded executables using cryptographic hashes (like SHA-256) matching a trusted manifest before execution. Even when using HTTPS, this protects against compromised infrastructure hosting the updates.
