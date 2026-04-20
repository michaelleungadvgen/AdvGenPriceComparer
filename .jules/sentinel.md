## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-20 - Exception Message Information Disclosure via API
**Vulnerability:** Detailed internal exception messages (`ex.Message`) were being returned directly to clients in API error responses (e.g., `PricesController.UploadData` and `PriceDataService.UploadDataAsync`). This can leak sensitive internal implementation details, such as database schemas, file paths, or system configurations to an attacker.
**Learning:** Catching general exceptions and returning their messages directly to the client is a common anti-pattern. While it helps with debugging, it violates the principle of failing securely and can provide attackers with valuable reconnaissance information.
**Prevention:** Always log detailed exception information securely on the server-side using `ILogger` or equivalent, and return safe, generic error messages to the client (e.g., "An internal error occurred"). Provide correlation IDs if client debugging is necessary, without exposing the raw error string.
