namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents an API key for client authentication
/// </summary>
public class ApiKey
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Friendly name for the API key
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Hashed API key value
    /// </summary>
    public string KeyHash { get; set; } = string.Empty;

    /// <summary>
    /// Whether the key is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Maximum requests per hour
    /// </summary>
    public int RateLimit { get; set; } = 100;

    /// <summary>
    /// When the key was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the key expires (null = never)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// Last time the key was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }

    /// <summary>
    /// Total number of requests made with this key
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// When the key was revoked (null if not revoked)
    /// </summary>
    public DateTime? RevokedAt { get; set; }
}
