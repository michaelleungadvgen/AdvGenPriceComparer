## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2025-02-28 - Authorization and Rate Limit Bypass in Custom Middleware
**Vulnerability:** Custom ASP.NET Core middleware (`ApiKeyMiddleware` and `RateLimitMiddleware`) used loose string matching (`path.Contains("/health")`) and prefix matching without trailing slashes (`path.StartsWith("/api/prices")`). This allowed attackers to bypass authentication and rate limits by appending the matched strings to other paths (e.g., `/api/admin?query=/health` or `/api/prices_fake`).
**Learning:** In HTTP request routing, using `Contains` on URL paths is highly dangerous because the target string can appear anywhere (e.g., query strings or middle of path segments). Prefix matching without a trailing slash (directory boundary) can unintentionally match completely different endpoints.
**Prevention:** Always use exact matching (`==`) or strict prefix matching that includes directory boundaries (e.g., `StartsWith("/path/")`). For authorization and routing, prefer built-in ASP.NET Core features (like `[Authorize]` attributes and endpoint routing) over manual path string parsing in middleware whenever possible. Also, ensure development-only endpoints explicitly check the environment (`IWebHostEnvironment.IsDevelopment()`).
