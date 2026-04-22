## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-15 - Unverified Executable Downloads
**Vulnerability:** The auto-update mechanism (`UpdateService.cs`) downloaded and executed installers without verifying their cryptographic hash against the update manifest, risking the execution of modified or malicious binaries via MitM attacks or compromised servers (TOCTOU vulnerability).
**Learning:** Always compute the cryptographic hash of downloaded update binaries directly in memory before saving them to disk or executing them. Validating the hash ensures the integrity and authenticity of the payload.
**Prevention:** Implement strict hash verification (e.g., SHA-256) for all downloaded executables, gracefully handling prefix variations (like `sha256:`) and empty fallbacks to prevent breaking updates when hashes are unavailable.
