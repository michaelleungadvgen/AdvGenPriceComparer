using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

public class PriceRecordEntity
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
    public decimal? OriginalPrice { get; set; }
    
    [BsonField("isOnSale")]
    public bool IsOnSale { get; set; } = false;
    
    [BsonField("saleDescription")]
    public string? SaleDescription { get; set; }
    
    [BsonField("dateRecorded")]
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    
    [BsonField("source")]
    public string? Source { get; set; }
    
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

    // Convert from Core PriceRecord to LiteDB PriceRecordEntity
    public static PriceRecordEntity FromPriceRecord(PriceRecord priceRecord)
    {
        return new PriceRecordEntity
        {
            Id = ObjectIdHelper.ParseObjectIdOrDefault(priceRecord.Id),
            ItemId = ObjectIdHelper.ParseObjectIdOrDefault(priceRecord.ItemId),
            PlaceId = ObjectIdHelper.ParseObjectIdOrDefault(priceRecord.PlaceId),
            Price = priceRecord.Price,
            OriginalPrice = priceRecord.OriginalPrice,
            IsOnSale = priceRecord.IsOnSale,
            SaleDescription = priceRecord.SaleDescription,
            DateRecorded = priceRecord.DateRecorded,
            Source = priceRecord.Source,
            CatalogueDate = priceRecord.CatalogueDate,
            ValidFrom = priceRecord.ValidFrom,
            ValidTo = priceRecord.ValidTo,
            IsVerified = priceRecord.IsVerified,
            Notes = priceRecord.Notes
        };
    }

    // Convert from LiteDB PriceRecordEntity to Core PriceRecord
    public PriceRecord ToPriceRecord()
    {
        return new PriceRecord
        {
            Id = Id.ToString(),
            ItemId = ItemId.ToString(),
            PlaceId = PlaceId.ToString(),
            Price = Price,
            OriginalPrice = OriginalPrice,
            IsOnSale = IsOnSale,
            SaleDescription = SaleDescription,
            DateRecorded = DateRecorded,
            Source = Source,
            CatalogueDate = CatalogueDate,
            ValidFrom = ValidFrom,
            ValidTo = ValidTo,
            IsVerified = IsVerified,
            Notes = Notes
        };
    }
}