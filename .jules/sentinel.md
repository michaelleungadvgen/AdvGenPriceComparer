## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Information Leakage in API Responses
**Vulnerability:** Raw exception messages (`ex.Message`) were being exposed to clients in error responses from API controllers (e.g., `PricesController`).
**Learning:** Exposing raw exceptions can leak sensitive internal server details, such as file paths, database structures, or third-party API keys, to unauthorized users.
**Prevention:** Always catch generic exceptions and return safe, generic error messages to the client. Detailed exceptions should only be logged securely on the server side using the application logger.
