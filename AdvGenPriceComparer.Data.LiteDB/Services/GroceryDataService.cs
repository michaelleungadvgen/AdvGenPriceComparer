using System;
using System.Linq;
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
        Alerts = new AlertRepository(_database);

        // Initialize with default supermarket chains if database is empty
        InitializeDefaultData();
    }

    public GroceryDataService(DatabaseService databaseService)
    {
        _database = databaseService;
        Items = new ItemRepository(_database);
        Places = new PlaceRepository(_database);
        PriceRecords = new PriceRecordRepository(_database);
        Alerts = new AlertRepository(_database);

        // Initialize with default supermarket chains if database is empty
        InitializeDefaultData();
    }

    public IItemRepository Items { get; }
    public IPlaceRepository Places { get; }
    public IPriceRecordRepository PriceRecords { get; }
    public IAlertRepository Alerts { get; }

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

    public string RecordPrice(string itemId, string placeId, decimal price,
        bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null,
        DateTime? validFrom = null, DateTime? validTo = null, string source = "manual")
    {
        // Get previous price for alert checking
        var previousPriceRecord = PriceRecords.GetLatestPrice(itemId, placeId);

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

        var recordId = PriceRecords.Add(priceRecord);

        // Check and trigger alerts if price changed
        if (previousPriceRecord != null && previousPriceRecord.Price != price)
        {
            CheckAndTriggerAlerts(itemId, placeId, previousPriceRecord.Price, price, isOnSale);
        }

        return recordId;
    }

    private void CheckAndTriggerAlerts(string itemId, string placeId, decimal oldPrice, decimal newPrice, bool isOnSale)
    {
        // Get alerts for this specific item/place combination
        var itemAlerts = Alerts.GetAlertsByItem(itemId);

        foreach (var alert in itemAlerts)
        {
            // Skip if alert is for a different place (when PlaceId is specified)
            if (alert.PlaceId != null && alert.PlaceId != placeId)
                continue;

            // Check if alert should trigger
            bool shouldTrigger = alert.Type switch
            {
                AlertType.OnSale => isOnSale,
                _ => alert.ShouldTrigger(oldPrice, newPrice)
            };

            if (shouldTrigger)
            {
                alert.Trigger(newPrice, oldPrice);

                // Generate message
                var item = Items.GetById(itemId);
                var place = Places.GetById(placeId);
                alert.Message = alert.GenerateMessage(item?.Name ?? "Unknown Item", place?.Name);

                Alerts.Update(alert);
            }
        }
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

    public (decimal min, decimal max, decimal avg) GetPriceStatistics(string itemId)
    {
        var min = PriceRecords.GetLowestPrice(itemId) ?? 0;
        var max = PriceRecords.GetHighestPrice(itemId) ?? 0;
        var avg = PriceRecords.GetAveragePrice(itemId) ?? 0;
        
        return (min, max, avg);
    }

    public bool IsGoodDeal(string itemId, string placeId, decimal currentPrice)
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
    public Item? GetItemById(string id)
    {
        return Items.GetById(id);
    }

    public IEnumerable<Item> GetAllItems()
    {
        return Items.GetAll();
    }

    public Place? GetPlaceById(string id)
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

    // Reporting and analytics methods
    public IEnumerable<PriceRecord> GetPriceHistory(string? itemId = null, string? placeId = null, DateTime? from = null, DateTime? to = null)
    {
        var allRecords = PriceRecords.GetAll();
        
        // Apply filters
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
        
        var categoryGroups = items.GroupBy(i => i.Category ?? "Unknown");
        
        foreach (var group in categoryGroups)
        {
            var categoryItems = group.ToList();
            var totalPrice = 0m;
            var priceCount = 0;
            
            foreach (var item in categoryItems)
            {
                var avgPrice = PriceRecords.GetAveragePrice(item.Id);
                if (avgPrice.HasValue)
                {
                    totalPrice += avgPrice.Value;
                    priceCount++;
                }
            }
            
            var avgCategoryPrice = priceCount > 0 ? totalPrice / priceCount : 0m;
            yield return (group.Key, avgCategoryPrice, categoryItems.Count);
        }
    }

    public IEnumerable<(string storeName, decimal avgPrice, int productCount)> GetStoreComparisonStats()
    {
        var places = Places.GetAll();
        
        foreach (var place in places)
        {
            var priceRecords = PriceRecords.GetAll()
                .Where(pr => pr.PlaceId == place.Id)
                .ToList();
                
            if (priceRecords.Any())
            {
                var avgPrice = priceRecords.Average(pr => pr.Price);
                var uniqueItems = priceRecords.Select(pr => pr.ItemId).Distinct().Count();
                
                var storeName = !string.IsNullOrEmpty(place.Chain) ? place.Chain : place.Name;
                yield return (storeName, avgPrice, uniqueItems);
            }
        }
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