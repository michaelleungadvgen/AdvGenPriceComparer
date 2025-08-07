using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

public class GroceryDataService : IGroceryDataService
{
    private readonly DatabaseService _database;
    private bool _disposed = false;

    public GroceryDataService(string databasePath = "GroceryPrices.db")
    {
        _database = new DatabaseService(databasePath);
        Items = new ItemRepository(_database);
        Places = new PlaceRepository(_database);
        PriceRecords = new PriceRecordRepository(_database);
        
        // Initialize with default supermarket chains if database is empty
        InitializeDefaultData();
    }

    public ItemRepository Items { get; }
    public PlaceRepository Places { get; }
    public PriceRecordRepository PriceRecords { get; }

    private void InitializeDefaultData()
    {
        // Add default Australian supermarket chains if they don't exist
        if (Places.GetTotalCount() == 0)
        {
            var defaultChains = new[]
            {
                new Place { Name = "Coles", Chain = "Coles" },
                new Place { Name = "Woolworths", Chain = "Woolworths" },
                new Place { Name = "IGA", Chain = "IGA" },
                new Place { Name = "ALDI", Chain = "ALDI" },
                new Place { Name = "Foodworks", Chain = "Foodworks" }
            };

            foreach (var chain in defaultChains)
            {
                Places.Add(chain);
            }
        }
    }

    // Comprehensive grocery item operations
    public ObjectId AddGroceryItem(string name, string? brand = null, string? category = null, 
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

    public ObjectId AddSupermarket(string name, string chain, string? address = null, 
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

    public ObjectId RecordPrice(ObjectId itemId, ObjectId placeId, decimal price, 
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
            Source = source
        };

        return PriceRecords.Add(priceRecord);
    }

    // Grocery-specific search and analysis methods
    public IEnumerable<Item> SearchGroceries(string searchTerm)
    {
        return Items.SearchByName(searchTerm)
            .Concat(Items.GetByBrand(searchTerm))
            .Distinct();
    }

    public IEnumerable<Place> FindNearbyStores(string suburb, string? chain = null)
    {
        var stores = Places.GetBySuburb(suburb);
        return chain != null ? stores.Where(s => s.Chain == chain) : stores;
    }

    public (decimal min, decimal max, decimal avg) GetPriceStatistics(ObjectId itemId)
    {
        var min = PriceRecords.GetLowestPrice(itemId) ?? 0;
        var max = PriceRecords.GetHighestPrice(itemId) ?? 0;
        var avg = PriceRecords.GetAveragePrice(itemId) ?? 0;
        
        return (min, max, avg);
    }

    public bool IsGoodDeal(ObjectId itemId, ObjectId placeId, decimal currentPrice)
    {
        var averagePrice = PriceRecords.GetAveragePrice(itemId);
        if (!averagePrice.HasValue) return false;
        
        // Consider it a good deal if current price is 10% below average
        return currentPrice <= averagePrice.Value * 0.9m;
    }

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

    public void BackupData(string backupPath)
    {
        _database.BackupDatabase(backupPath);
    }

    public void OptimizeDatabase()
    {
        _database.OptimizeDatabase();
    }

    // Interface implementations
    public Item? GetItemById(ObjectId id)
    {
        return Items.GetById(id);
    }

    public IEnumerable<Item> GetAllItems()
    {
        return Items.GetAll();
    }

    public Place? GetPlaceById(ObjectId id)
    {
        return Places.GetById(id);
    }

    public IEnumerable<Place> GetAllPlaces()
    {
        return Places.GetAll();
    }

    public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10)
    {
        return PriceRecords.GetRecentPriceUpdates(count);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _database?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}