using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service implementation for monitoring and notifying about price drops
/// </summary>
public class PriceDropNotificationService : IPriceDropNotificationService
{
    private readonly IGroceryDataService _groceryData;
    private readonly INotificationService _notificationService;
    private readonly ILoggerService _logger;
    private readonly System.Timers.Timer _checkTimer;
    private readonly object _lockObject = new();

    public bool IsMonitoring { get; private set; }

    public event EventHandler<PriceDropEventArgs>? PriceDropDetected;

    public PriceDropNotificationService(
        IGroceryDataService groceryData,
        INotificationService notificationService,
        ILoggerService logger)
    {
        _groceryData = groceryData ?? throw new ArgumentNullException(nameof(groceryData));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize timer for periodic checks (default: check every 30 minutes)
        _checkTimer = new System.Timers.Timer(TimeSpan.FromMinutes(30).TotalMilliseconds);
        _checkTimer.Elapsed += async (s, e) => await OnTimerElapsedAsync();
        _checkTimer.AutoReset = true;
    }

    /// <inheritdoc />
    public void StartMonitoring()
    {
        lock (_lockObject)
        {
            if (IsMonitoring) return;

            _logger.LogInfo("Starting price drop notification monitoring");
            _checkTimer.Start();
            IsMonitoring = true;
        }
    }

    /// <inheritdoc />
    public void StopMonitoring()
    {
        lock (_lockObject)
        {
            if (!IsMonitoring) return;

            _logger.LogInfo("Stopping price drop notification monitoring");
            _checkTimer.Stop();
            IsMonitoring = false;
        }
    }

    /// <inheritdoc />
    public async Task CheckAllAlertsAsync()
    {
        _logger.LogInfo("Checking all price drop alerts");

        try
        {
            var activeAlerts = _groceryData.Alerts.GetActiveAlerts()
                .Where(a => a.Type == AlertType.PriceDecrease || 
                            a.Type == AlertType.PriceChange || 
                            a.Type == AlertType.PriceThreshold)
                .ToList();

            foreach (var alert in activeAlerts)
            {
                await CheckAlertAsync(alert);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error checking all alerts", ex);
        }
    }

    /// <inheritdoc />
    public async Task CheckPriceDropAsync(string itemId, decimal oldPrice, decimal newPrice, string? placeId = null)
    {
        if (string.IsNullOrEmpty(itemId)) return;

        try
        {
            var alerts = _groceryData.Alerts.GetAlertsByItem(itemId)
                .Where(a => a.IsActive && 
                           (a.Type == AlertType.PriceDecrease || 
                            a.Type == AlertType.PriceChange ||
                            a.Type == AlertType.PriceThreshold))
                .ToList();

            // If placeId is specified, filter alerts for that place
            if (!string.IsNullOrEmpty(placeId))
            {
                alerts = alerts.Where(a => 
                    string.IsNullOrEmpty(a.PlaceId) || 
                    a.PlaceId == placeId).ToList();
            }

            foreach (var alert in alerts)
            {
                if (alert.ShouldTrigger(oldPrice, newPrice))
                {
                    await TriggerAlertAsync(alert, oldPrice, newPrice);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking price drop for item {itemId}", ex);
        }
    }

    /// <inheritdoc />
    public Task<string> CreatePriceDropAlertAsync(
        string itemId, 
        decimal? thresholdPercentage = null, 
        decimal? thresholdPrice = null,
        string? alertName = null)
    {
        var alert = new AlertLogicEntity
        {
            ItemId = itemId,
            Type = AlertType.PriceDecrease,
            ThresholdPercentage = thresholdPercentage,
            ThresholdPrice = thresholdPrice,
            AlertName = alertName ?? $"Price Drop Alert - {itemId}",
            Condition = AlertCondition.Below,
            IsActive = true,
            CheckFrequency = AlertFrequency.OnUpdate
        };

        var id = _groceryData.Alerts.Add(alert);
        _logger.LogInfo($"Created price drop alert {id} for item {itemId}");

        return Task.FromResult(id);
    }

    /// <inheritdoc />
    public IEnumerable<AlertLogicEntity> GetTriggeredNotifications()
    {
        return _groceryData.Alerts.GetTriggeredAlerts(DateTime.UtcNow.AddDays(-7))
            .Where(a => a.Type == AlertType.PriceDecrease || 
                        a.Type == AlertType.PriceChange || 
                        a.Type == AlertType.PriceThreshold)
            .OrderByDescending(a => a.LastTriggered)
            .ToList();
    }

    /// <inheritdoc />
    public Task MarkAsReadAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) return Task.CompletedTask;

        _groceryData.Alerts.MarkAsRead(alertId);
        _logger.LogInfo($"Marked alert {alertId} as read");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DismissNotificationAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId)) return Task.CompletedTask;

