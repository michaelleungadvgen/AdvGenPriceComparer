## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Insecure Update Download and TOCTOU Vulnerability
**Vulnerability:** The auto-update mechanism downloaded the installer binary and executed it without verifying its integrity against the `FileHash` provided in the update manifest, and wrote the file to disk before performing any validation, risking a Time-of-Check to Time-of-Use (TOCTOU) vulnerability.
**Learning:** In auto-update systems, relying solely on HTTPS is insufficient if the server itself or the CDN is compromised. Furthermore, writing an unverified file to disk and then hashing the disk file allows another process to tamper with the file between the check and execution.
**Prevention:** Always cryptographically verify downloaded executable binaries against a trusted manifest hash. To prevent TOCTOU attacks, compute the cryptographic hash directly on the downloaded byte array in memory *before* writing it to the local file system.
