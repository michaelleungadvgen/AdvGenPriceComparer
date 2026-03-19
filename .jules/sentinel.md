## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Insecure Update Execution without Hash Verification
**Vulnerability:** The auto-update mechanism (`UpdateService`) downloaded an executable update installer (e.g. MSI/EXE) over the network and executed it directly without verifying its signature or cryptographic hash, exposing the application to potential supply-chain and man-in-the-middle attacks.
**Learning:** Any mechanism that downloads executable code and runs it must cryptographically verify that the file has not been tampered with. The presence of an HTTPS connection is insufficient protection against compromised update servers or CDN caching issues.
**Prevention:** Always verify downloaded executable files against a known-good cryptographic hash (such as SHA-256) provided in a secure manifest before saving them to disk or executing them. Abort the execution immediately if the hash verification fails.
