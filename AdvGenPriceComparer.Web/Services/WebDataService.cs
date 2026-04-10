using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;

namespace AdvGenPriceComparer.Web.Services;

/// <summary>
/// Web data service wrapper that provides data access for the Blazor web application.
/// This service abstracts the underlying data provider (IGroceryDataService) and provides
/// web-optimized methods for browsing, searching, and managing grocery price data.
/// </summary>
public interface IWebDataService
{
    // Items
    IEnumerable<Item> GetItems(string? search = null, string? category = null, int page = 1, int pageSize = 20);
    Item? GetItemById(string id);
    IEnumerable<string> GetCategories();
    int GetTotalItemCount(string? search = null, string? category = null);
    
    // Places/Stores
    IEnumerable<Place> GetPlaces();
    Place? GetPlaceById(string id);
    IEnumerable<string> GetStoreChains();
    
    // Price Records
    IEnumerable<PriceRecord> GetPriceRecords(string? itemId = null, string? placeId = null);
    PriceRecord? GetLatestPrice(string itemId, string? placeId = null);
    IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? from = null, DateTime? to = null);
    
    // Deals and Analytics
    IEnumerable<Item> GetWeeklyDeals(int limit = 20);
    IEnumerable<(Item? Item, decimal BestPrice, Place? BestStore)> GetBestDeals(int limit = 20);
    
    // Admin CRUD Operations
    string CreateItem(Item item);
    void UpdateItem(Item item);
    void DeleteItem(string id);
    
    string CreatePlace(Place place);
    void UpdatePlace(Place place);
    void DeletePlace(string id);
    
    string CreatePriceRecord(PriceRecord priceRecord);
    void UpdatePriceRecord(PriceRecord priceRecord);
    void DeletePriceRecord(string id);
    
    // Statistics
    DashboardStats GetDashboardStats();
}

/// <summary>
/// Dashboard statistics for the admin dashboard
/// </summary>
public class DashboardStats
{
    public int TotalItems { get; set; }
    public int TotalPlaces { get; set; }
    public int TotalPriceRecords { get; set; }
    public int ActiveDeals { get; set; }
    public DateTime LastUpdated { get; set; }
}

/// <summary>
/// Implementation of IWebDataService using IGroceryDataService
/// </summary>
public class WebDataService : IWebDataService
{
    private readonly IGroceryDataService _groceryDataService;
    private readonly ILogger<WebDataService> _logger;

    public WebDataService(IGroceryDataService groceryDataService, ILogger<WebDataService> logger)
    {
        _groceryDataService = groceryDataService;
        _logger = logger;
    }

    public IEnumerable<Item> GetItems(string? search = null, string? category = null, int page = 1, int pageSize = 20)
    {
        try
        {
            var allItems = _groceryDataService.Items.GetAll();
            var query = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => 
                    i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(i.Brand) && i.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(i.Barcode) && i.Barcode.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Category == category);
            }

