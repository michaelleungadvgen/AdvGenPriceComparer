using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class PriceRecordRepository : IPriceRecordRepository
{
    private readonly DatabaseService _database;

    public PriceRecordRepository(DatabaseService database)
    {
        _database = database;
    }

    public ObjectId Add(PriceRecord priceRecord)
    {
        priceRecord.DateRecorded = DateTime.UtcNow;
        return _database.PriceRecords.Insert(priceRecord);
    }

    public bool Update(PriceRecord priceRecord)
    {
        return _database.PriceRecords.Update(priceRecord);
    }

    public bool Delete(ObjectId id)
    {
        return _database.PriceRecords.Delete(id);
    }

    public PriceRecord? GetById(ObjectId id)
    {
        return _database.PriceRecords.FindById(id);
    }

    public IEnumerable<PriceRecord> GetAll()
    {
        return _database.PriceRecords.FindAll();
    }

    public IEnumerable<PriceRecord> GetByItem(ObjectId itemId)
    {
        return _database.PriceRecords
            .Find(x => x.ItemId == itemId)
            .OrderByDescending(x => x.DateRecorded);
    }

    public IEnumerable<PriceRecord> GetByPlace(ObjectId placeId)
    {
        return _database.PriceRecords
            .Find(x => x.PlaceId == placeId)
            .OrderByDescending(x => x.DateRecorded);
    }

    public IEnumerable<PriceRecord> GetByItemAndPlace(ObjectId itemId, ObjectId placeId)
    {
        return _database.PriceRecords
            .Find(x => x.ItemId == itemId && x.PlaceId == placeId)
            .OrderByDescending(x => x.DateRecorded);
    }

    public PriceRecord? GetLatestPrice(ObjectId itemId, ObjectId placeId)
    {
        return _database.PriceRecords
            .Find(x => x.ItemId == itemId && x.PlaceId == placeId)
            .OrderByDescending(x => x.DateRecorded)
            .FirstOrDefault();
    }

    public IEnumerable<PriceRecord> GetCurrentSales()
    {
        var now = DateTime.UtcNow;
        return _database.PriceRecords
            .Find(x => x.IsOnSale && 
                      (!x.ValidTo.HasValue || x.ValidTo >= now) &&
                      (!x.ValidFrom.HasValue || x.ValidFrom <= now))
            .OrderBy(x => x.Price);
    }

    public IEnumerable<PriceRecord> GetPriceHistory(ObjectId itemId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _database.PriceRecords.Find(x => x.ItemId == itemId);
        
        if (fromDate.HasValue)
            query = query.Where(x => x.DateRecorded >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(x => x.DateRecorded <= toDate.Value);
            
        return query.OrderBy(x => x.DateRecorded);
    }

    public decimal? GetLowestPrice(ObjectId itemId)
    {
        var records = _database.PriceRecords.Find(x => x.ItemId == itemId);
        return records.Any() ? records.Min(x => x.Price) : null;
    }

    public decimal? GetHighestPrice(ObjectId itemId)
    {
        var records = _database.PriceRecords.Find(x => x.ItemId == itemId);
        return records.Any() ? records.Max(x => x.Price) : null;
    }

    public decimal? GetAveragePrice(ObjectId itemId)
    {
        var records = _database.PriceRecords.Find(x => x.ItemId == itemId);
        return records.Any() ? records.Average(x => x.Price) : null;
    }

    public IEnumerable<PriceRecord> GetBestDeals(int count = 10)
    {
        // Returns items currently on sale with the biggest discount percentage
        return _database.PriceRecords
            .Find(x => x.IsOnSale && x.OriginalPrice.HasValue && x.OriginalPrice > x.Price)
            .OrderByDescending(x => (x.OriginalPrice!.Value - x.Price) / x.OriginalPrice.Value * 100)
            .Take(count);
    }

    public IEnumerable<PriceRecord> ComparePrices(ObjectId itemId)
    {
        // Get the latest price for each place for a specific item
        return _database.PriceRecords
            .Find(x => x.ItemId == itemId)
            .GroupBy(x => x.PlaceId)
            .Select(g => g.OrderByDescending(x => x.DateRecorded).First())
            .OrderBy(x => x.Price);
    }

    public int GetTotalRecordsCount()
    {
        return _database.PriceRecords.Count();
    }

    public int GetRecordsCountThisWeek()
    {
        var weekAgo = DateTime.UtcNow.AddDays(-7);
        return _database.PriceRecords.Count(x => x.DateRecorded >= weekAgo);
    }

    public int GetSaleRecordsCount()
    {
        return _database.PriceRecords.Count(x => x.IsOnSale);
    }

    public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10)
    {
        return _database.PriceRecords
            .FindAll()
            .OrderByDescending(x => x.DateRecorded)
            .Take(count);
    }

    public Dictionary<string, int> GetPriceRecordsBySource()
    {
        return _database.PriceRecords
            .Find(x => !string.IsNullOrEmpty(x.Source))
            .GroupBy(x => x.Source!)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}