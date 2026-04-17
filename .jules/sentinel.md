## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-05 - Avoid Exposing Raw Exception Messages
**Vulnerability:** The application was exposing raw exception messages directly to clients (`ex.Message`) in API controllers (e.g. `PricesController.cs`).
**Learning:** Returning detailed error messages (such as exception stack traces or raw messages) to clients can inadvertently leak sensitive system internals, database constraint information, or internal file paths to attackers, which could aid in further attacks.
**Prevention:** Catch generic exceptions and return generic, safe error messages to the client (e.g. "An internal error occurred while processing the request"). Log the detailed exception message securely on the server-side instead.
