## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-04-06 - Raw Exception Leakage in API Responses
**Vulnerability:** Raw exception messages (`ex.Message`) were being exposed to clients in API error responses (e.g., `PricesController.UploadData` and `PriceDataService.UploadDataAsync`).
**Learning:** Exposing raw exception details can leak sensitive internal system information, such as database schema details, file paths, or third-party service configurations, which can be leveraged by attackers.
**Prevention:** Catch exceptions globally or in controllers, log the full exception details securely on the server side using a logging framework, and return safe, generic error messages (e.g., "An internal server error occurred.") to the client.
