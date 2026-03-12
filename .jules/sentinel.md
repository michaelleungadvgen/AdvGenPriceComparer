## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2026-03-05 - Missing Authentication on API Endpoint
**Vulnerability:** The API allowed unauthenticated GET requests to `/api/prices` endpoints in all environments. This exposed all pricing data to anonymous access.
**Learning:** Hardcoded path exclusions from authentication middleware often lack environment constraints. Development conveniences (like allowing public reads for UI testing) can silently become production vulnerabilities if not explicitly scoped using `IWebHostEnvironment.IsDevelopment()`.
**Prevention:** Always scope authentication bypasses intended for development to the development environment using `_env.IsDevelopment()`. Never assume path-based filters are inherently safe for production.
