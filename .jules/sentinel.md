## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Exception Message Leakage via API Responses
**Vulnerability:** Raw exception messages (`ex.Message`) were returned directly in HTTP API responses (e.g., `UploadResult.ErrorMessage`) when errors occurred during data processing.
**Learning:** Returning raw exceptions can expose sensitive internal information such as stack traces, database schemas, or filesystem paths to potentially malicious actors, giving them insights into backend infrastructure.
**Prevention:** Catch generic exceptions and return opaque, safe error messages to the client (e.g., "An internal error occurred"). Log the detailed exception securely on the server side (e.g., via `ILogger` or an internal database record) for debugging purposes.
