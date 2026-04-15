## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-04-15 - Missing Cryptographic Verification on Downloaded Updates
**Vulnerability:** Update mechanism downloaded and executed executable files without cryptographic verification.
**Learning:** In auto-updater scenarios, missing cryptographic verification creates a severe Time-of-Check to Time-of-Use (TOCTOU) vulnerability where man-in-the-middle attacks can replace downloaded executables.
**Prevention:** Compute and verify cryptographic hashes (like SHA-256) entirely in memory directly on downloaded byte arrays *before* persisting the bytes to the local filesystem for execution.
