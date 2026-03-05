using AdvGenPriceComparer.Server.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// SignalR implementation of the notification service
/// </summary>
public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<PriceUpdateHub> _hubContext;
    private readonly ILogger<SignalRNotificationService> _logger;

    public SignalRNotificationService(
        IHubContext<PriceUpdateHub> hubContext,
        ILogger<SignalRNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Notify clients about a price update for a specific item
    /// </summary>
    public async Task NotifyPriceUpdateAsync(PriceUpdateNotification notification)
    {
        try
        {
            // Send to item-specific group
            await _hubContext.Clients.Group($"item_{notification.ItemId}")
                .SendAsync("PriceUpdated", notification);

            // Send to place-specific group
            await _hubContext.Clients.Group($"place_{notification.PlaceId}")
                .SendAsync("PriceUpdated", notification);

            _logger.LogInformation(
                "Price update notification sent for item {ItemId} at place {PlaceId}: ${Price}",
                notification.ItemId, notification.PlaceId, notification.NewPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send price update notification");
        }
    }

    /// <summary>
    /// Notify clients about a new deal
    /// </summary>
    public async Task NotifyNewDealAsync(NewDealNotification notification)
    {
        try
        {
            // Send to new deals group
            await _hubContext.Clients.Group("new_deals")
                .SendAsync("NewDeal", notification);

            // Also send to item-specific group
            await _hubContext.Clients.Group($"item_{notification.ItemId}")
                .SendAsync("NewDeal", notification);

            // And place-specific group
            await _hubContext.Clients.Group($"place_{notification.PlaceId}")
                .SendAsync("NewDeal", notification);

            _logger.LogInformation(
                "New deal notification sent for item {ItemName} at {PlaceName}: ${Price}",
                notification.ItemName, notification.PlaceName, notification.Price);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send new deal notification");
        }
    }

    /// <summary>
    /// Notify clients that new data has been uploaded
    /// </summary>
    public async Task NotifyDataUploadedAsync(int itemsUploaded, int placesUploaded, int pricesUploaded)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("DataUploaded", new
            {
                ItemsUploaded = itemsUploaded,
                PlacesUploaded = placesUploaded,
                PricesUploaded = pricesUploaded,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Data upload notification sent: {Items} items, {Places} places, {Prices} prices",
                itemsUploaded, placesUploaded, pricesUploaded);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send data upload notification");
        }
    }

    /// <summary>
    /// Broadcast a message to all connected clients
    /// </summary>
    public async Task BroadcastMessageAsync(string message)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("BroadcastMessage", new
            {
                Message = message,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Broadcast message sent: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast message");
        }
    }
}
