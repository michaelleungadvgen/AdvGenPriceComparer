## 2025-02-28 - Plaintext API Key Storage in Settings File [RESOLVED]
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Status:** ✅ FIXED - SettingsService now encrypts API keys using DPAPI
**Implementation:** 
- `EncryptString()` method uses `ProtectedData.Protect()` with `DataProtectionScope.CurrentUser`
- `DecryptString()` method uses `ProtectedData.Unprotect()` with migration support from plaintext
- Graceful fallback for non-Windows platforms (returns plaintext for tests)
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.
## 2026-06-21 - Exception Message Information Leakage
**Vulnerability:** API endpoints and health checks returned raw `Exception.Message` values directly to the client (e.g., in `HealthCheckController.cs` and `PricesController.cs`).
**Learning:** Returning raw exception messages from database or internal service layers can leak sensitive infrastructure details, file paths, or schema information to potential attackers. Internal logs should capture the detailed exception, while the client should receive a safe, generic message.
**Prevention:** Always log the full exception internally (e.g., via `ILogger`) and return a generic error message (like "An internal error occurred") to the API consumer. Do not interpolate `ex.Message` into HTTP responses.
