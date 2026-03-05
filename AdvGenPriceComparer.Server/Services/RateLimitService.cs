using System.Collections.Concurrent;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// In-memory rate limiting service using sliding window
/// </summary>
public class RateLimitService : IRateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitEntry> _entries = new();
    private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(5);
    private DateTime _lastCleanup = DateTime.UtcNow;

    /// <inheritdoc />
    public bool IsAllowed(string key, int limit, int windowSeconds = 60)
    {
        CleanupIfNeeded();

        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-windowSeconds);

        var entry = _entries.GetOrAdd(key, _ => new RateLimitEntry());
        
        lock (entry)
        {
            // Remove requests outside the window
            entry.Requests.RemoveAll(r => r < windowStart);

            if (entry.Requests.Count >= limit)
            {
                return false;
            }

            entry.Requests.Add(now);
            return true;
        }
    }

    /// <inheritdoc />
    public int GetRemainingRequests(string key, int limit, int windowSeconds = 60)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-windowSeconds);

        if (!_entries.TryGetValue(key, out var entry))
        {
            return limit;
        }

        lock (entry)
        {
            entry.Requests.RemoveAll(r => r < windowStart);
            return Math.Max(0, limit - entry.Requests.Count);
        }
    }

    /// <inheritdoc />
    public TimeSpan? GetRetryAfter(string key, int limit, int windowSeconds = 60)
    {
        var now = DateTime.UtcNow;
        var windowStart = now.AddSeconds(-windowSeconds);

        if (!_entries.TryGetValue(key, out var entry))
        {
            return null;
        }

        lock (entry)
        {
            entry.Requests.RemoveAll(r => r < windowStart);

            if (entry.Requests.Count < limit)
            {
                return null;
            }

            // Find when the oldest request in the window will expire
            var oldestRequest = entry.Requests.Min();
            var retryAfter = oldestRequest.AddSeconds(windowSeconds) - now;
            return retryAfter > TimeSpan.Zero ? retryAfter : TimeSpan.Zero;
        }
    }

    private void CleanupIfNeeded()
    {
        var now = DateTime.UtcNow;
        if (now - _lastCleanup < _cleanupInterval)
        {
            return;
        }

        _lastCleanup = now;
        var cutoff = now.AddMinutes(-10); // Remove entries older than 10 minutes

        foreach (var key in _entries.Keys)
        {
            if (_entries.TryGetValue(key, out var entry))
            {
                lock (entry)
                {
                    if (entry.LastRequest < cutoff)
                    {
                        _entries.TryRemove(key, out _);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Rate limit tracking entry
/// </summary>
public class RateLimitEntry
{
    public List<DateTime> Requests { get; } = new();
    public DateTime LastRequest => Requests.Count > 0 ? Requests.Max() : DateTime.MinValue;
}
