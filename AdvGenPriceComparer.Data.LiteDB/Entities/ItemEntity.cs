using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

/// <summary>
/// Database entity for Item. Inherits from core Item model and adds LiteDB-specific mapping.
/// The base.Id (string) is hidden in favor of ObjectId for LiteDB optimization.
/// All other properties are inherited from the Item model with LiteDB field mappings applied via BsonMapper configuration.
/// </summary>
public class ItemEntity : Item
{
    // Shadow the Id property to use ObjectId for LiteDB optimization
    [BsonId]
    public new ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// Creates an ItemEntity from a core Item model.
    /// </summary>
    public static ItemEntity FromItem(Item item)
    {
        var entity = new ItemEntity
        {
            Id = ObjectIdHelper.ParseObjectIdOrDefault(item.Id),
            Name = item.Name,
            Description = item.Description,
            Brand = item.Brand,
            Category = item.Category,
            SubCategory = item.SubCategory,
            Barcode = item.Barcode,
            PackageSize = item.PackageSize,
            Unit = item.Unit,
            Weight = item.Weight,
            Volume = item.Volume,
            ImageUrl = item.ImageUrl,
            NutritionalInfo = new Dictionary<string, decimal>(item.NutritionalInfo),
            Allergens = new List<string>(item.Allergens),
            DietaryFlags = new List<string>(item.DietaryFlags),
            Tags = new List<string>(item.Tags),
            IsActive = item.IsActive,
            DateAdded = item.DateAdded,
            LastUpdated = item.LastUpdated,
            ExtraInformation = new Dictionary<string, string>(item.ExtraInformation)
        };

        return entity;
    }

    /// <summary>
    /// Converts this ItemEntity to a core Item model.
    /// </summary>
    public Item ToItem()
    {
        return new Item
        {
            Id = Id.ToString(),
            Name = Name,
            Description = Description,
            Brand = Brand,
            Category = Category,
            SubCategory = SubCategory,
            Barcode = Barcode,
            PackageSize = PackageSize,
            Unit = Unit,
            Weight = Weight,
            Volume = Volume,
            ImageUrl = ImageUrl,
            NutritionalInfo = new Dictionary<string, decimal>(NutritionalInfo),
            Allergens = new List<string>(Allergens),
            DietaryFlags = new List<string>(DietaryFlags),
            Tags = new List<string>(Tags),
            IsActive = IsActive,
            DateAdded = DateAdded,
            LastUpdated = LastUpdated,
            ExtraInformation = new Dictionary<string, string>(ExtraInformation)
        };
    }
}
