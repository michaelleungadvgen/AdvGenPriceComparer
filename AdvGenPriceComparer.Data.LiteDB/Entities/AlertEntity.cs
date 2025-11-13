using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

/// <summary>
/// Database entity for AlertLogicEntity. Maps the core AlertLogicEntity model to LiteDB.
/// </summary>
public class AlertEntity
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    public ObjectId ItemId { get; set; }

    public ObjectId? PlaceId { get; set; }

    public AlertType Type { get; set; }

    public decimal? ThresholdPercentage { get; set; }

    public decimal? ThresholdPrice { get; set; }

    public AlertCondition Condition { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? LastTriggered { get; set; }

    public decimal? BaselinePrice { get; set; }

    public decimal? CurrentPrice { get; set; }

    public decimal? PreviousPrice { get; set; }

    public bool IsActive { get; set; }

    public bool IsRead { get; set; }

    public bool IsDismissed { get; set; }

    public string? Message { get; set; }

    public string? Notes { get; set; }

    public AlertFrequency CheckFrequency { get; set; }

    public string? AlertName { get; set; }

    /// <summary>
    /// Creates an AlertEntity from a core AlertLogicEntity model.
    /// </summary>
    public static AlertEntity FromAlert(AlertLogicEntity alert)
    {
        return new AlertEntity
        {
            Id = ObjectIdHelper.ParseObjectIdOrDefault(alert.Id),
            ItemId = ObjectIdHelper.ParseObjectIdOrDefault(alert.ItemId),
            PlaceId = alert.PlaceId != null ? ObjectIdHelper.ParseObjectIdOrDefault(alert.PlaceId) : null,
            Type = alert.Type,
            ThresholdPercentage = alert.ThresholdPercentage,
            ThresholdPrice = alert.ThresholdPrice,
            Condition = alert.Condition,
            DateCreated = alert.DateCreated,
            LastTriggered = alert.LastTriggered,
            BaselinePrice = alert.BaselinePrice,
            CurrentPrice = alert.CurrentPrice,
            PreviousPrice = alert.PreviousPrice,
            IsActive = alert.IsActive,
            IsRead = alert.IsRead,
            IsDismissed = alert.IsDismissed,
            Message = alert.Message,
            Notes = alert.Notes,
            CheckFrequency = alert.CheckFrequency,
            AlertName = alert.AlertName
        };
    }

    /// <summary>
    /// Converts this AlertEntity to a core AlertLogicEntity model.
    /// </summary>
    public AlertLogicEntity ToAlert()
    {
        return new AlertLogicEntity
        {
            Id = Id.ToString(),
            ItemId = ItemId.ToString(),
            PlaceId = PlaceId?.ToString(),
            Type = Type,
            ThresholdPercentage = ThresholdPercentage,
            ThresholdPrice = ThresholdPrice,
            Condition = Condition,
            DateCreated = DateCreated,
            LastTriggered = LastTriggered,
            BaselinePrice = BaselinePrice,
            CurrentPrice = CurrentPrice,
            PreviousPrice = PreviousPrice,
            IsActive = IsActive,
            IsRead = IsRead,
            IsDismissed = IsDismissed,
            Message = Message,
            Notes = Notes,
            CheckFrequency = CheckFrequency,
            AlertName = AlertName
        };
    }
}
