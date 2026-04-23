## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-05-24 - Unconfigurable API Key Bypass
**Vulnerability:** The API key middleware (`ApiKeyMiddleware.cs`) allowed anonymous GET requests to `/api/prices` in all environments without offering a configuration to toggle this behavior, effectively bypassing authentication for reads regardless of user intent.
**Learning:** Development-only conveniences or hardcoded bypasses can easily become security flaws if they ignore application configuration settings. If an API is meant to be public or private, this should be governed by an explicit configuration flag rather than hardcoded logic or loose environment assumptions.
**Prevention:** Always bind security bypasses to explicit configuration settings (like `ApiSettings:RequireApiKey` or `ApiSettings:AllowPublicReadAccess`) so administrators can explicitly enable or disable the public surface area of the application based on their environment.
