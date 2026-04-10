namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents the health status of the server
/// </summary>
public class HealthStatus
{
    /// <summary>
    /// Overall health status (Healthy, Degraded, Unhealthy)
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Timestamp when the health check was performed
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Server version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Individual component health checks
    /// </summary>
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();

    /// <summary>
    /// Server uptime information
    /// </summary>
    public UptimeInfo? Uptime { get; set; }
}

/// <summary>
/// Health status of an individual component
/// </summary>
public class ComponentHealth
{
    /// <summary>
    /// Component status (Healthy, Degraded, Unhealthy)
    /// </summary>
    public string Status { get; set; } = "Healthy";

    /// <summary>
    /// Response time in milliseconds
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Optional error message if component is unhealthy
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Additional details about the component
    /// </summary>
    public Dictionary<string, object>? Details { get; set; }
}

/// <summary>
/// Server uptime information
/// </summary>
public class UptimeInfo
{
    /// <summary>
    /// Server start time
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Total uptime duration
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Uptime as a formatted string (e.g., "2d 5h 30m")
    /// </summary>
    public string Formatted { get; set; } = string.Empty;
}
