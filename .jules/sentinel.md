## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2023-10-27 - Information Leakage in API Error Responses
**Vulnerability:** The API endpoint `UploadData` and its underlying service `PriceDataService` were returning raw `Exception.Message` strings in the `UploadResult` and storing them in the database. This could expose internal system details, stack traces, database schema information, or other sensitive data to potential attackers if an unexpected error occurs during processing.
**Learning:** Returning unhandled exception details directly to API clients violates the principle of "fail securely". It allows malicious actors to probe the system and potentially gain insights into its architecture, dependencies, and internal state.
**Prevention:** Always catch exceptions at the API boundary, log the detailed exception securely on the server side using `ILogger`, and return a generic, non-revealing error message to the client (e.g., "An internal server error occurred."). Use structured error responses to provide actionable (but safe) feedback.
