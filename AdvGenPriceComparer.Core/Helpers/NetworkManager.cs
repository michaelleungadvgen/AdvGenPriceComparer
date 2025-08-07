using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Services;
using LiteDB;

namespace AdvGenPriceComparer.Core.Helpers;

public class NetworkManager : IDisposable
{
    private readonly ServerConfigService _serverConfig;
    private readonly IGroceryDataService _groceryData;
    private readonly List<NetworkPeer> _connectedPeers = new();
    private readonly Dictionary<string, TcpClient> _connections = new();
    private TcpListener? _server;
    private bool _isServerRunning = false;
    private bool _disposed = false;

    public event EventHandler<PriceShareMessage>? PriceReceived;
    public event EventHandler<NetworkPeer>? PeerConnected;
    public event EventHandler<NetworkPeer>? PeerDisconnected;
    public event EventHandler<string>? ErrorOccurred;

    public string NodeId { get; } = Environment.MachineName + "_" + Guid.NewGuid().ToString("N")[..8];
    public bool IsServerRunning => _isServerRunning;
    public IReadOnlyList<NetworkPeer> ConnectedPeers => _connectedPeers.AsReadOnly();

    public NetworkManager(IGroceryDataService groceryData, ServerConfigService? serverConfig = null)
    {
        _groceryData = groceryData;
        _serverConfig = serverConfig ?? new ServerConfigService();
    }

    #region Server Functionality

    public async Task<bool> StartServer(int port = 8081)
    {
        try
        {
            if (_isServerRunning) return true;

            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();
            _isServerRunning = true;

            System.Diagnostics.Debug.WriteLine($"P2P Price Server started on port {port}");
            
            // Accept connections in background
            _ = Task.Run(AcceptConnectionsAsync);
            
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to start server: {ex.Message}");
            return false;
        }
    }

    private async Task AcceptConnectionsAsync()
    {
        while (_isServerRunning && _server != null)
        {
            try
            {
                var tcpClient = await _server.AcceptTcpClientAsync();
                _ = Task.Run(() => HandleClientAsync(tcpClient));
            }
            catch (ObjectDisposedException)
            {
                // Server was stopped
                break;
            }
            catch (Exception ex)
            {
                ErrorOccurred?.Invoke(this, $"Error accepting connection: {ex.Message}");
            }
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "unknown";
        var peer = new NetworkPeer
        {
            Id = $"peer_{Guid.NewGuid():N}",
            Host = clientEndpoint.Split(':')[0],
            IsConnected = true,
            LastSeen = DateTime.UtcNow,
            Version = "1.0"
        };

        try
        {
            _connectedPeers.Add(peer);
            PeerConnected?.Invoke(this, peer);

            var stream = client.GetStream();
            var buffer = new byte[4096];

            while (client.Connected && _isServerRunning)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessIncomingMessage(message, peer);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Client error: {ex.Message}");
        }
        finally
        {
            peer.IsConnected = false;
            _connectedPeers.Remove(peer);
            PeerDisconnected?.Invoke(this, peer);
            client.Close();
        }
    }

    public void StopServer()
    {
        _isServerRunning = false;
        _server?.Stop();
        _server = null;
        
        // Disconnect all clients
        foreach (var connection in _connections.Values)
        {
            connection.Close();
        }
        _connections.Clear();
        _connectedPeers.Clear();
    }

    #endregion

    #region Client Functionality

    public async Task<bool> ConnectToServer(string serverName)
    {
        var server = _serverConfig.GetServerByName(serverName);
        if (server == null)
        {
            ErrorOccurred?.Invoke(this, $"Server '{serverName}' not found in configuration");
            return false;
        }

        return await ConnectToServer(server.Host!, server.Port);
    }

    public async Task<bool> ConnectToServer(string host, int port)
    {
        try
        {
            var key = $"{host}:{port}";
            if (_connections.ContainsKey(key))
            {
                return true; // Already connected
            }

            var client = new TcpClient();
            await client.ConnectAsync(host, port);
            
            _connections[key] = client;

            var peer = new NetworkPeer
            {
                Id = key,
                Host = host,
                Port = port,
                IsConnected = true,
                LastSeen = DateTime.UtcNow
            };

            _connectedPeers.Add(peer);
            PeerConnected?.Invoke(this, peer);

            // Handle incoming messages from this server
            _ = Task.Run(() => ListenToServerAsync(client, peer));

            System.Diagnostics.Debug.WriteLine($"Connected to price sharing server {host}:{port}");
            return true;
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Failed to connect to {host}:{port}: {ex.Message}");
            return false;
        }
    }

    private async Task ListenToServerAsync(TcpClient client, NetworkPeer peer)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];

