## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-05 - Information Leakage via Exception Messages in API
**Vulnerability:** Raw exception messages (`ex.Message`) were being exposed to clients in API error responses in `PricesController` and `PriceDataService`.
**Learning:** Returning raw exception messages to users/clients can unintentionally expose sensitive internal information such as database schemas, internal paths, or stack components, giving attackers an insight into the application's architecture.
**Prevention:** Always return generic error messages to clients (e.g., "An internal error occurred"). Log the detailed exception (including the original `ex.Message`) securely on the server-side to aid debugging while keeping it hidden from end-users.
