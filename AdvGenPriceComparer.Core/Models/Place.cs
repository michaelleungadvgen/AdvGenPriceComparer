using LiteDB;

namespace AdvGenPriceComparer.Core.Models;

public class Place
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    
    [BsonField("name")]
    public required string Name { get; set; }
    
    [BsonField("chain")]
    public string? Chain { get; set; } // Coles, Woolworths, IGA, etc.
    
    [BsonField("storeNumber")]
    public string? StoreNumber { get; set; }
    
    [BsonField("address")]
    public string? Address { get; set; }
    
    [BsonField("suburb")]
    public string? Suburb { get; set; }
    
    [BsonField("state")]
    public string? State { get; set; }
    
    [BsonField("postcode")]
    public string? Postcode { get; set; }
    
    [BsonField("latitude")]
    public double? Latitude { get; set; }
    
    [BsonField("longitude")]
    public double? Longitude { get; set; }
    
    [BsonField("phoneNumber")]
    public string? PhoneNumber { get; set; }
    
    [BsonField("email")]
    public string? Email { get; set; }
    
    [BsonField("website")]
    public string? Website { get; set; }
    
    [BsonField("hours")]
    public string? Hours { get; set; }
    
    [BsonField("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonField("dateAdded")]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    [BsonField("extraInfo")]
    public Dictionary<string, string> ExtraInformation { get; set; } = new();
}
