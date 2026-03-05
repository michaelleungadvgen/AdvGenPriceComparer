using Microsoft.AspNetCore.SignalR;

namespace AdvGenPriceComparer.Server.Hubs;

/// <summary>
/// SignalR hub for real-time price updates
/// </summary>
public class PriceUpdateHub : Hub
{
    private readonly ILogger<PriceUpdateHub> _logger;

    public PriceUpdateHub(ILogger<PriceUpdateHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Subscribe to price updates for specific items
    /// </summary>
    public async Task SubscribeToItem(int itemId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"item_{itemId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to item {ItemId}", 
            Context.ConnectionId, itemId);
    }

    /// <summary>
    /// Unsubscribe from price updates for specific items
    /// </summary>
    public async Task UnsubscribeFromItem(int itemId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"item_{itemId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from item {ItemId}", 
            Context.ConnectionId, itemId);
    }

    /// <summary>
    /// Subscribe to all price updates for a specific store/place
    /// </summary>
    public async Task SubscribeToPlace(int placeId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"place_{placeId}");
        _logger.LogInformation("Connection {ConnectionId} subscribed to place {PlaceId}", 
            Context.ConnectionId, placeId);
    }

    /// <summary>
    /// Unsubscribe from price updates for a specific store/place
    /// </summary>
    public async Task UnsubscribeFromPlace(int placeId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"place_{placeId}");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from place {PlaceId}", 
            Context.ConnectionId, placeId);
    }

    /// <summary>
    /// Subscribe to all new deal notifications
    /// </summary>
    public async Task SubscribeToNewDeals()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "new_deals");
        _logger.LogInformation("Connection {ConnectionId} subscribed to new deals", 
            Context.ConnectionId);
    }

    /// <summary>
    /// Unsubscribe from new deal notifications
    /// </summary>
    public async Task UnsubscribeFromNewDeals()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, "new_deals");
        _logger.LogInformation("Connection {ConnectionId} unsubscribed from new deals", 
            Context.ConnectionId);
    }

    /// <summary>
    /// Called when a client connects
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Called when a client disconnects
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            _logger.LogWarning(exception, "Client {ConnectionId} disconnected with error", 
                Context.ConnectionId);
        }
        else
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        }
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// DTO for price update notifications
/// </summary>
public class PriceUpdateNotification
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int PlaceId { get; set; }
    public string PlaceName { get; set; } = string.Empty;
    public decimal NewPrice { get; set; }
    public decimal? OldPrice { get; set; }
    public DateTime UpdateTime { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? SalePrice { get; set; }
    public decimal? RegularPrice { get; set; }
}

/// <summary>
/// DTO for new deal notifications
/// </summary>
public class NewDealNotification
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
    public DateTime? DealEndDate { get; set; }
}
