## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-05-15 - Environment Security Bypass Vulnerability
**Vulnerability:** The `ApiKeyMiddleware` in the ASP.NET Core server project bypassed authentication for `GET /api/prices` endpoints, which was intended only for local development, but omitted the environment check. This resulted in unauthenticated access to the endpoint in production.
**Learning:** Hardcoded path conditions or HTTP method checks without an explicit environment validation (`IWebHostEnvironment.IsDevelopment()`) in middleware lead to unintended authorization bypasses.
**Prevention:** Environment-specific security bypasses (like for local development) in middleware must explicitly use `IWebHostEnvironment.IsDevelopment()` rather than relying on assumed environments or missing checks.
