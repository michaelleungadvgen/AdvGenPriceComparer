using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Services;
using LiteDB;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for managing user-defined price alerts
/// </summary>
public class PriceAlertService : IPriceAlertService
{
    private readonly DatabaseService _databaseService;
    private readonly INotificationService _notificationService;
    private readonly ILoggerService _logger;
    private readonly IGroceryDataService _groceryData;

    public event EventHandler<PriceAlertTriggeredEventArgs>? AlertTriggered;

    public PriceAlertService(
        DatabaseService databaseService,
        INotificationService notificationService,
        ILoggerService logger,
        IGroceryDataService groceryData)
    {
        _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _groceryData = groceryData ?? throw new ArgumentNullException(nameof(groceryData));
    }

    /// <inheritdoc />
    public async Task<PriceAlert> CreateAlertAsync(
        string itemId,
        decimal targetPrice,
        PriceAlertCondition condition,
        string? placeId = null,
        string? alertName = null,
        DateTime? expiryDate = null)
    {
        if (string.IsNullOrEmpty(itemId))
            throw new ArgumentException("Item ID is required", nameof(itemId));

        // Get item and place names for display
        var item = _groceryData.Items.GetById(itemId);
        var place = placeId != null ? _groceryData.Places.GetById(placeId) : null;

        var alert = new PriceAlert
        {
            ItemId = itemId,
            ItemName = item?.Name,
            PlaceId = placeId,
            PlaceName = place?.Name,
            TargetPrice = targetPrice,
            Condition = condition,
            AlertName = alertName,
            ExpiryDate = expiryDate?.ToUniversalTime(),
            Status = PriceAlertStatus.Active,
            DateCreated = DateTime.UtcNow
        };

        var entity = PriceAlertEntity.FromPriceAlert(alert);
        _databaseService.PriceAlerts.Insert(entity);
        alert.Id = entity.Id.ToString();

        _logger.LogInfo($"Created price alert {alert.Id} for item {itemId} with target ${targetPrice:F2}");

        return await Task.FromResult(alert);
    }

