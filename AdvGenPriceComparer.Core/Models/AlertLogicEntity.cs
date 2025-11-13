namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents an alert for price changes on watched grocery items
/// </summary>
public class AlertLogicEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// ID of the grocery item being watched
    /// </summary>
    public required string ItemId { get; set; }

    /// <summary>
    /// ID of the supermarket place (optional - if null, watches all stores)
    /// </summary>
    public string? PlaceId { get; set; }

    /// <summary>
    /// Type of alert trigger
    /// </summary>
    public AlertType Type { get; set; } = AlertType.PriceChange;

    /// <summary>
    /// Threshold for price change alerts (e.g., 10% decrease)
    /// </summary>
    public decimal? ThresholdPercentage { get; set; }

    /// <summary>
    /// Absolute price threshold (e.g., alert when price drops below $5.00)
    /// </summary>
    public decimal? ThresholdPrice { get; set; }

    /// <summary>
    /// Condition for triggering alert
    /// </summary>
    public AlertCondition Condition { get; set; } = AlertCondition.Any;

    /// <summary>
    /// When this alert was created
    /// </summary>
    public DateTime DateCreated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last time this alert was triggered
    /// </summary>
    public DateTime? LastTriggered { get; set; }

    /// <summary>
    /// Price when alert was created (for comparison)
    /// </summary>
    public decimal? BaselinePrice { get; set; }

    /// <summary>
    /// Current price that triggered the alert
    /// </summary>
    public decimal? CurrentPrice { get; set; }

    /// <summary>
    /// Previous price before the change
    /// </summary>
    public decimal? PreviousPrice { get; set; }

    /// <summary>
    /// Whether this alert is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this alert has been read/acknowledged
    /// </summary>
    public bool IsRead { get; set; } = false;

    /// <summary>
    /// Whether this alert has been dismissed
    /// </summary>
    public bool IsDismissed { get; set; } = false;

    /// <summary>
    /// Alert message/description
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Additional notes about the alert
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Frequency for checking this alert
    /// </summary>
    public AlertFrequency CheckFrequency { get; set; } = AlertFrequency.OnUpdate;

    /// <summary>
    /// User-defined name/label for this alert
    /// </summary>
    public string? AlertName { get; set; }

    /// <summary>
    /// Checks if this alert should trigger based on price change
    /// </summary>
    public bool ShouldTrigger(decimal oldPrice, decimal newPrice)
    {
        if (!IsActive) return false;

        return Type switch
        {
            AlertType.PriceIncrease => CheckPriceIncrease(oldPrice, newPrice),
            AlertType.PriceDecrease => CheckPriceDecrease(oldPrice, newPrice),
            AlertType.PriceChange => oldPrice != newPrice,
            AlertType.PriceThreshold => CheckPriceThreshold(newPrice),
            AlertType.BackInStock => true, // Handled separately
            AlertType.OnSale => true, // Handled separately
            _ => false
        };
    }

    /// <summary>
    /// Generates alert message based on price change
    /// </summary>
    public string GenerateMessage(string itemName, string? placeName = null)
    {
        var location = placeName != null ? $" at {placeName}" : "";

        if (CurrentPrice.HasValue && PreviousPrice.HasValue)
        {
            var change = CurrentPrice.Value - PreviousPrice.Value;
            var changePercent = Math.Round((change / PreviousPrice.Value) * 100, 1);

            if (change < 0)
            {
                return $"{itemName}{location} price dropped by ${Math.Abs(change):F2} ({Math.Abs(changePercent):F1}%) to ${CurrentPrice.Value:F2}";
            }
            else
            {
                return $"{itemName}{location} price increased by ${change:F2} ({changePercent:F1}%) to ${CurrentPrice.Value:F2}";
            }
        }

        if (CurrentPrice.HasValue)
        {
            return $"{itemName}{location} is now ${CurrentPrice.Value:F2}";
        }

        return $"Alert for {itemName}{location}";
    }

    /// <summary>
    /// Marks the alert as triggered
    /// </summary>
    public void Trigger(decimal currentPrice, decimal? previousPrice = null)
    {
        LastTriggered = DateTime.UtcNow;
        CurrentPrice = currentPrice;
        PreviousPrice = previousPrice;
        IsRead = false;
    }

    /// <summary>
    /// Marks the alert as read
    /// </summary>
    public void MarkAsRead()
    {
        IsRead = true;
    }

    /// <summary>
    /// Dismisses the alert
    /// </summary>
    public void Dismiss()
    {
        IsDismissed = true;
        IsRead = true;
    }

    private bool CheckPriceIncrease(decimal oldPrice, decimal newPrice)
    {
        if (newPrice <= oldPrice) return false;

        if (ThresholdPercentage.HasValue)
        {
            var changePercent = ((newPrice - oldPrice) / oldPrice) * 100;
            return changePercent >= ThresholdPercentage.Value;
        }

        return true;
    }

    private bool CheckPriceDecrease(decimal oldPrice, decimal newPrice)
    {
        if (newPrice >= oldPrice) return false;

        if (ThresholdPercentage.HasValue)
        {
            var changePercent = ((oldPrice - newPrice) / oldPrice) * 100;
            return changePercent >= ThresholdPercentage.Value;
        }

        return true;
    }

    private bool CheckPriceThreshold(decimal newPrice)
    {
        if (!ThresholdPrice.HasValue) return false;

        return Condition switch
        {
            AlertCondition.Above => newPrice > ThresholdPrice.Value,
            AlertCondition.Below => newPrice < ThresholdPrice.Value,
            AlertCondition.Equals => Math.Abs(newPrice - ThresholdPrice.Value) < 0.01m,
            _ => false
        };
    }
}

/// <summary>
/// Type of alert
/// </summary>
public enum AlertType
{
    PriceChange,      // Any price change
    PriceIncrease,    // Price went up
    PriceDecrease,    // Price went down
    PriceThreshold,   // Price crosses threshold
    BackInStock,      // Item available again
    OnSale            // Item goes on sale
}

/// <summary>
/// Condition for threshold alerts
/// </summary>
public enum AlertCondition
{
    Any,      // Any change
    Above,    // Price above threshold
    Below,    // Price below threshold
    Equals    // Price equals threshold
}

/// <summary>
/// How often to check for alerts
/// </summary>
public enum AlertFrequency
{
    OnUpdate,     // Check whenever data updates
    Daily,        // Check once per day
    Weekly,       // Check once per week
    Manual        // Only check when user requests
}
