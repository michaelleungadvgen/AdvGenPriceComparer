## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Missing Cryptographic Verification of Auto-Updates
**Vulnerability:** The application's auto-update mechanism (`UpdateService.cs`) downloaded and blindly executed installer files via `Process.Start()` using only a URL, making it highly susceptible to supply chain attacks or Man-in-the-Middle (MITM) tampering.
**Learning:** Any auto-update mechanism that downloads and executes files must cryptographically verify the integrity of the payload before execution. Simply fetching from a known URL is insufficient defense.
**Prevention:** Downloaded updates must always be cryptographically verified using a strong hashing algorithm (like SHA-256) against a trusted `FileHash` property in the update manifest before calling `Process.Start()`.
