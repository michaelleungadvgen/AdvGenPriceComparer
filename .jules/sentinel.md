## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-05 - Unverified Executable Downloads in Auto-Update
**Vulnerability:** The WPF auto-update mechanism downloaded executables (MSI/EXE) via HTTP(S) and immediately executed them via `Process.Start` without cryptographically verifying their integrity, exposing users to RCE risks via compromised servers or hijacked URLs.
**Learning:** Even when using HTTPS and having file hashes available in an update manifest, failing to enforce hash verification before writing the file to disk or executing it negates those security controls.
**Prevention:** Always verify downloaded executables against a trusted hash (e.g., SHA-256) in memory before saving to the file system and executing. Handle hash mismatches by securely failing the operation.
