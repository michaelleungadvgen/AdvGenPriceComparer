## 2024-05-24 - API Error Information Leakage
**Vulnerability:** The API `UploadData` endpoint was leaking raw exception messages (`ex.Message`) back to clients in the JSON response when an error occurred, specifically in the `ErrorMessage = $"Internal error: {ex.Message}"` assignment.
**Learning:** Returning unhandled exception details directly in API responses can expose sensitive information about the server environment, backend architecture, or database structure to an attacker.
**Prevention:** Always catch exceptions and return a generic error message (e.g., "An internal server error occurred.") to the client, while securely logging the full exception details on the server side using the configured logger.
