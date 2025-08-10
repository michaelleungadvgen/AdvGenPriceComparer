using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

public class PlaceEntity
{
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();
    
    [BsonField("name")]
    public required string Name { get; set; }
    
    [BsonField("chain")]
    public string? Chain { get; set; }
    
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
    
    [BsonField("phone")]
    public string? Phone { get; set; }
    
    [BsonField("email")]
    public string? Email { get; set; }
    
    [BsonField("website")]
    public string? Website { get; set; }
    
    [BsonField("operatingHours")]
    public string? OperatingHours { get; set; }
    
    [BsonField("services")]
    public List<string> Services { get; set; } = new();
    
    [BsonField("isActive")]
    public bool IsActive { get; set; } = true;
    
    [BsonField("dateAdded")]
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    [BsonField("extraInfo")]
    public Dictionary<string, string> ExtraInformation { get; set; } = new();

    // Convert from Core Place to LiteDB PlaceEntity
    public static PlaceEntity FromPlace(Place place)
    {
        return new PlaceEntity
        {
            Id = ObjectIdHelper.ParseObjectIdOrDefault(place.Id),
            Name = place.Name,
            Chain = place.Chain,
            StoreNumber = place.StoreNumber,
            Address = place.Address,
            Suburb = place.Suburb,
            State = place.State,
            Postcode = place.Postcode,
            Latitude = place.Latitude,
            Longitude = place.Longitude,
            Phone = place.Phone,
            Email = place.Email,
            Website = place.Website,
            OperatingHours = place.OperatingHours,
            Services = new List<string>(place.Services),
            IsActive = place.IsActive,
            DateAdded = place.DateAdded,
            ExtraInformation = new Dictionary<string, string>(place.ExtraInformation)
        };
    }

    // Convert from LiteDB PlaceEntity to Core Place
    public Place ToPlace()
    {
        return new Place
        {
            Id = Id.ToString(),
            Name = Name,
            Chain = Chain,
            StoreNumber = StoreNumber,
            Address = Address,
            Suburb = Suburb,
            State = State,
            Postcode = Postcode,
            Latitude = Latitude,
            Longitude = Longitude,
            Phone = Phone,
            Email = Email,
            Website = Website,
            OperatingHours = OperatingHours,
            Services = new List<string>(Services),
            IsActive = IsActive,
            DateAdded = DateAdded,
            ExtraInformation = new Dictionary<string, string>(ExtraInformation)
        };
    }
}