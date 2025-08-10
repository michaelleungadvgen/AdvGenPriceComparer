namespace AdvGenPriceComparer.Core.Models;

public class Place
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string Name { get; set; }
    
    public string? Chain { get; set; } // Coles, Woolworths, IGA, etc.
    
    public string? StoreNumber { get; set; }
    
    public string? Address { get; set; }
    
    public string? Suburb { get; set; }
    
    public string? State { get; set; }
    
    public string? Postcode { get; set; }
    
    public double? Latitude { get; set; }
    
    public double? Longitude { get; set; }
    
    public string? Phone { get; set; }
    
    public string? Email { get; set; }
    
    public string? Website { get; set; }
    
    public string? OperatingHours { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime DateAdded { get; set; } = DateTime.UtcNow;
    
    public Dictionary<string, string> ExtraInformation { get; set; } = new();

    public List<string> Services { get; set; } = new();
}
