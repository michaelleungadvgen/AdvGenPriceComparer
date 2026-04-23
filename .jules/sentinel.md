## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-02-28 - Information Exposure via Exception Messages
**Vulnerability:** Internal error details (`ex.Message`), which could include stack traces, database schemas, or sensitive system information, were being returned to the client in HTTP 400 response payloads during API data uploads.
**Learning:** Returning unhandled exception messages directly to clients is a common source of information leakage (CWE-209). This exposes internal architecture details that an attacker could use to craft more targeted attacks.
**Prevention:** Catch generic exceptions and log the detailed error internally on the server side, but return a safe, generic error message (e.g., "An internal error occurred") to the client.
