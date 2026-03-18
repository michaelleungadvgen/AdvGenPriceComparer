using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Service for managing user-defined price alerts
/// </summary>
public interface IPriceAlertService
{
    /// <summary>
    /// Creates a new price alert
    /// </summary>
    Task<PriceAlert> CreateAlertAsync(string itemId, decimal targetPrice, PriceAlertCondition condition, string? placeId = null, string? alertName = null, DateTime? expiryDate = null);

    /// <summary>
    /// Updates an existing price alert
    /// </summary>
    Task<bool> UpdateAlertAsync(PriceAlert alert);

    /// <summary>
    /// Deletes a price alert by ID
    /// </summary>
    Task<bool> DeleteAlertAsync(string alertId);

    /// <summary>
    /// Gets a price alert by ID
    /// </summary>
    Task<PriceAlert?> GetAlertByIdAsync(string alertId);

    /// <summary>
    /// Gets all price alerts for a specific item
    /// </summary>
    Task<IEnumerable<PriceAlert>> GetAlertsByItemAsync(string itemId);

    /// <summary>
    /// Gets all price alerts for a specific store
    /// </summary>
    Task<IEnumerable<PriceAlert>> GetAlertsByStoreAsync(string placeId);

    /// <summary>
    /// Gets all active price alerts
    /// </summary>
    Task<IEnumerable<PriceAlert>> GetAllActiveAlertsAsync();

    /// <summary>
    /// Gets all price alerts (including triggered/expired)
    /// </summary>
    Task<IEnumerable<PriceAlert>> GetAllAlertsAsync();

    /// <summary>
    /// Gets triggered alerts that haven't been acknowledged
    /// </summary>
    Task<IEnumerable<PriceAlert>> GetTriggeredAlertsAsync();

    /// <summary>
    /// Checks if any alerts should trigger based on the current price
    /// </summary>
    Task<IEnumerable<PriceAlert>> CheckAlertsAsync(string itemId, decimal currentPrice, string? placeId = null);

    /// <summary>
    /// Acknowledges a triggered alert
    /// </summary>
    Task<bool> AcknowledgeAlertAsync(string alertId);

    /// <summary>
    /// Reactivates a triggered alert
    /// </summary>
    Task<bool> ReactivateAlertAsync(string alertId);

    /// <summary>
    /// Disables an active alert
    /// </summary>
    Task<bool> DisableAlertAsync(string alertId);

    /// <summary>
    /// Gets the count of active alerts
    /// </summary>
    Task<int> GetActiveAlertCountAsync();

    /// <summary>
    /// Gets the count of triggered alerts
    /// </summary>
    Task<int> GetTriggeredAlertCountAsync();

    /// <summary>
    /// Event raised when a price alert is triggered
    /// </summary>
    event EventHandler<PriceAlertTriggeredEventArgs>? AlertTriggered;
}

/// <summary>
/// Event arguments for price alert trigger
/// </summary>
public class PriceAlertTriggeredEventArgs : EventArgs
{
    public string AlertId { get; set; } = string.Empty;
    public string ItemId { get; set; } = string.Empty;
    public string? ItemName { get; set; }
    public string? PlaceName { get; set; }
    public decimal TargetPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public PriceAlertCondition Condition { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
    public string? AlertName { get; set; }

    /// <summary>
    /// Formatted message for the alert
    /// </summary>
    public string GetMessage()
    {
        var itemDisplay = string.IsNullOrEmpty(ItemName) ? "Item" : ItemName;
        var placeDisplay = string.IsNullOrEmpty(PlaceName) ? "" : $" at {PlaceName}";
        return $"🎯 Price Alert: {itemDisplay}{placeDisplay} is now ${CurrentPrice:F2} (target: ${TargetPrice:F2})";
    }
}
