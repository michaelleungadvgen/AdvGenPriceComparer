namespace AdvGenPriceComparer.Core.Models;

public class PriceRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public required string ItemId { get; set; }
    
    public required string PlaceId { get; set; }
    
    public required decimal Price { get; set; }
    
    public decimal? OriginalPrice { get; set; } // For tracking discounted prices
    
    public bool IsOnSale { get; set; } = false;
    
    public string? SaleDescription { get; set; } // "50% off", "Buy 2 get 1 free", etc.
    
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;
    
    public string? Source { get; set; } // "catalogue", "manual", "api", etc.
    
    public DateTime? CatalogueDate { get; set; }
    
    public DateTime? ValidFrom { get; set; }
    
    public DateTime? ValidTo { get; set; }
    
    public bool IsVerified { get; set; } = false;
    
    public string? Notes { get; set; }
}
