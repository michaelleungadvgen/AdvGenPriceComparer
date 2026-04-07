## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Unverified Auto-Update Execution
**Vulnerability:** The application auto-updater downloaded and executed MSI/EXE installers directly from a remote URL without verifying the integrity of the downloaded payload, allowing potential Man-In-The-Middle (MITM) attacks or compromised distribution servers to execute arbitrary code.
**Learning:** Even if update manifests are loaded over HTTPS, relying solely on transport-layer security is insufficient for executing downloaded binaries. Attackers compromising the distribution server could replace the binary while keeping the manifest intact.
**Prevention:** Always cryptographically verify downloaded updates using strong hashing algorithms (like SHA-256) against a trusted `FileHash` provided in the update manifest. Fail securely by rejecting updates if the manifest hash is missing or mismatched.
