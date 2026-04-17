## 2024-05-15 - Authorization Bypass Due to Missing Environment Checks
**Vulnerability:** The API key middleware (`ApiKeyMiddleware.cs`) allowed unauthorized read access (GET requests) to `/api/prices` in all environments, despite code comments indicating this was meant for development only.
**Learning:** Security bypasses intended for local development should never rely solely on implicit states or comments; they must explicitly check the environment to ensure they aren't accidentally promoted to production.
**Prevention:** Always inject `IWebHostEnvironment` and wrap development-only bypasses with an explicit `env.IsDevelopment()` check.
