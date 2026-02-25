using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for monitoring price changes and sending notifications when price drops occur
/// </summary>
public interface IPriceDropNotificationService
{
    /// <summary>
    /// Starts monitoring for price drops
    /// </summary>
    void StartMonitoring();

    /// <summary>
    /// Stops monitoring for price drops
    /// </summary>
    void StopMonitoring();

    /// <summary>
    /// Checks all active alerts immediately (useful for manual checks)
    /// </summary>
    Task CheckAllAlertsAsync();

    /// <summary>
    /// Checks alerts for a specific item when its price changes
    /// </summary>
    Task CheckPriceDropAsync(string itemId, decimal oldPrice, decimal newPrice, string? placeId = null);

    /// <summary>
    /// Creates a new price drop alert for an item
    /// </summary>
    Task<string> CreatePriceDropAlertAsync(string itemId, decimal? thresholdPercentage = null, decimal? thresholdPrice = null, string? alertName = null);

    /// <summary>
    /// Gets all triggered price drop notifications
    /// </summary>
    IEnumerable<AlertLogicEntity> GetTriggeredNotifications();

    /// <summary>
    /// Marks a notification as read
    /// </summary>
    Task MarkAsReadAsync(string alertId);

    /// <summary>
    /// Dismisses a notification
    /// </summary>
    Task DismissNotificationAsync(string alertId);

    /// <summary>
    /// Gets the count of unread price drop notifications
    /// </summary>
    int GetUnreadNotificationCount();

    /// <summary>
    /// Event raised when a price drop is detected
    /// </summary>
    event EventHandler<PriceDropEventArgs>? PriceDropDetected;

    /// <summary>
    /// Whether the monitoring service is currently active
    /// </summary>
    bool IsMonitoring { get; }
}

/// <summary>
/// Event arguments for price drop detection
/// </summary>
public class PriceDropEventArgs : EventArgs
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? PlaceName { get; set; }
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public decimal PriceChange { get; set; }
    public decimal ChangePercentage { get; set; }
    public AlertLogicEntity Alert { get; set; } = null!;
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
