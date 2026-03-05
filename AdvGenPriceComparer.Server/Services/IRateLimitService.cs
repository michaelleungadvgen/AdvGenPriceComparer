namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Interface for rate limiting service
/// </summary>
public interface IRateLimitService
{
    /// <summary>
    /// Check if a request is allowed for the given key
    /// </summary>
    /// <param name="key">API key or IP address</param>
    /// <param name="limit">Maximum requests allowed</param>
    /// <param name="windowSeconds">Time window in seconds</param>
    /// <returns>True if request is allowed, false if rate limited</returns>
    bool IsAllowed(string key, int limit, int windowSeconds = 60);

    /// <summary>
    /// Get remaining requests for a key
    /// </summary>
    int GetRemainingRequests(string key, int limit, int windowSeconds = 60);

    /// <summary>
    /// Get time until next request is allowed
    /// </summary>
    TimeSpan? GetRetryAfter(string key, int limit, int windowSeconds = 60);
}
