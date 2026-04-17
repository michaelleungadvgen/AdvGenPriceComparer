## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Overly Permissive CORS Configuration
**Vulnerability:** The SignalR CORS policy was configured with `AllowAnyOrigin()`, which allows any website to make cross-origin requests to the server, potentially leading to CSRF or unauthorized access if not properly secured.
**Learning:** `AllowAnyOrigin()` is a significant security risk, especially when combined with APIs that manage sensitive data or require credentials. It is mutually exclusive with `AllowCredentials()` in ASP.NET Core.
**Prevention:** Always use `WithOrigins()` to specify explicit, trusted origins in production environments. Configure allowed origins via appsettings.json to allow flexibility across environments without compromising the default secure posture.
