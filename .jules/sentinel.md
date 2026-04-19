## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-18 - Overly Permissive CORS Policy
**Vulnerability:** The SignalR setup in `AdvGenPriceComparer.Server` used `.AllowAnyOrigin()`, which accepts connections from any domain, along with WebSockets making it vulnerable to CSWSH and other cross-origin data exposure.
**Learning:** SignalR endpoints shouldn't allow any origin while accepting credentials (although mutually exclusive usually, .AllowCredentials() could be added, restricting `AllowAnyOrigin`). A static list of explicitly trusted origins from configuration prevents unwanted cross-origin clients.
**Prevention:** Always restrict CORS policies to explicitly allowed domains by loading them from configuration rather than using `.AllowAnyOrigin()`, and only pair it with `.AllowCredentials()` when explicitly listed origins are provided.
