## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-05 - Insecure CORS Policy (AllowAnyOrigin)
**Vulnerability:** The ASP.NET Core API used an overly permissive CORS configuration (`AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()`) for its `SignalRPolicy`. This entirely defeated the browser's Same-Origin Policy.
**Learning:** Developers often use wildcard CORS policies (`*`) during development to quickly bypass CORS errors, but forget to restrict them before production. This allows any malicious website a user visits to make authenticated/cross-origin requests to the API on the user's behalf.
**Prevention:** Never use `AllowAnyOrigin()` in production APIs. Always restrict CORS to specific, trusted domains using `WithOrigins(...)` loaded from environment-specific configuration files.
