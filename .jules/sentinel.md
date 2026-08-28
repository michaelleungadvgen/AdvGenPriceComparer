## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-06-17 - Prevent Path-Based Security Bypasses in Middleware
**Vulnerability:** Path exclusion matching in ASP.NET middleware using `Contains()` (e.g., `path.Contains("/health")`) allowed bypassing authentication and rate limiting by appending arbitrary strings to unauthorized paths. Furthermore, `StartsWith()` without trailing slashes (e.g., `/api/prices`) allowed access to unintended sibling routes (e.g., `/api/prices_admin`).
**Learning:** Always use strict exact matching or strict directory prefix matching (`StartsWith("/path/")`) for security-critical route exclusions.
**Prevention:** Ensure all path exclusions in custom middleware rely on exact matching or strict boundary prefixes and explicitly check environment context (e.g., `IsDevelopment()`) before granting unauthenticated access.