    /// <inheritdoc />
    public Task<bool> UpdateAlertAsync(PriceAlert alert)
    {
        if (alert == null || string.IsNullOrEmpty(alert.Id))
            return Task.FromResult(false);

        try
        {
            var entity = PriceAlertEntity.FromPriceAlert(alert);
            var existing = _databaseService.PriceAlerts.FindById(entity.Id);
            
            if (existing == null)
            {
                _logger.LogWarning($"Price alert {alert.Id} not found for update");
                return Task.FromResult(false);
            }

            _databaseService.PriceAlerts.Update(entity);
            _logger.LogInfo($"Updated price alert {alert.Id}");
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating price alert {alert.Id}", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAlertAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId))
            return Task.FromResult(false);

        try
        {
            var objectId = new ObjectId(alertId);
            var result = _databaseService.PriceAlerts.Delete(objectId);
            
            if (result)
            {
                _logger.LogInfo($"Deleted price alert {alertId}");
            }
            
            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting price alert {alertId}", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<PriceAlert?> GetAlertByIdAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId))
            return Task.FromResult<PriceAlert?>(null);

        try
        {
            var objectId = new ObjectId(alertId);
            var entity = _databaseService.PriceAlerts.FindById(objectId);
            
            if (entity == null)
                return Task.FromResult<PriceAlert?>(null);

            // Update item/place names from current data
            var alert = entity.ToPriceAlert();
            UpdateCachedNames(alert);
            
            return Task.FromResult<PriceAlert?>(alert);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price alert {alertId}", ex);
            return Task.FromResult<PriceAlert?>(null);
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PriceAlert>> GetAlertsByItemAsync(string itemId)
    {
        if (string.IsNullOrEmpty(itemId))
            return Task.FromResult(Enumerable.Empty<PriceAlert>());

        try
        {
            var objectId = new ObjectId(itemId);
            var entities = _databaseService.PriceAlerts
                .Find(x => x.ItemId == objectId)
                .ToList();

            var alerts = entities.Select(e => {
                var alert = e.ToPriceAlert();
                UpdateCachedNames(alert);
                return alert;
            });

            return Task.FromResult(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price alerts for item {itemId}", ex);
            return Task.FromResult(Enumerable.Empty<PriceAlert>());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PriceAlert>> GetAlertsByStoreAsync(string placeId)
    {
        if (string.IsNullOrEmpty(placeId))
            return Task.FromResult(Enumerable.Empty<PriceAlert>());

        try
        {
            var objectId = new ObjectId(placeId);
            var entities = _databaseService.PriceAlerts
                .Find(x => x.PlaceId == objectId)
                .ToList();

            var alerts = entities.Select(e => {
                var alert = e.ToPriceAlert();
                UpdateCachedNames(alert);
                return alert;
            });

            return Task.FromResult(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price alerts for place {placeId}", ex);
            return Task.FromResult(Enumerable.Empty<PriceAlert>());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PriceAlert>> GetAllActiveAlertsAsync()
    {
        try
        {
            var entities = _databaseService.PriceAlerts
                .Find(x => x.Status == PriceAlertStatus.Active &&
                          (x.ExpiryDate == null || x.ExpiryDate > DateTime.UtcNow))
                .ToList();

            var alerts = entities.Select(e => {
                var alert = e.ToPriceAlert();
                UpdateCachedNames(alert);
                return alert;
            });

            return Task.FromResult(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting active price alerts", ex);
            return Task.FromResult(Enumerable.Empty<PriceAlert>());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PriceAlert>> GetAllAlertsAsync()
    {
        try
        {
            var entities = _databaseService.PriceAlerts
                .FindAll()
                .OrderByDescending(x => x.DateCreated)
                .ToList();

            var alerts = entities.Select(e => {
                var alert = e.ToPriceAlert();
                UpdateCachedNames(alert);
                return alert;
            });

            return Task.FromResult(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting all price alerts", ex);
            return Task.FromResult(Enumerable.Empty<PriceAlert>());
        }
    }

    /// <inheritdoc />
    public Task<IEnumerable<PriceAlert>> GetTriggeredAlertsAsync()
    {
        try
        {
            var entities = _databaseService.PriceAlerts
                .Find(x => x.Status == PriceAlertStatus.Triggered)
                .OrderByDescending(x => x.LastTriggered)
                .ToList();

            var alerts = entities.Select(e => {
                var alert = e.ToPriceAlert();
                UpdateCachedNames(alert);
                return alert;
            });

            return Task.FromResult(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting triggered price alerts", ex);
            return Task.FromResult(Enumerable.Empty<PriceAlert>());
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<PriceAlert>> CheckAlertsAsync(string itemId, decimal currentPrice, string? placeId = null)
    {
        if (string.IsNullOrEmpty(itemId))
            return Enumerable.Empty<PriceAlert>();

        var triggeredAlerts = new List<PriceAlert>();

        try
        {
            var objectId = new ObjectId(itemId);
            
            // Get alerts for this item (and optionally for this specific place)
            var query = _databaseService.PriceAlerts
                .Find(x => x.ItemId == objectId && x.Status == PriceAlertStatus.Active);

            if (!string.IsNullOrEmpty(placeId))
            {
                var placeObjectId = new ObjectId(placeId);
                query = query.Where(x => x.PlaceId == null || x.PlaceId == placeObjectId);
            }

            var alerts = query.ToList();

            foreach (var entity in alerts)
            {
                var alert = entity.ToPriceAlert();
                UpdateCachedNames(alert);

                // Check if alert should trigger
                if (alert.ShouldTrigger(currentPrice))
                {
                    alert.Trigger(currentPrice);
                    
                    // Update entity
                    entity.Status = alert.Status;
                    entity.LastTriggered = alert.LastTriggered;
                    entity.LastCheckedPrice = alert.LastCheckedPrice;
                    entity.TriggerCount = alert.TriggerCount;
                    _databaseService.PriceAlerts.Update(entity);

                    triggeredAlerts.Add(alert);

                    // Show notification
                    if (alert.EnableNotification)
                    {
                        var message = alert.GetDisplayMessage();
                        await _notificationService.ShowInfoAsync(message);
                    }

                    // Raise event
                    AlertTriggered?.Invoke(this, new PriceAlertTriggeredEventArgs
                    {
                        AlertId = alert.Id,
                        ItemId = alert.ItemId,
                        ItemName = alert.ItemName,
                        PlaceName = alert.PlaceName,
                        TargetPrice = alert.TargetPrice,
                        CurrentPrice = currentPrice,
                        Condition = alert.Condition,
                        Timestamp = DateTime.Now,
                        AlertName = alert.AlertName
                    });

                    _logger.LogInfo($"Price alert triggered: {alert.GetDisplayMessage()}");
                }
                else
                {
                    // Just update the last checked price
                    entity.LastCheckedPrice = currentPrice;
                    _databaseService.PriceAlerts.Update(entity);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking price alerts for item {itemId}", ex);
        }

        return triggeredAlerts;
    }

    /// <inheritdoc />
    public async Task<bool> AcknowledgeAlertAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId))
            return false;

        var alert = await GetAlertByIdAsync(alertId);
        if (alert == null)
            return false;

        // For triggered alerts, we can optionally disable them or keep them active
        // Here we'll keep the status as triggered but user has acknowledged it
        _logger.LogInfo($"Price alert {alertId} acknowledged");
        return true;
    }

    /// <inheritdoc />
    public async Task<bool> ReactivateAlertAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId))
            return false;

        var alert = await GetAlertByIdAsync(alertId);
        if (alert == null)
            return false;

        alert.Reactivate();
        return await UpdateAlertAsync(alert);
    }

    /// <inheritdoc />
    public async Task<bool> DisableAlertAsync(string alertId)
    {
        if (string.IsNullOrEmpty(alertId))
            return false;

        var alert = await GetAlertByIdAsync(alertId);
        if (alert == null)
            return false;

        alert.Status = PriceAlertStatus.Disabled;
        return await UpdateAlertAsync(alert);
    }

    /// <inheritdoc />
    public Task<int> GetActiveAlertCountAsync()
    {
        try
        {
            var count = _databaseService.PriceAlerts
                .Count(x => x.Status == PriceAlertStatus.Active &&
                           (x.ExpiryDate == null || x.ExpiryDate > DateTime.UtcNow));
            return Task.FromResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting active alert count", ex);
            return Task.FromResult(0);
        }
    }

    /// <inheritdoc />
    public Task<int> GetTriggeredAlertCountAsync()
    {
        try
        {
            var count = _databaseService.PriceAlerts
                .Count(x => x.Status == PriceAlertStatus.Triggered);
            return Task.FromResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting triggered alert count", ex);
            return Task.FromResult(0);
        }
    }

    private void UpdateCachedNames(PriceAlert alert)
    {
        // Update item name from current data
        if (!string.IsNullOrEmpty(alert.ItemId))
        {
            var item = _groceryData.Items.GetById(alert.ItemId);
            if (item != null)
            {
                alert.ItemName = item.Name;
            }
        }

        // Update place name from current data
        if (!string.IsNullOrEmpty(alert.PlaceId))
        {
            var place = _groceryData.Places.GetById(alert.PlaceId);
            if (place != null)
            {
                alert.PlaceName = place.Name;
            }
        }
    }
}