            while (client.Connected)
            {
                var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await ProcessIncomingMessage(message, peer);
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Server connection error: {ex.Message}");
        }
        finally
        {
            peer.IsConnected = false;
            _connectedPeers.Remove(peer);
            PeerDisconnected?.Invoke(this, peer);
        }
    }

    public async Task DisconnectFromServer(string host, int port)
    {
        var key = $"{host}:{port}";
        if (_connections.TryGetValue(key, out var client))
        {
            client.Close();
            _connections.Remove(key);
            
            var peer = _connectedPeers.FirstOrDefault(p => p.Id == key);
            if (peer != null)
            {
                peer.IsConnected = false;
                _connectedPeers.Remove(peer);
                PeerDisconnected?.Invoke(this, peer);
            }
        }
    }

    #endregion

    #region Message Processing

    private async Task ProcessIncomingMessage(string messageJson, NetworkPeer sender)
    {
        try
        {
            var networkMessage = System.Text.Json.JsonSerializer.Deserialize<NetworkMessage>(messageJson);
            if (networkMessage == null) return;

            sender.LastSeen = DateTime.UtcNow;

            switch (networkMessage.Type)
            {
                case MessageType.PriceShare:
                    await HandlePriceShare(networkMessage.Data!, sender);
                    break;
                    
                case MessageType.SyncRequest:
                    await HandleSyncRequest(networkMessage.Data!, sender);
                    break;
                    
                case MessageType.Heartbeat:
                    // Just update last seen time (already done above)
                    break;
                    
                default:
                    System.Diagnostics.Debug.WriteLine($"Unknown message type: {networkMessage.Type}");
                    break;
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error processing message: {ex.Message}");
        }
    }

    private async Task HandlePriceShare(string data, NetworkPeer sender)
    {
        try
        {
            var priceMessage = System.Text.Json.JsonSerializer.Deserialize<PriceShareMessage>(data);
            if (priceMessage == null) return;

            // Store the received price in our local database
            await StorePriceFromNetwork(priceMessage);
            
            // Notify listeners
            PriceReceived?.Invoke(this, priceMessage);
            
            System.Diagnostics.Debug.WriteLine($"Received price: {priceMessage.ItemName} - ${priceMessage.Price} at {priceMessage.StoreName}");
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error handling price share: {ex.Message}");
        }
    }

    private async Task HandleSyncRequest(string data, NetworkPeer sender)
    {
        try
        {
            var syncRequest = System.Text.Json.JsonSerializer.Deserialize<SyncRequestMessage>(data);
            if (syncRequest == null) return;

            // Get recent prices to share
            var recentPrices = GetRecentPricesForSync(syncRequest);
            
            var response = new SyncResponseMessage
            {
                Prices = recentPrices.ToArray(),
                TotalCount = recentPrices.Count,
                SyncTime = DateTime.UtcNow
            };

            await SendMessageToPeer(sender, MessageType.SyncResponse, response);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error handling sync request: {ex.Message}");
        }
    }

    #endregion

    #region Price Sharing

    public async Task SharePrice(ObjectId itemId, ObjectId placeId, decimal price, 
        bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null)
    {
        try
        {
            var item = _groceryData.Items.GetById(itemId);
            var place = _groceryData.Places.GetById(placeId);

            if (item == null || place == null) return;

            var priceMessage = new PriceShareMessage
            {
                ItemName = item.Name,
                ItemBrand = item.Brand,
                ItemCategory = item.Category,
                ItemBarcode = item.Barcode,
                PackageSize = item.PackageSize,
                StoreName = place.Name,
                StoreChain = place.Chain,
                StoreSuburb = place.Suburb,
                StoreState = place.State,
                Price = price,
                IsOnSale = isOnSale,
                OriginalPrice = originalPrice,
                SaleDescription = saleDescription
            };

            // Share with all connected peers
            await BroadcastMessage(MessageType.PriceShare, priceMessage);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error sharing price: {ex.Message}");
        }
    }

    public async Task RequestPriceSync(string? region = null)
    {
        var syncRequest = new SyncRequestMessage
        {
            LastSyncTime = DateTime.UtcNow.AddDays(-7), // Get last week's data
            Region = region
        };

        await BroadcastMessage(MessageType.SyncRequest, syncRequest);
    }

    private async Task StorePriceFromNetwork(PriceShareMessage priceMessage)
    {
        try
        {
            // Find or create item
            var item = _groceryData.Items.GetAll()
                .FirstOrDefault(i => i.Name == priceMessage.ItemName && 
                                   i.Brand == priceMessage.ItemBrand);

            if (item == null)
            {
                var itemId = _groceryData.AddGroceryItem(
                    priceMessage.ItemName!,
                    priceMessage.ItemBrand,
                    priceMessage.ItemCategory,
                    priceMessage.ItemBarcode,
                    priceMessage.PackageSize
                );
                item = _groceryData.Items.GetById(itemId);
            }

            // Find or create place
            var place = _groceryData.Places.GetAll()
                .FirstOrDefault(p => p.Name == priceMessage.StoreName &&
                                   p.Chain == priceMessage.StoreChain);

            if (place == null)
            {
                var placeId = _groceryData.AddSupermarket(
                    priceMessage.StoreName!,
                    priceMessage.StoreChain!,
                    suburb: priceMessage.StoreSuburb,
                    state: priceMessage.StoreState
                );
                place = _groceryData.Places.GetById(placeId);
            }

            if (item != null && place != null)
            {
                // Record the price
                _groceryData.RecordPrice(
                    item.Id,
                    place.Id,
                    priceMessage.Price,
                    priceMessage.IsOnSale,
                    priceMessage.OriginalPrice,
                    priceMessage.SaleDescription,
                    priceMessage.ValidFrom,
                    priceMessage.ValidTo,
                    "p2p-network"
                );
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error storing network price: {ex.Message}");
        }
    }

    private List<PriceShareMessage> GetRecentPricesForSync(SyncRequestMessage request)
    {
        var prices = new List<PriceShareMessage>();
        
        try
        {
            var recentRecords = _groceryData.PriceRecords.GetRecentPriceUpdates(100);
            
            foreach (var record in recentRecords)
            {
                if (record.DateRecorded < request.LastSyncTime) continue;

                var item = _groceryData.Items.GetById(record.ItemId);
                var place = _groceryData.Places.GetById(record.PlaceId);

                if (item == null || place == null) continue;

                // Filter by region if specified
                if (!string.IsNullOrEmpty(request.Region) && 
                    !string.Equals(place.State, request.Region, StringComparison.OrdinalIgnoreCase))
                    continue;

                prices.Add(new PriceShareMessage
                {
                    ItemName = item.Name,
                    ItemBrand = item.Brand,
                    ItemCategory = item.Category,
                    ItemBarcode = item.Barcode,
                    PackageSize = item.PackageSize,
                    StoreName = place.Name,
                    StoreChain = place.Chain,
                    StoreSuburb = place.Suburb,
                    StoreState = place.State,
                    Price = record.Price,
                    IsOnSale = record.IsOnSale,
                    OriginalPrice = record.OriginalPrice,
                    SaleDescription = record.SaleDescription,
                    ValidFrom = record.ValidFrom,
                    ValidTo = record.ValidTo
                });
            }
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error getting sync prices: {ex.Message}");
        }

        return prices;
    }

    #endregion

    #region Communication

    private async Task BroadcastMessage<T>(MessageType type, T data)
    {
        var tasks = _connectedPeers.Where(p => p.IsConnected)
            .Select(peer => SendMessageToPeer(peer, type, data));
        
        await Task.WhenAll(tasks);
    }

    private async Task SendMessageToPeer<T>(NetworkPeer peer, MessageType type, T data)
    {
        try
        {
            var key = peer.Id;
            if (!_connections.TryGetValue(key!, out var client) || !client.Connected)
                return;

            var networkMessage = new NetworkMessage
            {
                Type = type,
                SenderId = NodeId,
                Data = System.Text.Json.JsonSerializer.Serialize(data)
            };

            var json = System.Text.Json.JsonSerializer.Serialize(networkMessage);
            var bytes = Encoding.UTF8.GetBytes(json);

            var stream = client.GetStream();
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }
        catch (Exception ex)
        {
            ErrorOccurred?.Invoke(this, $"Error sending message to peer: {ex.Message}");
        }
    }

    #endregion

    #region Discovery and Health

    public async Task DiscoverAndConnectToServers(string? region = null)
    {
        var servers = string.IsNullOrEmpty(region) 
            ? _serverConfig.GetActiveServers()
            : _serverConfig.GetServersByRegion(region);

        var connectionTasks = servers.Select(async server =>
        {
            var connected = await ConnectToServer(server.Host!, server.Port);
            if (connected)
            {
                System.Diagnostics.Debug.WriteLine($"Connected to {server.Name}");
            }
        });

        await Task.WhenAll(connectionTasks);
    }

    public async Task SendHeartbeat()
    {
        await BroadcastMessage(MessageType.Heartbeat, new { timestamp = DateTime.UtcNow });
    }

    #endregion

    public void Dispose()
    {
        if (!_disposed)
        {
            StopServer();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
