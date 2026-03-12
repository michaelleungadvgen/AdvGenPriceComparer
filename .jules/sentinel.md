## 2025-02-28 - Plaintext API Key Storage in Settings File
**Vulnerability:** API keys were being written to local JSON files (`settings.json`) in plaintext by the `SettingsService`, making them accessible to any user or application with file system read access.
**Learning:** Local application settings stored in AppData are often treated as "secure enough" by developers, but they remain highly vulnerable to local credential theft if unencrypted.
**Prevention:** Always encrypt sensitive settings (like API keys, passwords, or tokens) at rest. For Windows desktop applications, utilize `System.Security.Cryptography.ProtectedData` (DPAPI) bound to the `CurrentUser` scope, which seamlessly encrypts data using the user's OS credentials.

## 2025-03-12 - Missing Environment Check in Middleware Authentication Bypass
**Vulnerability:** The `ApiKeyMiddleware` in the ASP.NET Core server allowed unauthenticated access to `GET /api/prices` based on the method and path, but failed to actually verify the environment using `IWebHostEnvironment.IsDevelopment()`. This would expose the endpoints in production.
**Learning:** Hardcoded path and method checks for "development" features are insufficient. Environment-specific security bypasses must explicitly evaluate the hosting environment (e.g., using `IsDevelopment()`) to prevent accidental production exposure.
**Prevention:** Always inject and utilize `IWebHostEnvironment` (which is a Singleton and safe to inject into convention-based middleware constructors) to enforce environment constraints when defining security exceptions.

## 2025-03-12 - Insecure Auto-Update Execution without Cryptographic Verification
**Vulnerability:** The application's `UpdateService` fetched an installer `.msi` via HTTP/HTTPS and immediately executed it using `Process.Start` without validating its integrity against the provided JSON manifest's `FileHash` property. This could allow execution of maliciously injected files during transit or from a compromised update server. (Note: A previous finding about `ApiKeyMiddleware` was reverted as the "bypass" was actually an intentional feature for public API access).
**Learning:** Even if update manifests are retrieved over HTTPS, relying solely on transport-layer security is insufficient defense against supply chain attacks or compromised endpoints.
**Prevention:** Always verify downloaded executables using strong cryptographic hashes (e.g., SHA-256) matching a known-good signature or manifest *before* invoking the OS process executor.
