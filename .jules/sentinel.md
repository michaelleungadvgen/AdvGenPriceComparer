## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-01 - Missing Signature Verification on Auto-Updates
**Vulnerability:** The auto-update mechanism in `UpdateService` downloaded executable updates (`.msi` or `.exe`) and executed them directly (`Process.Start()`) without verifying their integrity against the expected cryptographic hash. This exposed the application to potential supply chain or man-in-the-middle attacks where a malicious binary could be swapped for the legitimate update.
**Learning:** Even when using HTTPS, relying solely on transport encryption without validating the downloaded artifact's cryptographic hash can lead to a compromise if the update server is breached.
**Prevention:** Always verify the integrity of downloaded executables (e.g., using SHA-256) against a trusted, out-of-band manifest before launching them.
