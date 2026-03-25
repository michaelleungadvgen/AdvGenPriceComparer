## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Information Disclosure via Raw Exception Messages
**Vulnerability:** Raw exception messages (`ex.Message`) were being returned directly to clients in `BadRequest` responses during API operations in `PricesController` and `PriceDataService`.
**Learning:** Returning raw system exceptions across API boundaries can leak sensitive internal details (like file paths, database schemas, or third-party service configurations), violating the "fail securely" principle.
**Prevention:** Catch specific exceptions if needed, but always return safe, generic error messages to the client (e.g., "An unexpected error occurred"). Ensure detailed exceptions are only logged server-side for debugging.
