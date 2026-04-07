
## 2026-03-10 - Fix API key middleware security bypass
**Vulnerability:** The 'public read access' in `ApiKeyMiddleware.cs` bypasses authentication in all environments because it lacks an explicit environment check.
**Learning:** In convention-based middleware, scoped services and transient state cannot be safely injected into the constructor if they rely on the request lifecycle. However, `IWebHostEnvironment` is a singleton and can be safely injected. Always explicitly use environment checks (`env.IsDevelopment()`) rather than relying on assumed context.
**Prevention:** Always verify the environment using `IWebHostEnvironment.IsDevelopment()` when creating dev-only security bypasses.
