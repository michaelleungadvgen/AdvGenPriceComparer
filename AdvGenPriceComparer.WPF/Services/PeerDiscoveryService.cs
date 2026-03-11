using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Services;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for discovering P2P peers from multiple sources.
/// Supports local files, HTTP URLs, embedded resources, and network shares.
/// </summary>
public class PeerDiscoveryService
{
    private readonly ILoggerService _logger;
    private readonly ServerConfigService _serverConfig;
    private readonly HttpClient _httpClient;
    private readonly string _cachePath;
    private readonly JsonSerializerOptions _jsonOptions;

    // In-memory cache of discovered peers
    private readonly Dictionary<string, DiscoveredPeer> _peers = new();
    private readonly List<DiscoverySource> _sources = new();

    // Events
    public event EventHandler<DiscoveredPeer>? PeerDiscovered;
    public event EventHandler<DiscoveredPeer>? PeerUpdated;
    public event EventHandler<DiscoveryResult>? DiscoveryCompleted;

    /// <summary>
    /// All discovered peers
    /// </summary>
    public IReadOnlyCollection<DiscoveredPeer> Peers => _peers.Values.ToList().AsReadOnly();

    /// <summary>
    /// Configured discovery sources
    /// </summary>
    public IReadOnlyCollection<DiscoverySource> Sources => _sources.AsReadOnly();

