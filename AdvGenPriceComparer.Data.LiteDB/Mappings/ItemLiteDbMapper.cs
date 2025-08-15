using LiteDB;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Data.LiteDB.Mappings;

public static class ItemLiteDbMapper
{
    public static BsonDocument ToBson(Item item)
    {
        var doc = new BsonDocument
        {
            ["_id"] = item.Id.ToString(),
            ["name"] = item.Name,
            ["description"] = item.Description,
            ["brand"] = item.Brand,
            ["category"] = item.Category,
            ["subCategory"] = item.SubCategory,
            ["barcode"] = item.Barcode,
            ["packageSize"] = item.PackageSize,
            ["unit"] = item.Unit,
            ["weight"] = item.Weight,
            ["volume"] = item.Volume,
            ["imageUrl"] = item.ImageUrl,
            ["nutritionalInfo"] = new BsonDocument(item.NutritionalInfo.ToDictionary(kv => kv.Key, kv => new BsonValue(kv.Value))),
            ["allergens"] = new BsonArray(item.Allergens),
            ["dietaryFlags"] = new BsonArray(item.DietaryFlags),
            ["tags"] = new BsonArray(item.Tags),
            ["isActive"] = item.IsActive,
            ["dateAdded"] = item.DateAdded,
            ["lastUpdated"] = item.LastUpdated,
            ["extraInfo"] = new BsonDocument(item.ExtraInformation)
        };
        return doc;
    }

    public static Item FromBson(BsonDocument doc)
    {
        return new Item
        {
            Id = Guid.TryParse(doc["_id"].AsString, out var guid) ? guid : Guid.NewGuid(),
            Name = doc["name"].AsString,
            Description = doc["description"].IsNull ? null : doc["description"].AsString,
            Brand = doc["brand"].IsNull ? null : doc["brand"].AsString,
            Category = doc["category"].IsNull ? null : doc["category"].AsString,
            SubCategory = doc["subCategory"].IsNull ? null : doc["subCategory"].AsString,
            Barcode = doc["barcode"].IsNull ? null : doc["barcode"].AsString,
            PackageSize = doc["packageSize"].IsNull ? null : doc["packageSize"].AsString,
            Unit = doc["unit"].IsNull ? null : doc["unit"].AsString,
            Weight = doc["weight"].IsNull ? null : doc["weight"].AsDecimal,
            Volume = doc["volume"].IsNull ? null : doc["volume"].AsDecimal,
            ImageUrl = doc["imageUrl"].IsNull ? null : doc["imageUrl"].AsString,
            NutritionalInfo = doc["nutritionalInfo"].IsDocument ? doc["nutritionalInfo"].AsDocument.ToDictionary(kv => kv.Key, kv => kv.Value.AsDecimal) : new Dictionary<string, decimal>(),
            Allergens = doc["allergens"].IsArray ? doc["allergens"].AsArray.Select(x => x.AsString).ToList() : new List<string>(),
            DietaryFlags = doc["dietaryFlags"].IsArray ? doc["dietaryFlags"].AsArray.Select(x => x.AsString).ToList() : new List<string>(),
            Tags = doc["tags"].IsArray ? doc["tags"].AsArray.Select(x => x.AsString).ToList() : new List<string>(),
            IsActive = doc["isActive"].AsBoolean,
            DateAdded = doc["dateAdded"].AsDateTime,
            LastUpdated = doc["lastUpdated"].AsDateTime,
            ExtraInformation = doc["extraInfo"].IsDocument ? doc["extraInfo"].AsDocument.ToDictionary(kv => kv.Key, kv => kv.Value.AsString) : new Dictionary<string, string>()
        };
    }
}
