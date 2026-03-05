using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// SignalR client service for real-time price updates
/// </summary>
public class PriceUpdateClientService : IPriceUpdateClientService, IDisposable
{
    private HubConnection? _hubConnection;
    private readonly ILoggerService _logger;
    private bool _isConnected;

    public event EventHandler<PriceUpdateEventArgs>? PriceUpdated;
    public event EventHandler<NewDealEventArgs>? NewDealReceived;
    public event EventHandler<DataUploadedEventArgs>? DataUploaded;
    public event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    public bool IsConnected => _isConnected && _hubConnection?.State == HubConnectionState.Connected;
    public string ServerUrl { get; private set; } = string.Empty;

    public PriceUpdateClientService(ILoggerService logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Connect to the SignalR hub
    /// </summary>
    public async Task ConnectAsync(string serverUrl, string? apiKey = null)
    {
        try
        {
            ServerUrl = serverUrl;

            // Build the connection
            var connectionBuilder = new HubConnectionBuilder()
                .WithUrl($"{serverUrl}/hubs/price-updates", options =>
                {
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        options.Headers.Add("X-API-Key", apiKey);
                    }
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .ConfigureLogging(logging =>
                {
                    // We could integrate with our logger here if needed
                });

            _hubConnection = connectionBuilder.Build();

            // Set up event handlers
            SetupEventHandlers();

            // Start the connection
            await _hubConnection.StartAsync();
            _isConnected = true;

            _logger.LogInfo($"Connected to SignalR hub at {serverUrl}");
            OnConnectionStatusChanged(true, "Connected successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to connect to SignalR hub at {serverUrl}", ex);
            _isConnected = false;
            OnConnectionStatusChanged(false, "Connection failed", ex);
            throw;
        }
    }

    /// <summary>
    /// Disconnect from the SignalR hub
    /// </summary>
    public async Task DisconnectAsync()
    {
        try
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                _isConnected = false;
                _logger.LogInfo("Disconnected from SignalR hub");
                OnConnectionStatusChanged(false, "Disconnected");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error disconnecting from SignalR hub", ex);
            throw;
        }
    }

    /// <summary>
    /// Subscribe to price updates for a specific item
    /// </summary>
    public async Task SubscribeToItemAsync(int itemId)
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("SubscribeToItem", itemId);
        _logger.LogInfo($"Subscribed to price updates for item {itemId}");
    }

    /// <summary>
    /// Unsubscribe from price updates for a specific item
    /// </summary>
    public async Task UnsubscribeFromItemAsync(int itemId)
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("UnsubscribeFromItem", itemId);
        _logger.LogInfo($"Unsubscribed from price updates for item {itemId}");
    }

    /// <summary>
    /// Subscribe to price updates for a specific store/place
    /// </summary>
    public async Task SubscribeToPlaceAsync(int placeId)
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("SubscribeToPlace", placeId);
        _logger.LogInfo($"Subscribed to price updates for place {placeId}");
    }

    /// <summary>
    /// Unsubscribe from price updates for a specific store/place
    /// </summary>
    public async Task UnsubscribeFromPlaceAsync(int placeId)
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("UnsubscribeFromPlace", placeId);
        _logger.LogInfo($"Unsubscribed from price updates for place {placeId}");
    }

    /// <summary>
    /// Subscribe to new deal notifications
    /// </summary>
    public async Task SubscribeToNewDealsAsync()
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("SubscribeToNewDeals");
        _logger.LogInfo("Subscribed to new deal notifications");
    }

    /// <summary>
    /// Unsubscribe from new deal notifications
    /// </summary>
    public async Task UnsubscribeFromNewDealsAsync()
    {
        if (_hubConnection == null || !IsConnected)
        {
            throw new InvalidOperationException("Not connected to the hub");
        }

        await _hubConnection.InvokeAsync("UnsubscribeFromNewDeals");
        _logger.LogInfo("Unsubscribed from new deal notifications");
    }

    /// <summary>
    /// Set up SignalR event handlers
    /// </summary>
    private void SetupEventHandlers()
    {
        if (_hubConnection == null) return;

        // Handle price updates
        _hubConnection.On<dynamic>("PriceUpdated", data =>
        {
            try
            {
                var args = new PriceUpdateEventArgs
                {
                    ItemId = data.itemId,
                    ItemName = data.itemName,
                    PlaceId = data.placeId,
                    PlaceName = data.placeName,
                    NewPrice = data.newPrice,
                    OldPrice = data.oldPrice,
                    UpdateTime = data.updateTime,
                    IsOnSale = data.isOnSale
                };

                PriceUpdated?.Invoke(this, args);
                _logger.LogInfo($"Received price update for {args.ItemName} at {args.PlaceName}: ${args.NewPrice}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error handling price update", ex);
            }
        });

        // Handle new deals
        _hubConnection.On<dynamic>("NewDeal", data =>
        {
            try
            {
                var args = new NewDealEventArgs
                {
                    ItemId = data.itemId,
                    ItemName = data.itemName,
                    Brand = data.brand,
                    Category = data.category,
                    PlaceId = data.placeId,
                    PlaceName = data.placeName,
                    Price = data.price,
                    OriginalPrice = data.originalPrice,
                    Savings = data.savings,
                    DealStartDate = data.dealStartDate
                };

                NewDealReceived?.Invoke(this, args);
                _logger.LogInfo($"Received new deal notification for {args.ItemName} at {args.PlaceName}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error handling new deal notification", ex);
            }
        });

        // Handle data uploaded
        _hubConnection.On<dynamic>("DataUploaded", data =>
        {
            try
            {
                var args = new DataUploadedEventArgs
                {
                    ItemsUploaded = data.itemsUploaded,
                    PlacesUploaded = data.placesUploaded,
                    PricesUploaded = data.pricesUploaded,
                    Timestamp = data.timestamp
                };

                DataUploaded?.Invoke(this, args);
                _logger.LogInfo($"Received data upload notification: {args.ItemsUploaded} items, {args.PlacesUploaded} places, {args.PricesUploaded} prices");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error handling data uploaded notification", ex);
            }
        });

        // Handle reconnection
        _hubConnection.Reconnecting += error =>
        {
            _isConnected = false;
            OnConnectionStatusChanged(false, "Reconnecting...", error);
            _logger.LogWarning("SignalR connection lost, reconnecting...");
            return Task.CompletedTask;
        };

        _hubConnection.Reconnected += connectionId =>
        {
            _isConnected = true;
            OnConnectionStatusChanged(true, "Reconnected successfully");
            _logger.LogInfo($"SignalR connection restored. ConnectionId: {connectionId}");
            return Task.CompletedTask;
        };

        _hubConnection.Closed += error =>
        {
            _isConnected = false;
            OnConnectionStatusChanged(false, "Connection closed", error);
            _logger.LogWarning("SignalR connection closed");
            return Task.CompletedTask;
        };
    }

    /// <summary>
    /// Raise the ConnectionStatusChanged event
    /// </summary>
    private void OnConnectionStatusChanged(bool isConnected, string message, Exception? error = null)
    {
        ConnectionStatusChanged?.Invoke(this, new ConnectionStatusEventArgs
        {
            IsConnected = isConnected,
            Message = message,
            Error = error
        });
    }

    /// <summary>
    /// Dispose the service
    /// </summary>
    public void Dispose()
    {
        if (_hubConnection != null)
        {
            _hubConnection.StopAsync().GetAwaiter().GetResult();
            _hubConnection.DisposeAsync().GetAwaiter().GetResult();
            _hubConnection = null;
        }
    }
}