    public PeerDiscoveryService(
        ILoggerService logger,
        ServerConfigService serverConfig,
        string? cachePath = null)
    {
        _logger = logger;
        _serverConfig = serverConfig;
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AdvGenPriceComparer/1.0");

        _cachePath = cachePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "peer_discovery_cache.json");

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Load cached peers and default sources
        LoadCachedPeers();
        InitializeDefaultSources();
    }

    #region Source Management

    /// <summary>
    /// Initialize default discovery sources
    /// </summary>
    private void InitializeDefaultSources()
    {
        _sources.Add(new DiscoverySource
        {
            Name = "DefaultStaticPeers",
            Type = DiscoverySourceType.Embedded,
            Path = "AdvGenPriceComparer.WPF.Data.DefaultDiscovery.json",
            Priority = 1,
            IsEnabled = true,
            RefreshIntervalMinutes = 0 // Manual only
        });

        _sources.Add(new DiscoverySource
        {
            Name = "LocalCache",
            Type = DiscoverySourceType.LocalFile,
            Path = _cachePath,
            Priority = 2,
            IsEnabled = true,
            RefreshIntervalMinutes = 0
        });

        _logger.LogInfo($"Initialized {_sources.Count} default peer discovery sources");
    }

    /// <summary>
    /// Add a new discovery source
    /// </summary>
    public void AddSource(DiscoverySource source)
    {
        var existing = _sources.FirstOrDefault(s => s.Name == source.Name);
        if (existing != null)
        {
            _sources.Remove(existing);
        }
        _sources.Add(source);
        _sources.Sort((a, b) => a.Priority.CompareTo(b.Priority));
        _logger.LogInfo($"Added discovery source: {source.Name} ({source.Type})");
    }

    /// <summary>
    /// Remove a discovery source
    /// </summary>
    public bool RemoveSource(string name)
    {
        var source = _sources.FirstOrDefault(s => s.Name == name);
        if (source != null)
        {
            _sources.Remove(source);
            _logger.LogInfo($"Removed discovery source: {name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Enable a discovery source
    /// </summary>
    public bool EnableSource(string name)
    {
        var source = _sources.FirstOrDefault(s => s.Name == name);
        if (source != null)
        {
            source.IsEnabled = true;
            _logger.LogInfo($"Enabled discovery source: {name}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disable a discovery source
    /// </summary>
    public bool DisableSource(string name)
    {
        var source = _sources.FirstOrDefault(s => s.Name == name);
        if (source != null)
        {
            source.IsEnabled = false;
            _logger.LogInfo($"Disabled discovery source: {name}");
            return true;
        }
        return false;
    }

    #endregion

    #region Peer Discovery

    /// <summary>
    /// Discover peers from all enabled sources
    /// </summary>
    public async Task<Dictionary<string, DiscoveryResult>> DiscoverPeersFromAllSourcesAsync(
        bool forceRefresh = false,
        IProgress<DiscoveryProgress>? progress = null)
    {
        var results = new Dictionary<string, DiscoveryResult>();
        var enabledSources = _sources.Where(s => s.IsEnabled).OrderBy(s => s.Priority).ToList();

        _logger.LogInfo($"Starting peer discovery from {enabledSources.Count} enabled sources");
        progress?.Report(new DiscoveryProgress { Phase = "Starting", Percentage = 0, Message = $"Checking {enabledSources.Count} sources..." });

        var totalSources = enabledSources.Count;
        var currentSource = 0;

        foreach (var source in enabledSources)
        {
            currentSource++;
            var percentage = (currentSource - 1) * 100 / totalSources;
            progress?.Report(new DiscoveryProgress
            {
                Phase = "Discovering",
                Percentage = percentage,
                Message = $"Checking source: {source.Name}...",
                CurrentSource = source.Name
            });

            // Check if refresh is needed
            if (!forceRefresh && source.RefreshIntervalMinutes > 0 && source.LastChecked.HasValue)
            {
                var elapsed = DateTime.UtcNow - source.LastChecked.Value;
                if (elapsed.TotalMinutes < source.RefreshIntervalMinutes)
                {
                    _logger.LogInfo($"Skipping source {source.Name}, last checked {elapsed.TotalMinutes:F0} minutes ago");
                    continue;
                }
            }

            try
            {
                var result = await DiscoverPeersFromSourceAsync(source);
                results[source.Name] = result;

                if (result.Success)
                {
                    source.LastChecked = DateTime.UtcNow;
                    source.LastError = null;

                    // Merge discovered peers into cache
                    MergeDiscoveredPeers(result.Peers, source.Name);
                }
                else
                {
                    source.LastError = result.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error discovering peers from source {source.Name}", ex);
                source.LastError = ex.Message;
                results[source.Name] = new DiscoveryResult
                {
                    Success = false,
                    SourceName = source.Name,
                    ErrorMessage = ex.Message
                };
            }
        }

        progress?.Report(new DiscoveryProgress
        {
            Phase = "Complete",
            Percentage = 100,
            Message = $"Discovery complete. Found {_peers.Count} total peers from {results.Count(r => r.Value.Success)} sources."
        });

        // Save updated cache
        await SaveCacheAsync();

        _logger.LogInfo($"Peer discovery complete. Total peers: {_peers.Count}");
        return results;
    }

    /// <summary>
    /// Discover peers from a specific source
    /// </summary>
    public async Task<DiscoveryResult> DiscoverPeersFromSourceAsync(DiscoverySource source)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = new DiscoveryResult
        {
            SourceName = source.Name
        };

        _logger.LogInfo($"Discovering peers from source: {source.Name} ({source.Type})");

        try
        {
            List<DiscoveredPeer> peers;

            switch (source.Type)
            {
                case DiscoverySourceType.LocalFile:
                    peers = await DiscoverFromLocalFileAsync(source.Path);
                    break;

                case DiscoverySourceType.HttpUrl:
                    peers = await DiscoverFromHttpUrlAsync(source);
                    break;

                case DiscoverySourceType.Embedded:
                    peers = await DiscoverFromEmbeddedAsync(source.Path);
                    break;

                case DiscoverySourceType.NetworkShare:
                    peers = await DiscoverFromNetworkShareAsync(source.Path);
                    break;

                default:
                    throw new NotSupportedException($"Discovery source type not supported: {source.Type}");
            }

            result.Success = true;
            result.Peers = peers;

            // Track new and updated peers
            foreach (var peer in peers)
            {
                if (!_peers.ContainsKey(peer.Id))
                {
                    result.NewPeersCount++;
                }
                else if (_peers[peer.Id].LastUpdated != peer.LastUpdated)
                {
                    result.UpdatedPeersCount++;
                }
            }

            _logger.LogInfo($"Discovered {peers.Count} peers from {source.Name} ({result.NewPeersCount} new, {result.UpdatedPeersCount} updated)");
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogError($"Discovery failed for source {source.Name}", ex);
        }
        finally
        {
            stopwatch.Stop();
            result.Duration = stopwatch.Elapsed;
        }

        DiscoveryCompleted?.Invoke(this, result);
        return result;
    }

    /// <summary>
    /// Discover peers from a local file
    /// </summary>
    private async Task<List<DiscoveredPeer>> DiscoverFromLocalFileAsync(string path)
    {
        if (!File.Exists(path))
        {
            _logger.LogWarning($"Discovery file not found: {path}");
            return new List<DiscoveredPeer>();
        }

        var json = await File.ReadAllTextAsync(path);
        var peers = JsonSerializer.Deserialize<List<DiscoveredPeer>>(json, _jsonOptions);
        return peers ?? new List<DiscoveredPeer>();
    }

    /// <summary>
    /// Discover peers from an HTTP/HTTPS URL
    /// </summary>
    private async Task<List<DiscoveredPeer>> DiscoverFromHttpUrlAsync(DiscoverySource source)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, source.Path);

        if (!string.IsNullOrEmpty(source.ApiKey))
        {
            request.Headers.Add("X-API-Key", source.ApiKey);
        }

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var peers = await response.Content.ReadFromJsonAsync<List<DiscoveredPeer>>(_jsonOptions);
        return peers ?? new List<DiscoveredPeer>();
    }

    /// <summary>
    /// Discover peers from an embedded resource
    /// </summary>
    private async Task<List<DiscoveredPeer>> DiscoverFromEmbeddedAsync(string resourcePath)
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(resourcePath);

        if (stream == null)
        {
            _logger.LogWarning($"Embedded resource not found: {resourcePath}");
            return new List<DiscoveredPeer>();
        }

        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        var peers = JsonSerializer.Deserialize<List<DiscoveredPeer>>(json, _jsonOptions);
        return peers ?? new List<DiscoveredPeer>();
    }

    /// <summary>
    /// Discover peers from a network share/UNC path
    /// </summary>
    private async Task<List<DiscoveredPeer>> DiscoverFromNetworkShareAsync(string path)
    {
        // Network shares work the same as local files in .NET
        // UNC paths like \\server\share\discovery.json are supported
        return await DiscoverFromLocalFileAsync(path);
    }

    #endregion

    #region Peer Management

    /// <summary>
    /// Merge newly discovered peers into the cache
    /// </summary>
    private void MergeDiscoveredPeers(List<DiscoveredPeer> peers, string sourceName)
    {
        foreach (var peer in peers)
        {
            // Ensure source is tracked
            if (string.IsNullOrEmpty(peer.Source))
            {
                peer.Source = sourceName;
            }

            if (_peers.TryGetValue(peer.Id, out var existingPeer))
            {
                // Update existing peer if the new data is newer
                var newTime = peer.LastUpdated ?? peer.LastSeen ?? DateTime.MinValue;
                var existingTime = existingPeer.LastUpdated ?? existingPeer.LastSeen ?? DateTime.MinValue;

                if (newTime > existingTime)
                {
                    // Preserve statistics
                    peer.SuccessfulImports = existingPeer.SuccessfulImports;
                    peer.FailedImports = existingPeer.FailedImports;
                    peer.IsEnabled = existingPeer.IsEnabled;
                    peer.IsFavorite = existingPeer.IsFavorite;
                    peer.DiscoveredAt = existingPeer.DiscoveredAt;

                    _peers[peer.Id] = peer;
                    PeerUpdated?.Invoke(this, peer);
                }
            }
            else
            {
                // New peer
                _peers[peer.Id] = peer;
                PeerDiscovered?.Invoke(this, peer);
            }
        }
    }

    /// <summary>
    /// Get a specific peer by ID
    /// </summary>
    public DiscoveredPeer? GetPeer(string id)
    {
        return _peers.TryGetValue(id, out var peer) ? peer : null;
    }

    /// <summary>
    /// Get peers by region
    /// </summary>
    public IEnumerable<DiscoveredPeer> GetPeersByRegion(string region)
    {
        return _peers.Values.Where(p =>
            p.Region?.Equals(region, StringComparison.OrdinalIgnoreCase) ?? false);
    }

    /// <summary>
    /// Get healthy peers
    /// </summary>
    public IEnumerable<DiscoveredPeer> GetHealthyPeers()
    {
        return _peers.Values.Where(p =>
            p.HealthStatus == PeerHealthStatus.Healthy && p.IsEnabled);
    }

    /// <summary>
    /// Get peers available for static data import
    /// </summary>
    public IEnumerable<DiscoveredPeer> GetStaticPeers()
    {
        return _peers.Values.Where(p =>
            p.Type == "static_peer" &&
            p.IsEnabled &&
            (p.HealthStatus == PeerHealthStatus.Healthy || p.HealthStatus == PeerHealthStatus.Unknown));
    }

    /// <summary>
    /// Enable a peer
    /// </summary>
    public bool EnablePeer(string id)
    {
        if (_peers.TryGetValue(id, out var peer))
        {
            peer.IsEnabled = true;
            _logger.LogInfo($"Enabled peer: {id}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Disable a peer
    /// </summary>
    public bool DisablePeer(string id)
    {
        if (_peers.TryGetValue(id, out var peer))
        {
            peer.IsEnabled = false;
            _logger.LogInfo($"Disabled peer: {id}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Mark a peer as favorite
    /// </summary>
    public bool SetPeerFavorite(string id, bool isFavorite)
    {
        if (_peers.TryGetValue(id, out var peer))
        {
            peer.IsFavorite = isFavorite;
            _logger.LogInfo($"Set peer {id} favorite status to: {isFavorite}");
            return true;
        }
        return false;
    }

    /// <summary>
    /// Remove a peer
    /// </summary>
    public bool RemovePeer(string id)
    {
        if (_peers.Remove(id))
        {
            _logger.LogInfo($"Removed peer: {id}");
            return true;
        }
        return false;
    }

    #endregion

    #region Health Checking

    /// <summary>
    /// Check health of a specific peer by attempting to access its discovery.json
    /// </summary>
    public async Task<PeerHealthStatus> CheckPeerHealthAsync(DiscoveredPeer peer)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        peer.LastHealthCheck = DateTime.UtcNow;

        try
        {
            var discoveryUrl = peer.Address.TrimEnd('/') + "/discovery.json";
            using var response = await _httpClient.GetAsync(discoveryUrl);

            stopwatch.Stop();
            peer.ResponseTimeMs = stopwatch.ElapsedMilliseconds;

            if (response.IsSuccessStatusCode)
            {
                peer.HealthStatus = PeerHealthStatus.Healthy;
                peer.HealthCheckError = null;
                _logger.LogInfo($"Peer {peer.Id} health check passed ({peer.ResponseTimeMs}ms)");
            }
            else
            {
                peer.HealthStatus = PeerHealthStatus.Unhealthy;
                peer.HealthCheckError = $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}";
                _logger.LogWarning($"Peer {peer.Id} health check failed: {peer.HealthCheckError}");
            }
        }
        catch (TaskCanceledException)
        {
            stopwatch.Stop();
            peer.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            peer.HealthStatus = PeerHealthStatus.Timeout;
            peer.HealthCheckError = "Connection timed out";
            _logger.LogWarning($"Peer {peer.Id} health check timed out");
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            peer.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            peer.HealthStatus = PeerHealthStatus.Error;
            peer.HealthCheckError = ex.Message;
            _logger.LogError($"Peer {peer.Id} health check error", ex);
        }

        return peer.HealthStatus;
    }

    /// <summary>
    /// Check health of all enabled peers
    /// </summary>
    public async Task<Dictionary<string, PeerHealthStatus>> CheckAllPeersHealthAsync(
        IProgress<string>? progress = null)
    {
        var results = new Dictionary<string, PeerHealthStatus>();
        var enabledPeers = _peers.Values.Where(p => p.IsEnabled).ToList();

        _logger.LogInfo($"Checking health of {enabledPeers.Count} enabled peers");
        progress?.Report($"Checking health of {enabledPeers.Count} peers...");

        foreach (var peer in enabledPeers)
        {
            progress?.Report($"Checking {peer.Id}...");
            var status = await CheckPeerHealthAsync(peer);
            results[peer.Id] = status;
        }

        var healthyCount = results.Count(r => r.Value == PeerHealthStatus.Healthy);
        progress?.Report($"Health check complete. {healthyCount}/{enabledPeers.Count} peers healthy.");
        _logger.LogInfo($"Health check complete. {healthyCount}/{enabledPeers.Count} peers healthy");

        await SaveCacheAsync();
        return results;
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Load peers from cache file
    /// </summary>
    private void LoadCachedPeers()
    {
        try
        {
            if (File.Exists(_cachePath))
            {
                var json = File.ReadAllText(_cachePath);
                var cachedPeers = JsonSerializer.Deserialize<Dictionary<string, DiscoveredPeer>>(json, _jsonOptions);

                if (cachedPeers != null)
                {
                    foreach (var peer in cachedPeers)
                    {
                        _peers[peer.Key] = peer.Value;
                    }
                    _logger.LogInfo($"Loaded {_peers.Count} peers from cache");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading peer cache", ex);
        }
    }

    /// <summary>
    /// Save peers to cache file
    /// </summary>
    public async Task SaveCacheAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_cachePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(_peers, _jsonOptions);
            await File.WriteAllTextAsync(_cachePath, json);
            _logger.LogDebug($"Saved {_peers.Count} peers to cache");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error saving peer cache", ex);
        }
    }

    /// <summary>
    /// Clear all cached peers
    /// </summary>
    public void ClearCache()
    {
        _peers.Clear();
        if (File.Exists(_cachePath))
        {
            File.Delete(_cachePath);
        }
        _logger.LogInfo("Peer cache cleared");
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get discovery statistics
    /// </summary>
    public DiscoveryStatistics GetStatistics()
    {
        var enabledSources = _sources.Where(s => s.IsEnabled).ToList();
        var workingSources = enabledSources.Count(s => string.IsNullOrEmpty(s.LastError));

        return new DiscoveryStatistics
        {
            TotalSources = _sources.Count,
            EnabledSources = enabledSources.Count,
            WorkingSources = workingSources,
            TotalPeers = _peers.Count,
            HealthyPeers = _peers.Values.Count(p => p.HealthStatus == PeerHealthStatus.Healthy),
            UnhealthyPeers = _peers.Values.Count(p =>
                p.HealthStatus == PeerHealthStatus.Unhealthy ||
                p.HealthStatus == PeerHealthStatus.Error ||
                p.HealthStatus == PeerHealthStatus.Timeout),
            EnabledPeers = _peers.Values.Count(p => p.IsEnabled),
            LastDiscoveryRun = _sources.Max(s => s.LastChecked)
        };
    }

    #endregion

    #region Import Tracking

    /// <summary>
    /// Record a successful import from a peer
    /// </summary>
    public void RecordSuccessfulImport(string peerId)
    {
        if (_peers.TryGetValue(peerId, out var peer))
        {
            peer.SuccessfulImports++;
            _ = SaveCacheAsync();
        }
    }

    /// <summary>
    /// Record a failed import from a peer
    /// </summary>
    public void RecordFailedImport(string peerId)
    {
        if (_peers.TryGetValue(peerId, out var peer))
        {
            peer.FailedImports++;
            _ = SaveCacheAsync();
        }
    }

    #endregion
}

/// <summary>
/// Progress information for discovery operations
/// </summary>
public class DiscoveryProgress
{
    public string Phase { get; set; } = string.Empty;
    public int Percentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? CurrentSource { get; set; }
    public int? PeersFound { get; set; }
}
