## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Insecure Update Execution
**Vulnerability:** The application update mechanism (`UpdateService`) downloaded an executable/MSI file from a remote URL and immediately executed it using `Process.Start` without cryptographically verifying the file's integrity.
**Learning:** Automatically executing downloaded files based solely on an HTTP response is a critical supply chain risk. If the update server is compromised, attackers could serve a malicious payload which the application would blindly execute.
**Prevention:** Always require and validate cryptographic hashes (e.g., SHA-256) of downloaded update payloads against a trusted manifest before execution. Delete any file that fails verification.
