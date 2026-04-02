## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-18 - Insecure File Execution in Auto-Update Mechanism
**Vulnerability:** The auto-update mechanism (`UpdateService`) downloaded installer files directly from an external URL and executed them blindly using `Process.Start()` without any cryptographic verification, creating a severe risk of supply chain attacks or Man-in-the-Middle (MITM) tampering.
**Learning:** Blindly executing downloaded binaries is a critical security vulnerability. An attacker who compromises the update server or intercepts the download could replace the installer with a malicious payload that would be automatically executed with the user's privileges.
**Prevention:** Downloaded updates must always be cryptographically verified using strong hashing algorithms (like SHA-256) against a known-good hash before execution.
