## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-03-05 - Missing Installer Signature Verification
**Vulnerability:** The auto-update mechanism downloaded and executed installers without cryptographically verifying their integrity, allowing for potential remote code execution.
**Learning:** Automatic updates are a prime target for supply chain attacks. Simply fetching an installer over HTTPS is insufficient if the manifest itself or the download server is compromised.
**Prevention:** Always verify downloaded executables using a strong cryptographic hash (e.g., SHA-256) provided in a secure manifest, and ensure validation fails securely (rejecting empty or missing hashes).
