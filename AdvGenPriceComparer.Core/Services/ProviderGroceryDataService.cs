using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Services;

/// <summary>
/// Implementation of IGroceryDataService that uses an IDatabaseProvider
/// </summary>
public class ProviderGroceryDataService : IGroceryDataService
{
    private readonly IDatabaseProvider _provider;
    private bool _disposed = false;

    public ProviderGroceryDataService(IDatabaseProvider provider)
    {
        _provider = provider;
    }

    public IItemRepository Items => _provider.Items;
    public IPlaceRepository Places => _provider.Places;
    public IPriceRecordRepository PriceRecords => _provider.PriceRecords;
    public IAlertRepository Alerts => _provider.Alerts;

    public string AddGroceryItem(string name, string? brand = null, string? category = null, 
        string? barcode = null, string? packageSize = null, string? unit = null)
    {
        var item = new Item
        {
            Name = name,
            Brand = brand,
            Category = category,
            Barcode = barcode,
            PackageSize = packageSize,
            Unit = unit
        };

        return Items.Add(item);
    }

    public Item? GetItemById(string id) => Items.GetById(id);
    public IEnumerable<Item> GetAllItems() => Items.GetAll();

    public string AddSupermarket(string name, string chain, string? address = null, 
        string? suburb = null, string? state = null, string? postcode = null)
    {
        var place = new Place
        {
            Name = name,
            Chain = chain,
            Address = address,
            Suburb = suburb,
            State = state,
            Postcode = postcode
        };

        return Places.Add(place);
    }

    public Place? GetPlaceById(string id) => Places.GetById(id);
    public IEnumerable<Place> GetAllPlaces() => Places.GetAll();

    public string RecordPrice(string itemId, string placeId, decimal price, 
        bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null,
        DateTime? validFrom = null, DateTime? validTo = null, string source = "manual")
    {
        var priceRecord = new PriceRecord
        {
            ItemId = itemId,
            PlaceId = placeId,
            Price = price,
            IsOnSale = isOnSale,
            OriginalPrice = originalPrice,
            SaleDescription = saleDescription,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Source = source,
            DateRecorded = DateTime.UtcNow
        };

        return PriceRecords.Add(priceRecord);
    }

    public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10) => PriceRecords.GetRecentPriceUpdates(count);

    public IEnumerable<(Item item, decimal lowestPrice, Place place)> FindBestDeals(string? category = null)
    {
        var items = category != null ? Items.GetByCategory(category) : Items.GetAll();
        
        foreach (var item in items)
        {
            var bestPrice = PriceRecords.ComparePrices(item.Id).FirstOrDefault();
            if (bestPrice != null)
            {
                var place = Places.GetById(bestPrice.PlaceId);
                if (place != null)
                {
                    yield return (item, bestPrice.Price, place);
                }
            }
        }
    }

    public Dictionary<string, object> GetDashboardStats()
    {
        return new Dictionary<string, object>
        {
            ["totalItems"] = Items.GetTotalCount(),
            ["trackedStores"] = Places.GetTotalCount(),
            ["priceRecords"] = PriceRecords.GetTotalRecordsCount(),
            ["recentUpdates"] = PriceRecords.GetRecordsCountThisWeek(),
            ["currentSales"] = PriceRecords.GetSaleRecordsCount(),
            ["chainBreakdown"] = Places.GetChainCounts(),
            ["recentItems"] = Items.GetRecentlyAdded(5),
            ["bestDeals"] = PriceRecords.GetBestDeals(5)
        };
    }

    public IEnumerable<PriceRecord> GetPriceHistory(string? itemId = null, string? placeId = null, DateTime? from = null, DateTime? to = null)
    {
        var allRecords = PriceRecords.GetAll();
        
        if (!string.IsNullOrEmpty(itemId))
            allRecords = allRecords.Where(r => r.ItemId == itemId);
            
        if (!string.IsNullOrEmpty(placeId))
            allRecords = allRecords.Where(r => r.PlaceId == placeId);
            
        if (from.HasValue)
            allRecords = allRecords.Where(r => r.DateRecorded >= from.Value);
            
        if (to.HasValue)
            allRecords = allRecords.Where(r => r.DateRecorded <= to.Value);
            
        return allRecords.OrderBy(r => r.DateRecorded);
    }

    public IEnumerable<(string category, decimal avgPrice, int count)> GetCategoryStats()
    {
        var items = Items.GetAll().Where(i => !string.IsNullOrEmpty(i.Category));
        var categoryGroups = items.GroupBy(i => i.Category!);
        
        foreach (var group in categoryGroups)
        {
            var categoryItems = group.ToList();
            var prices = categoryItems.Select(i => PriceRecords.GetAveragePrice(i.Id) ?? 0).Where(p => p > 0).ToList();
            var avgCategoryPrice = prices.Any() ? prices.Average() : 0m;
            yield return (group.Key, avgCategoryPrice, categoryItems.Count);
        }
    }

    public IEnumerable<(string storeName, decimal avgPrice, int productCount)> GetStoreComparisonStats()
    {
        var places = Places.GetAll();
        
        foreach (var place in places)
        {
            var prices = PriceRecords.GetByPlace(place.Id).ToList();
            if (prices.Any())
            {
                var avgPrice = prices.Average(p => p.Price);
                var uniqueItems = prices.Select(p => p.ItemId).Distinct().Count();
                var storeName = !string.IsNullOrEmpty(place.Chain) ? place.Chain : place.Name;
                yield return (storeName, avgPrice, uniqueItems);
            }
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _provider.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
