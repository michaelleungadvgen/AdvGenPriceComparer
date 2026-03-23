## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Information Exposure Through Error Message
**Vulnerability:** The API endpoints (e.g., `PricesController.UploadData` and `PriceDataService.UploadDataAsync`) were directly exposing raw `Exception.Message` strings to the client in HTTP responses when an error occurred.
**Learning:** Exposing raw exception details can inadvertently leak sensitive system information, database structures, internal application state, or third-party service details to potential attackers, violating the fail-securely principle.
**Prevention:** Always catch exceptions and return generic, safe error messages to the client (e.g., "An internal error occurred"). Log the detailed exception securely on the server-side for debugging purposes.
