using AdvGenPriceComparer.Server.Hubs;
using AdvGenPriceComparer.Server.Models;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Interface for sending real-time notifications to clients
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Notify clients about a price update for a specific item
    /// </summary>
    Task NotifyPriceUpdateAsync(PriceUpdateNotification notification);

    /// <summary>
    /// Notify clients about a new deal
    /// </summary>
    Task NotifyNewDealAsync(NewDealNotification notification);

    /// <summary>
    /// Notify clients that new data has been uploaded
    /// </summary>
    Task NotifyDataUploadedAsync(int itemsUploaded, int placesUploaded, int pricesUploaded);

    /// <summary>
    /// Broadcast a message to all connected clients
    /// </summary>
    Task BroadcastMessageAsync(string message);
}
