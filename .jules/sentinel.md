## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Missing Hash Verification in Auto-Update Mechanism
**Vulnerability:** The auto-update mechanism downloaded an executable/MSI payload and executed it without verifying its integrity against the provided SHA-256 hash.
**Learning:** Even when pulling update manifests via HTTPS, omitting cryptographic signature or hash verification on the downloaded payload exposes the application to supply-chain attacks, leading to unauthenticated arbitrary code execution.
**Prevention:** Always cryptographically verify downloaded executable payloads in memory before writing to disk and executing. Fallbacks should exist for graceful failures but hashes must be enforced when present.
