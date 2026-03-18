using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

/// <summary>
/// Database entity for PriceAlert. Maps the core PriceAlert model to LiteDB.
/// </summary>
public class PriceAlertEntity
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public ObjectId ItemId { get; set; }

    public string? ItemName { get; set; }

    public ObjectId? PlaceId { get; set; }

    public string? PlaceName { get; set; }

    public decimal TargetPrice { get; set; }

    public PriceAlertCondition Condition { get; set; }

    public PriceAlertStatus Status { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? LastTriggered { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? AlertName { get; set; }

    public string? Notes { get; set; }

    public bool EnableNotification { get; set; } = true;

    public int TriggerCount { get; set; }

    public decimal? LastCheckedPrice { get; set; }

    /// <summary>
    /// Creates a PriceAlertEntity from a core PriceAlert model.
    /// </summary>
    public static PriceAlertEntity FromPriceAlert(PriceAlert alert)
    {
        return new PriceAlertEntity
        {
            Id = ObjectIdHelper.ParseObjectIdOrDefault(alert.Id),
            ItemId = ObjectIdHelper.ParseObjectIdOrDefault(alert.ItemId),
            ItemName = alert.ItemName,
            PlaceId = alert.PlaceId != null ? ObjectIdHelper.ParseObjectIdOrDefault(alert.PlaceId) : null,
            PlaceName = alert.PlaceName,
            TargetPrice = alert.TargetPrice,
            Condition = alert.Condition,
            Status = alert.Status,
            DateCreated = alert.DateCreated,
            LastTriggered = alert.LastTriggered,
            ExpiryDate = alert.ExpiryDate,
            AlertName = alert.AlertName,
            Notes = alert.Notes,
            EnableNotification = alert.EnableNotification,
            TriggerCount = alert.TriggerCount,
            LastCheckedPrice = alert.LastCheckedPrice
        };
    }

    /// <summary>
    /// Converts this PriceAlertEntity to a core PriceAlert model.
    /// </summary>
    public PriceAlert ToPriceAlert()
    {
        return new PriceAlert
        {
            Id = Id.ToString(),
            ItemId = ItemId.ToString(),
            ItemName = ItemName,
            PlaceId = PlaceId?.ToString(),
            PlaceName = PlaceName,
            TargetPrice = TargetPrice,
            Condition = Condition,
            Status = Status,
            DateCreated = DateCreated,
            LastTriggered = LastTriggered,
            ExpiryDate = ExpiryDate,
            AlertName = AlertName,
            Notes = Notes,
            EnableNotification = EnableNotification,
            TriggerCount = TriggerCount,
            LastCheckedPrice = LastCheckedPrice
        };
    }
}
