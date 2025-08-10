using LiteDB;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Utilities;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

public class PriceRecordRepository : IPriceRecordRepository
{
    private readonly DatabaseService _database;

    public PriceRecordRepository(DatabaseService database)
    {
        _database = database;
    }

    public string Add(PriceRecord priceRecord)
    {
        priceRecord.DateRecorded = DateTime.UtcNow;
        var entity = PriceRecordEntity.FromPriceRecord(priceRecord);
        var insertedId = _database.PriceRecords.Insert(entity);
        return insertedId.ToString();
    }

    public bool Update(PriceRecord priceRecord)
    {
        var entity = PriceRecordEntity.FromPriceRecord(priceRecord);
        return _database.PriceRecords.Update(entity);
    }

    public bool Delete(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return false;
        return _database.PriceRecords.Delete(objectId);
    }

    public PriceRecord? GetById(string id)
    {
        if (!ObjectIdHelper.TryParseObjectId(id, out var objectId)) return null;
        var entity = _database.PriceRecords.FindById(objectId);
        return entity?.ToPriceRecord();
    }

    public IEnumerable<PriceRecord> GetAll()
    {
        return _database.PriceRecords.FindAll().Select(x => x.ToPriceRecord());
    }

    public IEnumerable<PriceRecord> GetByItem(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return Enumerable.Empty<PriceRecord>();
        
        return _database.PriceRecords
            .Find(x => x.ItemId == objectItemId)
            .OrderByDescending(x => x.DateRecorded)
            .Select(x => x.ToPriceRecord());
    }

    public IEnumerable<PriceRecord> GetByPlace(string placeId)
    {
        if (!ObjectIdHelper.TryParseObjectId(placeId, out var objectPlaceId)) return Enumerable.Empty<PriceRecord>();
        
        return _database.PriceRecords
            .Find(x => x.PlaceId == objectPlaceId)
            .OrderByDescending(x => x.DateRecorded)
            .Select(x => x.ToPriceRecord());
    }

    public IEnumerable<PriceRecord> GetByItemAndPlace(string itemId, string placeId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId) || !ObjectIdHelper.TryParseObjectId(placeId, out var objectPlaceId)) 
            return Enumerable.Empty<PriceRecord>();
        
        return _database.PriceRecords
            .Find(x => x.ItemId == objectItemId && x.PlaceId == objectPlaceId)
            .OrderByDescending(x => x.DateRecorded)
            .Select(x => x.ToPriceRecord());
    }

    public PriceRecord? GetLatestPrice(string itemId, string placeId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId) || !ObjectIdHelper.TryParseObjectId(placeId, out var objectPlaceId)) 
            return null;
        
        var entity = _database.PriceRecords
            .Find(x => x.ItemId == objectItemId && x.PlaceId == objectPlaceId)
            .OrderByDescending(x => x.DateRecorded)
            .FirstOrDefault();
            
        return entity?.ToPriceRecord();
    }

    public IEnumerable<PriceRecord> GetCurrentSales()
    {
        var now = DateTime.UtcNow;
        return _database.PriceRecords
            .Find(x => x.IsOnSale && 
                      (!x.ValidTo.HasValue || x.ValidTo >= now) &&
                      (!x.ValidFrom.HasValue || x.ValidFrom <= now))
            .OrderBy(x => x.Price)
            .Select(x => x.ToPriceRecord());
    }

    public IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return Enumerable.Empty<PriceRecord>();
        
        var query = _database.PriceRecords.Find(x => x.ItemId == objectItemId);
        
        if (fromDate.HasValue)
            query = query.Where(x => x.DateRecorded >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(x => x.DateRecorded <= toDate.Value);
            
        return query.OrderBy(x => x.DateRecorded).Select(x => x.ToPriceRecord());
    }

    public decimal? GetLowestPrice(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return null;
        
        var records = _database.PriceRecords.Find(x => x.ItemId == objectItemId);
        return records.Any() ? records.Min(x => x.Price) : null;
    }

    public decimal? GetHighestPrice(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return null;
        
        var records = _database.PriceRecords.Find(x => x.ItemId == objectItemId);
        return records.Any() ? records.Max(x => x.Price) : null;
    }

    public decimal? GetAveragePrice(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return null;
        
        var records = _database.PriceRecords.Find(x => x.ItemId == objectItemId);
        return records.Any() ? records.Average(x => x.Price) : null;
    }

    public IEnumerable<PriceRecord> GetBestDeals(int count = 10)
    {
        // Returns items currently on sale with the biggest discount percentage
        return _database.PriceRecords
            .Find(x => x.IsOnSale && x.OriginalPrice.HasValue && x.OriginalPrice > x.Price)
            .OrderByDescending(x => (x.OriginalPrice!.Value - x.Price) / x.OriginalPrice.Value * 100)
            .Take(count)
            .Select(x => x.ToPriceRecord());
    }

    public IEnumerable<PriceRecord> ComparePrices(string itemId)
    {
        if (!ObjectIdHelper.TryParseObjectId(itemId, out var objectItemId)) return Enumerable.Empty<PriceRecord>();
        
        // Get the latest price for each place for a specific item
        return _database.PriceRecords
            .Find(x => x.ItemId == objectItemId)
            .GroupBy(x => x.PlaceId)
            .Select(g => g.OrderByDescending(x => x.DateRecorded).First())
            .OrderBy(x => x.Price)
            .Select(x => x.ToPriceRecord());
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
            .Take(count)
            .Select(x => x.ToPriceRecord());
    }

    public Dictionary<string, int> GetPriceRecordsBySource()
    {
        return _database.PriceRecords
            .Find(x => !string.IsNullOrEmpty(x.Source))
            .GroupBy(x => x.Source!)
            .ToDictionary(g => g.Key, g => g.Count());
    }
}