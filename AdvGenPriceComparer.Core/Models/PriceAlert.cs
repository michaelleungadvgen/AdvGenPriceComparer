namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a user-defined price alert for a specific item
/// </summary>
public class PriceAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the grocery item being watched
    /// </summary>
    public required string ItemId { get; set; }

    /// <summary>
    /// Name of the item (cached for display)
    /// </summary>
    public string? ItemName { get; set; }

    /// <summary>
    /// ID of the supermarket place (optional - if null, watches all stores)
    /// </summary>
    public string? PlaceId { get; set; }

    /// <summary>
    /// Name of the place (cached for display)
    /// </summary>
    public string? PlaceName { get; set; }

    /// <summary>
    /// The target price that triggers the alert
    /// </summary>
    public required decimal TargetPrice { get; set; }

    /// <summary>
    /// Type of price alert condition
    /// </summary>
    public PriceAlertCondition Condition { get; set; } = PriceAlertCondition.BelowOrEqual;

    /// <summary>
    /// Current status of the alert
    /// </summary>
    public PriceAlertStatus Status { get; set; } = PriceAlertStatus.Active;

    /// <summary>
    /// When this alert was created
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this alert was last triggered (if ever)
    /// </summary>
    public DateTime? LastTriggered { get; set; }

    /// <summary>
    /// When this alert expires (optional)
    /// </summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>
    /// Whether this alert is currently active
    /// </summary>
    public bool IsActive => Status == PriceAlertStatus.Active && (ExpiryDate == null || ExpiryDate > DateTime.UtcNow);

    /// <summary>
    /// User-defined name/label for this alert
    /// </summary>
    public string? AlertName { get; set; }

    /// <summary>
    /// Additional notes about the alert
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether to notify via toast notification when triggered
    /// </summary>
    public bool EnableNotification { get; set; } = true;

    /// <summary>
    /// Number of times this alert has been triggered
    /// </summary>
    public int TriggerCount { get; set; }

    /// <summary>
    /// The price when the alert was last checked
    /// </summary>
    public decimal? LastCheckedPrice { get; set; }

    /// <summary>
    /// Checks if the alert should trigger based on the current price
    /// </summary>
    public bool ShouldTrigger(decimal currentPrice)
    {
        if (!IsActive) return false;

        return Condition switch
        {
            PriceAlertCondition.BelowOrEqual => currentPrice <= TargetPrice,
            PriceAlertCondition.Below => currentPrice < TargetPrice,
            PriceAlertCondition.Equal => Math.Abs(currentPrice - TargetPrice) < 0.01m,
            PriceAlertCondition.Above => currentPrice > TargetPrice,
            PriceAlertCondition.AboveOrEqual => currentPrice >= TargetPrice,
            _ => false
        };
    }

    /// <summary>
    /// Marks the alert as triggered
    /// </summary>
    public void Trigger(decimal currentPrice)
    {
        LastTriggered = DateTime.UtcNow;
        LastCheckedPrice = currentPrice;
        TriggerCount++;
        Status = PriceAlertStatus.Triggered;
    }

    /// <summary>
    /// Reactivates a triggered alert
    /// </summary>
    public void Reactivate()
    {
        Status = PriceAlertStatus.Active;
    }

    /// <summary>
    /// Marks the alert as expired
    /// </summary>
    public void Expire()
    {
        Status = PriceAlertStatus.Expired;
    }

    /// <summary>
    /// Generates a display message for this alert
    /// </summary>
    public string GetDisplayMessage()
    {
        var conditionText = Condition switch
        {
            PriceAlertCondition.BelowOrEqual => "drops to or below",
            PriceAlertCondition.Below => "drops below",
            PriceAlertCondition.Equal => "reaches",
            PriceAlertCondition.Above => "rises above",
            PriceAlertCondition.AboveOrEqual => "rises to or above",
            _ => "reaches"
        };

        var itemDisplay = string.IsNullOrEmpty(ItemName) ? "Item" : ItemName;
        var placeDisplay = string.IsNullOrEmpty(PlaceName) ? "" : $" at {PlaceName}";

        return $"Alert when {itemDisplay}{placeDisplay} price {conditionText} ${TargetPrice:F2}";
    }
}

/// <summary>
/// Condition for triggering a price alert
/// </summary>
public enum PriceAlertCondition
{
    /// <summary>
    /// Alert when price drops to or below target
    /// </summary>
    BelowOrEqual,

    /// <summary>
    /// Alert when price drops below target
    /// </summary>
    Below,

    /// <summary>
    /// Alert when price equals target
    /// </summary>
    Equal,

    /// <summary>
    /// Alert when price rises above target
    /// </summary>
    Above,

    /// <summary>
    /// Alert when price rises to or above target
    /// </summary>
    AboveOrEqual
}

/// <summary>
/// Status of a price alert
/// </summary>
public enum PriceAlertStatus
{
    /// <summary>
    /// Alert is active and monitoring
    /// </summary>
    Active,

    /// <summary>
    /// Alert has been triggered
    /// </summary>
    Triggered,

    /// <summary>
    /// Alert has expired
    /// </summary>
    Expired,

    /// <summary>
    /// Alert has been manually disabled
    /// </summary>
    Disabled
}
