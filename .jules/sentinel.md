## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-07 - Exception Message Leakage in API Responses
**Vulnerability:** The API controllers (`PricesController` and `PriceDataService`) were catching generic `Exception` objects and exposing their internal `ex.Message` directly in API responses (`UploadResult.ErrorMessage`).
**Learning:** Exposing raw exception details can leak sensitive information to users, such as database connection strings, file paths, or backend logic details. This information could be exploited by malicious actors.
**Prevention:** Always use generic error messages for client responses (e.g., "An internal error occurred"). Log the detailed exception information safely on the server side (e.g., using `ILogger`) and return safe HTTP error responses instead.
