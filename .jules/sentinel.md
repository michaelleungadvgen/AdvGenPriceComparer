## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Unrestricted Public Access to Price Endpoints in Production
**Vulnerability:** The `ApiKeyMiddleware` in the ASP.NET Core Web API allowed anonymous read access (GET requests) to the `/api/prices` endpoints in all environments. This exposed sensitive pricing data without any authentication check.
**Learning:** Hardcoded environment-specific security bypasses can easily leak into production if not explicitly guarded by environment checks (e.g., `IWebHostEnvironment.IsDevelopment()`).
**Prevention:** Always use the `IWebHostEnvironment` service to verify the environment before granting any anonymous access or bypassing security middleware.