        _groceryData.Alerts.Dismiss(alertId);
        _logger.LogInfo($"Dismissed alert {alertId}");

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public int GetUnreadNotificationCount()
    {
        return _groceryData.Alerts.GetUnreadCount();
    }

    private async Task OnTimerElapsedAsync()
    {
        _logger.LogDebug("Periodic price drop check triggered");
        await CheckAllAlertsAsync();
    }

    private async Task CheckAlertAsync(AlertLogicEntity alert)
    {
        try
        {
            var latestPrice = _groceryData.PriceRecords.GetByItem(alert.ItemId)
                .OrderByDescending(p => p.DateRecorded)
                .FirstOrDefault();

            if (latestPrice == null) return;

            // Get previous price for comparison
            var previousPrice = _groceryData.PriceRecords.GetByItem(alert.ItemId)
                .Where(p => p.DateRecorded < latestPrice.DateRecorded)
                .OrderByDescending(p => p.DateRecorded)
                .FirstOrDefault();

            if (previousPrice == null) return;

            // Check if this is a new price change we haven't alerted on
            if (alert.LastTriggered.HasValue && 
                alert.LastTriggered.Value >= latestPrice.DateRecorded.ToUniversalTime())
            {
                return; // Already triggered for this price
            }

            if (alert.ShouldTrigger(previousPrice.Price, latestPrice.Price))
            {
                await TriggerAlertAsync(alert, previousPrice.Price, latestPrice.Price);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking alert {alert.Id}", ex);
        }
    }

    private async Task TriggerAlertAsync(AlertLogicEntity alert, decimal oldPrice, decimal newPrice)
    {
        try
        {
            // Update alert with trigger information
            alert.Trigger(newPrice, oldPrice);
            alert.BaselinePrice = oldPrice;
            _groceryData.Alerts.Update(alert);

            // Get item details
            var item = _groceryData.Items.GetById(alert.ItemId);
            var itemName = item?.Name ?? "Unknown Item";

            // Get place name if applicable
            string? placeName = null;
            if (!string.IsNullOrEmpty(alert.PlaceId))
            {
                var place = _groceryData.Places.GetById(alert.PlaceId);
                placeName = place?.Name;
            }

            // Generate notification message
            var message = alert.GenerateMessage(itemName, placeName);
            alert.Message = message;
            _groceryData.Alerts.Update(alert);

            // Show notification
            await _notificationService.ShowInfoAsync(message);

            // Raise event
            var change = newPrice - oldPrice;
            var changePercent = oldPrice > 0 ? (change / oldPrice) * 100 : 0;

            PriceDropDetected?.Invoke(this, new PriceDropEventArgs
            {
                ItemId = alert.ItemId,
                ItemName = itemName,
                PlaceName = placeName,
                OldPrice = oldPrice,
                NewPrice = newPrice,
                PriceChange = change,
                ChangePercentage = Math.Abs(changePercent),
                Alert = alert,
                Timestamp = DateTime.Now
            });

            _logger.LogInfo($"Price drop alert triggered: {message}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error triggering alert {alert.Id}", ex);
        }
    }

    /// <summary>
    /// Disposes the notification service
    /// </summary>
    public void Dispose()
    {
        StopMonitoring();
        _checkTimer?.Dispose();
    }
}
