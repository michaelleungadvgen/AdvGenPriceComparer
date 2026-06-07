## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-05-24 - Authorization and Rate Limit Bypass in Middleware
**Vulnerability:** API endpoints intended to be secured could be bypassed if the user manipulated the path to contain specific sub-strings (`/swagger` or `/health`). Attackers could exploit this by adding these strings as query parameters or manipulating valid path segments (e.g., `/api/admin/users?q=/swagger`).
**Learning:** Using `Contains` for string matching in routing/middleware path validation is highly susceptible to bypass attacks. `Contains` does not restrict *where* the matched string appears.
**Prevention:** Always use strict matching mechanisms for routing logic, such as exact path equivalence (`==`) or prefix matching (`StartsWith()`) to accurately scope exclusions to the exact intended endpoints.
