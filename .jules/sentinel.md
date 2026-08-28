## 2023-10-25 - Auth Bypass via Insecure Path Matching
**Vulnerability:** The custom `ApiKeyMiddleware` and `RateLimitMiddleware` used `.Contains("/health")` and `.Contains("/swagger")` to bypass authentication and rate limiting for health and documentation endpoints.
**Learning:** Using substring matching (`Contains`) for path-based authorization exclusions allows attackers to bypass security controls by injecting the bypass string into an otherwise protected route (e.g., `/api/items/by-category/health`).
**Prevention:** Always use exact matching (`==`) or prefix matching (`StartsWith()`) when evaluating request paths for security exclusions.
