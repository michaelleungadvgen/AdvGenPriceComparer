using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IGroceryDataService : IDisposable
{
    // Repository properties
    IItemRepository Items { get; }
    IPlaceRepository Places { get; }
    IPriceRecordRepository PriceRecords { get; }

    // Item operations
    string AddGroceryItem(string name, string? brand = null, string? category = null, 
        string? barcode = null, string? packageSize = null, string? unit = null);
    Item? GetItemById(string id);
    IEnumerable<Item> GetAllItems();

    // Place operations  
    string AddSupermarket(string name, string chain, string? address = null, 
        string? suburb = null, string? state = null, string? postcode = null);
    Place? GetPlaceById(string id);
    IEnumerable<Place> GetAllPlaces();

    // Price operations
    string RecordPrice(string itemId, string placeId, decimal price, 
        bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null,
        DateTime? validFrom = null, DateTime? validTo = null, string source = "manual");
    IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10);

    // Search and analysis
    IEnumerable<(Item item, decimal lowestPrice, Place place)> FindBestDeals(string? category = null);
    Dictionary<string, object> GetDashboardStats();
    
    // Reporting and analytics
    IEnumerable<PriceRecord> GetPriceHistory(string? itemId = null, string? placeId = null, DateTime? from = null, DateTime? to = null);
    IEnumerable<(string category, decimal avgPrice, int count)> GetCategoryStats();
    IEnumerable<(string storeName, decimal avgPrice, int productCount)> GetStoreComparisonStats();
}