            return query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting items");
            return Enumerable.Empty<Item>();
        }
    }

    public Item? GetItemById(string id)
    {
        try
        {
            return _groceryDataService.Items.GetById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting item by ID: {ItemId}", id);
            return null;
        }
    }

    public IEnumerable<string> GetCategories()
    {
        try
        {
            return _groceryDataService.Items.GetAllCategories();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return Enumerable.Empty<string>();
        }
    }

    public int GetTotalItemCount(string? search = null, string? category = null)
    {
        try
        {
            var allItems = _groceryDataService.Items.GetAll();
            var query = allItems.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => 
                    i.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (!string.IsNullOrEmpty(i.Brand) && i.Brand.Contains(search, StringComparison.OrdinalIgnoreCase)));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                query = query.Where(i => i.Category == category);
            }

            return query.Count();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total item count");
            return 0;
        }
    }

    public IEnumerable<Place> GetPlaces()
    {
        try
        {
            return _groceryDataService.Places.GetAll();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting places");
            return Enumerable.Empty<Place>();
        }
    }

    public Place? GetPlaceById(string id)
    {
        try
        {
            return _groceryDataService.Places.GetById(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting place by ID: {PlaceId}", id);
            return null;
        }
    }

    public IEnumerable<string> GetStoreChains()
    {
        try
        {
            var places = _groceryDataService.Places.GetAll();
            return places
                .Select(p => p.Chain)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store chains");
            return Enumerable.Empty<string>();
        }
    }

    public IEnumerable<PriceRecord> GetPriceRecords(string? itemId = null, string? placeId = null)
    {
        try
        {
            var records = _groceryDataService.PriceRecords.GetAll();
            var query = records.AsEnumerable();

            if (!string.IsNullOrEmpty(itemId))
            {
                query = query.Where(r => r.ItemId == itemId);
            }

            if (!string.IsNullOrEmpty(placeId))
            {
                query = query.Where(r => r.PlaceId == placeId);
            }

            return query.OrderByDescending(r => r.DateRecorded).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price records");
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public PriceRecord? GetLatestPrice(string itemId, string? placeId = null)
    {
        try
        {
            var records = GetPriceRecords(itemId, placeId);
            return records.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest price for item: {ItemId}", itemId);
            return null;
        }
    }

    public IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            return _groceryDataService.PriceRecords.GetByItem(itemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price history for item: {ItemId}", itemId);
            return Enumerable.Empty<PriceRecord>();
        }
    }

    public IEnumerable<Item> GetWeeklyDeals(int limit = 20)
    {
        try
        {
            var deals = GetBestDeals(limit);
            return deals.Select(d => d.Item).Where(i => i != null).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting weekly deals");
            return Enumerable.Empty<Item>();
        }
    }

    public IEnumerable<(Item? Item, decimal BestPrice, Place? BestStore)> GetBestDeals(int limit = 20)
    {
        try
        {
            // Get all price records
            var records = _groceryDataService.PriceRecords.GetAll()
                .Where(r => r.IsOnSale || r.OriginalPrice > r.Price)
                .OrderByDescending(r => r.DateRecorded)
                .Take(limit * 2)
                .ToList();

            var result = new List<(Item? Item, decimal BestPrice, Place? BestStore)>();

            foreach (var record in records)
            {
                var item = _groceryDataService.Items.GetById(record.ItemId);
                var place = _groceryDataService.Places.GetById(record.PlaceId);
                
                if (item != null && place != null)
                {
                    result.Add((item, record.Price, place));
                }

                if (result.Count >= limit) break;
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting best deals");
            return Enumerable.Empty<(Item?, decimal, Place?)>();
        }
    }

    // Admin CRUD Operations
    public string CreateItem(Item item)
    {
        return _groceryDataService.Items.Add(item);
    }

    public void UpdateItem(Item item)
    {
        _groceryDataService.Items.Update(item);
    }

    public void DeleteItem(string id)
    {
        _groceryDataService.Items.Delete(id);
    }

    public string CreatePlace(Place place)
    {
        return _groceryDataService.Places.Add(place);
    }

    public void UpdatePlace(Place place)
    {
        _groceryDataService.Places.Update(place);
    }

    public void DeletePlace(string id)
    {
        _groceryDataService.Places.Delete(id);
    }

    public string CreatePriceRecord(PriceRecord priceRecord)
    {
        return _groceryDataService.PriceRecords.Add(priceRecord);
    }

    public void UpdatePriceRecord(PriceRecord priceRecord)
    {
        _groceryDataService.PriceRecords.Update(priceRecord);
    }

    public void DeletePriceRecord(string id)
    {
        _groceryDataService.PriceRecords.Delete(id);
    }

    public DashboardStats GetDashboardStats()
    {
        try
        {
            var items = _groceryDataService.Items.GetAll();
            var places = _groceryDataService.Places.GetAll();
            var prices = _groceryDataService.PriceRecords.GetAll();
            var recentPrices = prices.Where(p => p.DateRecorded > DateTime.Now.AddDays(-7));

            return new DashboardStats
            {
                TotalItems = items.Count(),
                TotalPlaces = places.Count(),
                TotalPriceRecords = prices.Count(),
                ActiveDeals = recentPrices.Count(p => p.IsOnSale),
                LastUpdated = prices.Any() ? prices.Max(p => p.DateRecorded) : DateTime.Now
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard stats");
            return new DashboardStats();
        }
    }
}
