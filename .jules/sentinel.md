## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-04-12 - Insecure Auto-Update Execution and Verification Bypass
**Vulnerability:** The auto-update mechanism lacked cryptographic verification of downloaded binaries and launched them using insecure `UseShellExecute = true`, opening vectors for arbitrary code execution if the update manifest or download channel were compromised (e.g. DNS spoofing or MITM).
**Learning:** Checking auto-update mechanisms requires holistic analysis. TOCTOU (Time-of-Check to Time-of-Use) attacks can exploit delays between writing files and checking signatures. Furthermore, `UseShellExecute = true` can unintentionally execute a different handler based on Windows file associations or environment variables.
**Prevention:** Download files to byte arrays in memory, compute cryptographic hashes (SHA-256) on the byte array to prevent TOCTOU vulnerabilities, and then write to disk. Always execute updates with `UseShellExecute = false` and specify the explicit executable (e.g., `msiexec.exe` for MSI) to ensure deterministic execution.
