## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-13 - Insecure Update Execution
**Vulnerability:** Downloaded `.msi` application updates were being executed using `Process.Start` without any cryptographic verification, making the update mechanism susceptible to Man-in-the-Middle (MITM) or supply chain attacks.
**Learning:** Even if the update metadata is served over HTTPS, the actual binary download could be intercepted or compromised. Blindly executing downloaded binaries is a significant security risk.
**Prevention:** Always verify the integrity of downloaded executables using a strong cryptographic hash (e.g., SHA-256) provided in the trusted update manifest before executing them.
