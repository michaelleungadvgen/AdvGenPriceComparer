## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-04-09 - Insecure Auto-Update Mechanism
**Vulnerability:** The application downloaded and executed MSI installers without verifying their integrity, and executed them using `UseShellExecute = true`, making it susceptible to MITM attacks and arbitrary code execution.
**Learning:** Application updates must always verify the cryptographic hash (e.g., SHA-256) of downloaded binaries before execution. Using `UseShellExecute = true` for installers is less secure; explicit execution via `msiexec.exe /i` with `UseShellExecute = false` prevents arbitrary shell execution.
**Prevention:** Always validate installer hashes against a trusted manifest. Reject updates if the hash is missing or mismatched. Use `UseShellExecute = false` and explicit executable paths when launching processes.
