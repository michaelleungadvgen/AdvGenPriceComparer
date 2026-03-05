using System;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Interface for the SignalR client service that connects to real-time price updates
/// </summary>
public interface IPriceUpdateClientService
{
    /// <summary>
    /// Event raised when a price update is received from the server
    /// </summary>
    event EventHandler<PriceUpdateEventArgs>? PriceUpdated;

    /// <summary>
    /// Event raised when a new deal notification is received
    /// </summary>
    event EventHandler<NewDealEventArgs>? NewDealReceived;

    /// <summary>
    /// Event raised when data is uploaded by another client
    /// </summary>
    event EventHandler<DataUploadedEventArgs>? DataUploaded;

    /// <summary>
    /// Event raised when connection status changes
    /// </summary>
    event EventHandler<ConnectionStatusEventArgs>? ConnectionStatusChanged;

    /// <summary>
    /// Gets whether the client is currently connected to the server
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the server URL
    /// </summary>
    string ServerUrl { get; }

    /// <summary>
    /// Connect to the SignalR hub
    /// </summary>
    Task ConnectAsync(string serverUrl, string? apiKey = null);

    /// <summary>
    /// Disconnect from the SignalR hub
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// Subscribe to price updates for a specific item
    /// </summary>
    Task SubscribeToItemAsync(int itemId);

    /// <summary>
    /// Unsubscribe from price updates for a specific item
    /// </summary>
    Task UnsubscribeFromItemAsync(int itemId);

    /// <summary>
    /// Subscribe to price updates for a specific store/place
    /// </summary>
    Task SubscribeToPlaceAsync(int placeId);

    /// <summary>
    /// Unsubscribe from price updates for a specific store/place
    /// </summary>
    Task UnsubscribeFromPlaceAsync(int placeId);

    /// <summary>
    /// Subscribe to new deal notifications
    /// </summary>
    Task SubscribeToNewDealsAsync();

    /// <summary>
    /// Unsubscribe from new deal notifications
    /// </summary>
    Task UnsubscribeFromNewDealsAsync();
}

/// <summary>
/// Event args for price update events
/// </summary>
public class PriceUpdateEventArgs : EventArgs
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int PlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public decimal NewPrice { get; set; }
    public decimal? OldPrice { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool IsOnSale { get; set; }
}

/// <summary>
/// Event args for new deal notifications
/// </summary>
public class NewDealEventArgs : EventArgs
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public int PlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? Savings { get; set; }
    public DateTime DealStartDate { get; set; }
}

/// <summary>
/// Event args for data uploaded notifications
/// </summary>
public class DataUploadedEventArgs : EventArgs
{
    public int ItemsUploaded { get; set; }
    public int PlacesUploaded { get; set; }
    public int PricesUploaded { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Event args for connection status changes
/// </summary>
public class ConnectionStatusEventArgs : EventArgs
{
    public bool IsConnected { get; set; }
    public string? Message { get; set; }
    public Exception? Error { get; set; }
}
