using LiteDB;

namespace AdvGenPriceComparer.Core.Models;

public class Item
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
    
    [BsonField("imageUrl")]
    public string? ImageUrl { get; set; }
    
    [BsonField("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonField("dateAdded")]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    [BsonField("lastUpdated")]
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    [BsonField("extraInfo")]
    public Dictionary<string, string> ExtraInformation { get; set; } = new();
}

