namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Service interface for P2P (peer-to-peer) network operations.
/// This interface defines the contract for network communication without
/// infrastructure-specific dependencies (Clean Architecture principle).
/// Implementations handle the actual socket/transport layer details.
/// </summary>
public interface IP2PNetworkService : IDisposable
{
    /// <summary>
    /// Unique identifier for this network node
    /// </summary>
    string NodeId { get; }

    /// <summary>
    /// Whether the P2P server is currently running
    /// </summary>
    bool IsServerRunning { get; }

    /// <summary>
    /// List of currently connected peers
    /// </summary>
    IReadOnlyList<NetworkPeerInfo> ConnectedPeers { get; }

    /// <summary>
    /// Event raised when a price update is received from a peer
    /// </summary>
    event EventHandler<PriceShareEventArgs>? PriceReceived;

    /// <summary>
    /// Event raised when a peer connects
    /// </summary>
    event EventHandler<NetworkPeerInfo>? PeerConnected;

    /// <summary>
    /// Event raised when a peer disconnects
    /// </summary>
    event EventHandler<NetworkPeerInfo>? PeerDisconnected;

    /// <summary>
    /// Event raised when a network error occurs
    /// </summary>
    event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Start the P2P server to accept incoming connections
    /// </summary>
    /// <param name="port">Port number to listen on (default: 8081)</param>
    /// <returns>True if server started successfully</returns>
    Task<bool> StartServerAsync(int port = 8081);

    /// <summary>
    /// Stop the P2P server and disconnect all clients
    /// </summary>
    void StopServer();

    /// <summary>
    /// Connect to a peer server by hostname and port
    /// </summary>
    /// <param name="host">Hostname or IP address</param>
    /// <param name="port">Port number</param>
    /// <returns>True if connection was successful</returns>
    Task<bool> ConnectToServerAsync(string host, int port);

    /// <summary>
    /// Disconnect from a specific server
    /// </summary>
    Task DisconnectFromServerAsync(string host, int port);

    /// <summary>
    /// Share a price update with all connected peers
    /// </summary>
    Task SharePriceAsync(string itemId, string placeId, decimal price,
        bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null);

    /// <summary>
    /// Request price synchronization from connected peers
    /// </summary>
    /// <param name="region">Optional region filter</param>
    Task RequestPriceSyncAsync(string? region = null);

    /// <summary>
    /// Send a heartbeat to all connected peers
    /// </summary>
    Task SendHeartbeatAsync();

    /// <summary>
    /// Discover and connect to configured servers in a region
    /// </summary>
    Task DiscoverAndConnectToServersAsync(string? region = null);
}

/// <summary>
/// Information about a connected network peer
/// </summary>
public class NetworkPeerInfo
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsConnected { get; set; }
    public string? Version { get; set; }
    public string? Region { get; set; }
}

/// <summary>
/// Event arguments for price share events
/// </summary>
public class PriceShareEventArgs : EventArgs
{
    public string? ItemName { get; set; }
    public string? ItemBrand { get; set; }
    public string? ItemCategory { get; set; }
    public string? ItemBarcode { get; set; }
    public string? PackageSize { get; set; }
    public string? StoreName { get; set; }
    public string? StoreChain { get; set; }
    public string? StoreSuburb { get; set; }
    public string? StoreState { get; set; }
    public decimal Price { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? SaleDescription { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Source { get; set; } = "p2p";
}
