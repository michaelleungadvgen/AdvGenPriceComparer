using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

public class ItemEntity
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    
    [BsonField("name")]
    public required string Name { get; set; }
    
    [BsonField("description")]
    public string? Description { get; set; }
    
    [BsonField("brand")]
    public string? Brand { get; set; }
    
    [BsonField("category")]
    public string? Category { get; set; }
    
    [BsonField("subCategory")]
    public string? SubCategory { get; set; }
    
    [BsonField("barcode")]
    public string? Barcode { get; set; }
    
    [BsonField("packageSize")]
    public string? PackageSize { get; set; }
    
    [BsonField("unit")]
    public string? Unit { get; set; }
    
    [BsonField("weight")]
    public decimal? Weight { get; set; }
    
    [BsonField("volume")]
    public decimal? Volume { get; set; }
    
    [BsonField("imageUrl")]
    public string? ImageUrl { get; set; }
    
    [BsonField("nutritionalInfo")]
    public Dictionary<string, decimal> NutritionalInfo { get; set; } = new();
    
    [BsonField("allergens")]
    public List<string> Allergens { get; set; } = new();
    
    [BsonField("dietaryFlags")]
    public List<string> DietaryFlags { get; set; } = new();
    
    [BsonField("tags")]
    public List<string> Tags { get; set; } = new();
    
    [BsonField("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonField("dateAdded")]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    [BsonField("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [BsonField("extraInfo")]
    public Dictionary<string, string> ExtraInformation { get; set; } = new();

    // Convert from Core Item to LiteDB ItemEntity
    public static ItemEntity FromItem(Item item)
    {
        return new ItemEntity
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
    }

    // Convert from LiteDB ItemEntity to Core Item
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