## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Authorization Bypass in API Middleware
**Vulnerability:** The API key middleware (`ApiKeyMiddleware.cs`) allowed anonymous public read access to `GET /api/prices` without restricting it to the development environment.
**Learning:** Hardcoded path and method conditions in convention-based middleware can easily expose sensitive endpoints if environment checks (like `IWebHostEnvironment.IsDevelopment()`) are omitted. In convention-based middleware, Scoped services should be injected into the `InvokeAsync` method to prevent lifecycle mismatches, while Singleton services like `IWebHostEnvironment` can be safely injected via the constructor.
**Prevention:** Always wrap environment-specific bypasses (e.g., local development shortcuts) with strict environment checks using `IWebHostEnvironment.IsDevelopment()`.
