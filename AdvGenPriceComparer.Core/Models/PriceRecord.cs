using LiteDB;

namespace AdvGenPriceComparer.Core.Models;

public class PriceRecord
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    
    [BsonField("itemId")]
    public required ObjectId ItemId { get; set; }
    
    [BsonField("placeId")]
    public required ObjectId PlaceId { get; set; }
    
    [BsonField("price")]
    public required decimal Price { get; set; }
    
    [BsonField("originalPrice")]
    public decimal? OriginalPrice { get; set; } // For tracking discounted prices
    
    [BsonField("isOnSale")]
    public bool IsOnSale { get; set; } = false;
    
    [BsonField("saleDescription")]
    public string? SaleDescription { get; set; } // "50% off", "Buy 2 get 1 free", etc.
    
    [BsonField("dateRecorded")]
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    
    [BsonField("source")]
    public string? Source { get; set; } // "catalogue", "manual", "api", etc.
    
    [BsonField("catalogueDate")]
    public DateTime? CatalogueDate { get; set; }
    
    [BsonField("validFrom")]
    public DateTime? ValidFrom { get; set; }
    
    [BsonField("validTo")]
    public DateTime? ValidTo { get; set; }
    
    [BsonField("isVerified")]
    public bool IsVerified { get; set; } = false;
    
    [BsonField("notes")]
    public string? Notes { get; set; }
}
