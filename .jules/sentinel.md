## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-05 - Exception Information Leakage in API Responses
**Vulnerability:** The API endpoint `UploadData` and its underlying service were returning raw exception messages (`ex.Message`) directly to clients in the JSON response when an error occurred.
**Learning:** Returning raw exception messages is a form of information leakage that can expose sensitive internal server details, database structure, or execution context to potential attackers, violating the fail securely principle.
**Prevention:** Catch generic exceptions and return sanitized, generic error messages to clients (e.g., "An internal error occurred"). Always separate client-facing errors from internal server logs or database audit records, which should retain the full exception details for debugging purposes.
