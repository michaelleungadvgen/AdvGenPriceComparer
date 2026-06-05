## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2024-06-05 - Middleware Bypass Vulnerabilities
**Vulnerability:** API key authentication and rate limiting could be bypassed due to substring path matching (`.Contains("/swagger")`) instead of prefix matching, and development-only endpoint bypasses lacked an environment check.
**Learning:** Custom middleware path filtering is prone to bypass if path matching is not anchored. Additionally, comments about "development" bypasses do not enforce environments at runtime without an explicit `IsDevelopment()` check.
**Prevention:** Always use `StartsWith()` or exact matching (`==`) for path exclusion rules. Inject `IWebHostEnvironment` into middleware `InvokeAsync` and explicitly check `.IsDevelopment()` before allowing development-specific bypasses.
