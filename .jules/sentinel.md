## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Code Execution via Update Service
**Vulnerability:** The `UpdateService` downloaded and executed MSI/EXE installers based purely on a URL from a remotely fetched manifest. It called `Process.Start()` on the downloaded file without any cryptographic verification of the file's contents against an expected hash.
**Learning:** Even if the update manifest is retrieved over HTTPS, a compromised server or a man-in-the-middle attack (if cert validation is bypassed or a CA is compromised) could provide a malicious download URL, leading to arbitrary code execution (a critical supply chain vulnerability).
**Prevention:** Always cryptographically verify downloaded executable artifacts before running them. The update manifest must provide an expected hash (e.g., SHA-256), and the downloaded file's hash must be computed (`SHA256.HashData(data)`) and strictly compared to the expected hash before calling `Process.Start()`.
