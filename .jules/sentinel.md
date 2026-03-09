## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2024-03-21 - [Fix Authentication Bypass in ApiKeyMiddleware]
**Vulnerability:** The API key middleware hardcoded a bypass for GET requests to `/api/prices` that was meant to be development-only but was active in all environments, exposing price data endpoints publicly.
**Learning:** Hardcoded environment-specific logic (e.g., checking paths without checking `IWebHostEnvironment.IsDevelopment()`) in middleware can unintentionally persist to production.
**Prevention:** Always explicitly check `IWebHostEnvironment.IsDevelopment()` when implementing development-only security bypasses.
