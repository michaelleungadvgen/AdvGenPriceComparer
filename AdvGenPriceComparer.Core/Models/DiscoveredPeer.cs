namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a peer discovered from a discovery source.
/// This is used for P2P static data sharing network.
/// </summary>
public class DiscoveredPeer
{
    /// <summary>
    /// Unique identifier for the peer (convention: region-city-number)
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Type of peer: full_peer (active P2P node) or static_peer (HTTP-hosted files)
    /// </summary>
    public string Type { get; set; } = "static_peer";

    /// <summary>
    /// Base URL for accessing peer data (e.g., https://example.com/data)
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable geographic location (e.g., "Brisbane, QLD, Australia")
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Last successful connection timestamp (for full_peer)
    /// </summary>
    public DateTime? LastSeen { get; set; }

    /// <summary>
    /// Last data update timestamp (for static_peer)
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    /// <summary>
    /// Human-readable description of the peer
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Geographic region code (e.g., AU, NSW, VIC, QLD)
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// The source from which this peer was discovered
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// When this peer was first discovered
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Health status of the peer
    /// </summary>
    public PeerHealthStatus HealthStatus { get; set; } = PeerHealthStatus.Unknown;

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime? LastHealthCheck { get; set; }

    /// <summary>
    /// Health check error message (if any)
    /// </summary>
    public string? HealthCheckError { get; set; }

    /// <summary>
    /// Response time in milliseconds from last health check
    /// </summary>
    public long? ResponseTimeMs { get; set; }

    /// <summary>
    /// Number of successful imports from this peer
    /// </summary>
    public int SuccessfulImports { get; set; }

    /// <summary>
    /// Number of failed import attempts from this peer
    /// </summary>
    public int FailedImports { get; set; }

    /// <summary>
    /// Whether the peer is currently enabled for imports
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Whether the peer is a favorite/trusted source
    /// </summary>
    public bool IsFavorite { get; set; }

    /// <summary>
    /// Calculate overall success rate for imports
    /// </summary>
    public double SuccessRate
    {
        get
        {
            var total = SuccessfulImports + FailedImports;
            return total > 0 ? (double)SuccessfulImports / total : 0;
        }
    }

    /// <summary>
    /// Convert to ServerInfo for use with NetworkManager
    /// </summary>
    public ServerInfo ToServerInfo()
    {
        var uri = new Uri(Address);
        return new ServerInfo
        {
            Name = Id,
            Host = uri.Host,
            Port = uri.Port,
            IsSecure = uri.Scheme == "https",
            Region = Region,
            Description = Description,
            IsActive = HealthStatus == PeerHealthStatus.Healthy,
            LastSeen = LastSeen ?? LastUpdated ?? DateTime.MinValue
        };
    }
}

/// <summary>
/// Health status of a discovered peer
/// </summary>
public enum PeerHealthStatus
{
    Unknown,
    Healthy,
    Unhealthy,
    Timeout,
    Error
}

/// <summary>
/// A discovery source configuration for loading peers
/// </summary>
public class DiscoverySource
{
    /// <summary>
    /// Unique name/identifier for this source
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of source: LocalFile, HttpUrl, Embedded
    /// </summary>
    public DiscoverySourceType Type { get; set; } = DiscoverySourceType.HttpUrl;

    /// <summary>
    /// Path or URL to the discovery.json file
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// Whether this source is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Priority order (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Optional API key for authenticated sources
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// How often to refresh from this source (in minutes), 0 = manual only
    /// </summary>
    public int RefreshIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Last time this source was checked
    /// </summary>
    public DateTime? LastChecked { get; set; }

    /// <summary>
    /// Last error from this source (if any)
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// Types of discovery sources
/// </summary>
public enum DiscoverySourceType
{
    /// <summary>
    /// Local file path (e.g., C:\data\discovery.json)
    /// </summary>
    LocalFile,

    /// <summary>
    /// HTTP/HTTPS URL
    /// </summary>
    HttpUrl,

    /// <summary>
    /// Embedded resource in the application
    /// </summary>
    Embedded,

    /// <summary>
    /// Network share or UNC path
    /// </summary>
    NetworkShare
}

/// <summary>
/// Result of a peer discovery operation
/// </summary>
public class DiscoveryResult
{
    /// <summary>
    /// Whether the discovery was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Peers discovered
    /// </summary>
    public List<DiscoveredPeer> Peers { get; set; } = new();

    /// <summary>
    /// Error message if discovery failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Source that was used for discovery
    /// </summary>
    public string SourceName { get; set; } = string.Empty;

    /// <summary>
    /// When the discovery was performed
    /// </summary>
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Time taken to perform discovery
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Number of new peers found (not previously known)
    /// </summary>
    public int NewPeersCount { get; set; }

    /// <summary>
    /// Number of existing peers updated
    /// </summary>
    public int UpdatedPeersCount { get; set; }
}

/// <summary>
/// Statistics for peer discovery operations
/// </summary>
public class DiscoveryStatistics
{
    /// <summary>
    /// Total number of discovery sources configured
    /// </summary>
    public int TotalSources { get; set; }

    /// <summary>
    /// Number of enabled sources
    /// </summary>
    public int EnabledSources { get; set; }

    /// <summary>
    /// Number of working sources (last check successful)
    /// </summary>
    public int WorkingSources { get; set; }

    /// <summary>
    /// Total peers known
    /// </summary>
    public int TotalPeers { get; set; }

    /// <summary>
    /// Number of healthy peers
    /// </summary>
    public int HealthyPeers { get; set; }

    /// <summary>
    /// Number of unhealthy peers
    /// </summary>
    public int UnhealthyPeers { get; set; }

    /// <summary>
    /// Number of enabled peers (available for import)
    /// </summary>
    public int EnabledPeers { get; set; }

    /// <summary>
    /// Last discovery run timestamp
    /// </summary>
    public DateTime? LastDiscoveryRun { get; set; }

    /// <summary>
    /// Next scheduled discovery timestamp
    /// </summary>
    public DateTime? NextScheduledDiscovery { get; set; }
}
