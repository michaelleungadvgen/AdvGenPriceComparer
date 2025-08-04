namespace AdvGenPriceComparer.Core.Models;

public class Item
{
    public required string Name { get; set; }
    public string? Description { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public required string Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Url { get; set; }
    public string? Store { get; set; }
    public string? StoreUrl { get; set; }
    public string? StoreLogoUrl { get; set; }
    public string? StoreLocation { get; set; }
    public string? StorePhoneNumber { get; set; }
    public string? StoreEmail { get; set; }
    public string? StoreAddress { get; set; }
    public string? StoreCity { get; set; }
    public string? StoreState { get; set; }
    public string? StoreZip { get; set; }
    public string? StoreCountry { get; set; }
    public string? StoreLatitude { get; set; }
    public string? StoreLongitude { get; set; }
    public string? StoreHours { get; set; }
    public string? StoreHoursNote { get; set; }   
    public Dictionary<string, string> ExtraInformation { get; set; } = [];
}

