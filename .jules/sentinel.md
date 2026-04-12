## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-18 - Missing Integrity Validation on Updates
**Vulnerability:** The application downloaded an auto-update payload and executed it via `Process.Start(tempPath)` with `UseShellExecute = true` without performing any cryptographic hash validation, opening it up to man-in-the-middle code execution. Furthermore, it did not distinguish `msiexec` executions securely.
**Learning:** Checking hashes *after* writing the file opens a Time-of-Check to Time-of-Use (TOCTOU) vulnerability where the file can be swapped before execution. The `FileHash` often includes dynamic prefixes like `sha256:` that must be explicitly stripped before parsing, and executing `.msi` installers securely requires `msiexec.exe /i` with `UseShellExecute = false`.
**Prevention:** Always verify downloaded file bytes cryptographically *in-memory* against a known good hash from the manifest prior to flushing to disk, and always execute known binaries safely with explicit process arguments rather than relying on system shell handlers.
