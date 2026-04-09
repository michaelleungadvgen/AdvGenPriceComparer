using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Test implementation of IGroceryDataService for unit tests
/// </summary>
public class TestGroceryDataService : IGroceryDataService, IDisposable
{
    private readonly List<Item> _items = new();
    private readonly List<Place> _places = new();
    private readonly List<PriceRecord> _priceRecords = new();

    public IItemRepository Items => throw new NotImplementedException();
    public IPlaceRepository Places => throw new NotImplementedException();
    public IPriceRecordRepository PriceRecords => throw new NotImplementedException();
    public IAlertRepository Alerts => throw new NotImplementedException();

    public string AddTestItem(string name, string brand, string category)
    {
        var id = Guid.NewGuid().ToString();
        _items.Add(new Item
        {
            Id = id,
            Name = name,
            Brand = brand,
            Category = category,
            DateAdded = DateTime.UtcNow,
            IsActive = true
        });
        return id;
    }

    public string AddTestPlace(string name, string chain)
    {
        var id = Guid.NewGuid().ToString();
        _places.Add(new Place
        {
            Id = id,
            Name = name,
            Chain = chain,
            DateAdded = DateTime.UtcNow,
            IsActive = true
        });
        return id;
    }

    public string AddTestPriceRecord(string itemId, string placeId, decimal price, DateTime? date = null)
    {
        var id = Guid.NewGuid().ToString();
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        var place = _places.FirstOrDefault(p => p.Id == placeId);

        _priceRecords.Add(new PriceRecord
        {
            Id = id,
            ItemId = itemId,
            PlaceId = placeId,
            Price = price,
            DateRecorded = date ?? DateTime.Now
        });
        return id;
    }

    public string AddGroceryItem(string name, string? brand = null, string? category = null, string? barcode = null, string? packageSize = null, string? unit = null)
    {
        return AddTestItem(name, brand ?? "", category ?? "");
    }

    public Item? GetItemById(string id) => _items.FirstOrDefault(i => i.Id == id);
    public IEnumerable<Item> GetAllItems() => _items;

    public string AddSupermarket(string name, string chain, string? address = null, string? suburb = null, string? state = null, string? postcode = null)
    {
        return AddTestPlace(name, chain);
    }

    public Place? GetPlaceById(string id) => _places.FirstOrDefault(p => p.Id == id);
    public IEnumerable<Place> GetAllPlaces() => _places;

    public string RecordPrice(string itemId, string placeId, decimal price, bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null, DateTime? validFrom = null, DateTime? validTo = null, string source = "manual")
    {
        return AddTestPriceRecord(itemId, placeId, price);
    }

    public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10) => _priceRecords.Take(count);

    public IEnumerable<(Item item, decimal lowestPrice, Place place)> FindBestDeals(string? category = null)
    {
        return Enumerable.Empty<(Item, decimal, Place)>();
    }

    public Dictionary<string, object> GetDashboardStats()
    {
        return new Dictionary<string, object>
        {
            ["totalItems"] = _items.Count,
            ["totalStores"] = _places.Count,
            ["recentUpdates"] = _priceRecords.Count
        };
    }

    public IEnumerable<PriceRecord> GetPriceHistory(string? itemId = null, string? placeId = null, DateTime? from = null, DateTime? to = null)
    {
        var query = _priceRecords.AsEnumerable();
        if (!string.IsNullOrEmpty(itemId))
            query = query.Where(pr => pr.ItemId == itemId);
        if (!string.IsNullOrEmpty(placeId))
            query = query.Where(pr => pr.PlaceId == placeId);
        return query;
    }

    public IEnumerable<(string category, decimal avgPrice, int count)> GetCategoryStats()
    {
        return _items
            .Where(i => !string.IsNullOrEmpty(i.Category))
            .GroupBy(i => i.Category!)
            .Select(g => (g.Key, 0m, g.Count()));
    }

    public IEnumerable<(string storeName, decimal avgPrice, int productCount)> GetStoreComparisonStats()
    {
        return _places.Select(p => (p.Name, 0m, 0));
    }

    public void Dispose()
    {
        _items.Clear();
        _places.Clear();
        _priceRecords.Clear();
    }
}
