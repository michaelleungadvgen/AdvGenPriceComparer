## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Unverified Auto-Update Execution (TOCTOU/RCE)
**Vulnerability:** The application downloaded and executed MSI/EXE update files based on an unauthenticated JSON feed over HTTP/HTTPS without verifying the file integrity, potentially leading to Remote Code Execution if the feed or download was compromised.
**Learning:** Auto-updaters are highly targeted attack vectors. Verifying file hashes or digital signatures is critical before executing any downloaded binaries. Checking hash before writing to disk prevents TOCTOU.
**Prevention:** Always cryptographically verify downloaded binaries against an expected hash (or signature) directly on the byte array in memory before saving to the file system or calling `Process.Start`.
