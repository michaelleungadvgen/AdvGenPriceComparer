## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - API Key Bypass in Production
**Vulnerability:** The API key middleware (`ApiKeyMiddleware.cs`) allowed anonymous GET requests to `/api/prices` in all environments, completely bypassing authentication.
**Learning:** Development-only conveniences (like skipping auth for testing endpoints) can easily leak into production if the environment check is implied by comments but not enforced in code.
**Prevention:** Always explicitly check `IWebHostEnvironment.IsDevelopment()` before applying any development-specific security bypasses, rather than relying on route paths alone.
