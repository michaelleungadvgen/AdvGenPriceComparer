## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-03-05 - Fix Authentication Bypass in Middleware Path Checking
**Vulnerability:** API key and rate limit validation was skipped for any path containing "/swagger" or "/health" (e.g., "/api/secret?param=/swagger"), allowing authentication bypass on protected endpoints.
**Learning:** Using `String.Contains` on request paths is fundamentally insecure because an attacker controls the entire path string and can easily append bypass strings anywhere in the URL.
**Prevention:** Always use strict prefix matching (`String.StartsWith`) or exact matching (`String.Equals`) when evaluating HTTP request paths for security bypasses.
