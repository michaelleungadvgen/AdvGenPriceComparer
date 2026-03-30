## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Unverified Executable Downloads in Auto-Update Mechanism
**Vulnerability:** The application's auto-update service downloaded MSI installer files from a remote URL provided in an update manifest and executed them blindly, without verifying their cryptographic integrity or authenticity.
**Learning:** Blindly trusting network responses to download and execute installers, even over HTTPS, creates a massive attack surface. A compromised update server, or a MITM attack, could provide a malicious executable and easily achieve remote code execution (RCE) on client machines with high privileges.
**Prevention:** Always verify the integrity of executable downloads before executing them using strong cryptographic hashes (like SHA-256) checked against a trusted source or manifest. Additionally, the mechanism must fail securely: if the hash is missing or mismatched, the update must be aborted.
