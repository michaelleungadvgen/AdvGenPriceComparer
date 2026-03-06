## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-06 - Hardcoded Authorization Bypass
**Vulnerability:** The API key middleware implemented an authorization bypass using a simple path check (`path.StartsWith("/api/prices")`) intended for development testing, but failed to actually verify the hosting environment. This effectively allowed unauthenticated access to sensitive endpoints in production.
**Learning:** Development conveniences (like bypassing auth) often slip into production if they rely on comments instead of code. Always enforce environment checks (`IWebHostEnvironment.IsDevelopment()`) in code when implementing temporary or environment-specific security bypasses.
**Prevention:** Remove hardcoded paths and bypass logic in production code. If a development-only feature is required, wrap it completely within an environment check block.
