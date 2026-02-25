using LiteDB;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Utilities;
using CoreAlert = AdvGenPriceComparer.Core.Models.AlertLogicEntity;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly DatabaseService _database;

    public AlertRepository(DatabaseService database)
    {
        _database = database;
    }

    public string Add(CoreAlert alert)
    {
        alert.DateCreated = DateTime.UtcNow;
        var entity = AlertEntity.FromAlert(alert);
        var insertedId = _database.Alerts.Insert(entity);
        // Handle BsonValue properly - if it's an ObjectId, extract the string value
        return insertedId.IsObjectId ? insertedId.AsObjectId.ToString() : insertedId.ToString();
    }

    public bool Update(CoreAlert alert)
    {
        var entity = AlertEntity.FromAlert(alert);
        return _database.Alerts.Update(entity);
    }

    public bool Delete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        return _database.Alerts.Delete(objectId);
    }

    public CoreAlert? GetById(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return null;
        var entity = _database.Alerts.FindById(objectId);
        return entity?.ToAlert();
    }

    public IEnumerable<CoreAlert> GetAll()
    {
        return _database.Alerts.FindAll().Select(x => x.ToAlert());
    }

    public IEnumerable<CoreAlert> GetActiveAlerts()
    {
        return _database.Alerts
            .Find(x => x.IsActive && !x.IsDismissed)
            .Select(x => x.ToAlert());
    }

    public IEnumerable<CoreAlert> GetUnreadAlerts()
    {
        return _database.Alerts
            .Find(x => x.IsActive && !x.IsRead && !x.IsDismissed)
            .OrderByDescending(x => x.LastTriggered ?? x.DateCreated)
            .Select(x => x.ToAlert());
    }

    public IEnumerable<CoreAlert> GetAlertsByItem(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectId)) return Enumerable.Empty<CoreAlert>();

        return _database.Alerts
            .Find(x => x.ItemId == objectId && x.IsActive)
            .Select(x => x.ToAlert());
    }

    public IEnumerable<CoreAlert> GetAlertsByPlace(string placeId)
    {
        if (!ObjectIdHelper.TryParseObjectId(placeId, out var objectId)) return Enumerable.Empty<CoreAlert>();

        return _database.Alerts
            .Find(x => x.PlaceId == objectId && x.IsActive)
            .Select(x => x.ToAlert());
    }

    public IEnumerable<CoreAlert> GetTriggeredAlerts(DateTime since)
    {
        return _database.Alerts
            .Find(x => x.IsActive && x.LastTriggered.HasValue && x.LastTriggered.Value >= since)
            .OrderByDescending(x => x.LastTriggered)
            .Select(x => x.ToAlert());
    }

    public int GetUnreadCount()
    {
        return _database.Alerts.Count(x => x.IsActive && !x.IsRead && !x.IsDismissed);
    }

    public bool MarkAsRead(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;

        var entity = _database.Alerts.FindById(objectId);
        if (entity == null) return false;

        entity.IsRead = true;
        return _database.Alerts.Update(entity);
    }

    public bool MarkAllAsRead()
    {
        var unreadAlerts = _database.Alerts.Find(x => !x.IsRead && x.IsActive);
        foreach (var alert in unreadAlerts)
        {
            alert.IsRead = true;
            _database.Alerts.Update(alert);
        }
        return true;
    }

    public bool Dismiss(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;

        var entity = _database.Alerts.FindById(objectId);
        if (entity == null) return false;

        entity.IsDismissed = true;
        entity.IsRead = true;
        return _database.Alerts.Update(entity);
    }

    public bool DismissAllRead()
    {
        var readAlerts = _database.Alerts.Find(x => x.IsRead && !x.IsDismissed && x.IsActive);
        foreach (var alert in readAlerts)
        {
            alert.IsDismissed = true;
            _database.Alerts.Update(alert);
        }
        return true;
    }
}
