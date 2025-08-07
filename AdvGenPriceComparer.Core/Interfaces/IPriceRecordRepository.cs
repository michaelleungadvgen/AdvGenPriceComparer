using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IPriceRecordRepository
{
    ObjectId Add(PriceRecord priceRecord);
    bool Update(PriceRecord priceRecord);
    bool Delete(ObjectId id);
    PriceRecord? GetById(ObjectId id);
    IEnumerable<PriceRecord> GetAll();
    IEnumerable<PriceRecord> GetByItem(ObjectId itemId);
    IEnumerable<PriceRecord> GetByPlace(ObjectId placeId);
    IEnumerable<PriceRecord> GetByItemAndPlace(ObjectId itemId, ObjectId placeId);
    PriceRecord? GetLatestPrice(ObjectId itemId, ObjectId placeId);
    IEnumerable<PriceRecord> GetCurrentSales();
    IEnumerable<PriceRecord> GetPriceHistory(ObjectId itemId, DateTime? fromDate = null, DateTime? toDate = null);
    decimal? GetLowestPrice(ObjectId itemId);
    decimal? GetHighestPrice(ObjectId itemId);
    decimal? GetAveragePrice(ObjectId itemId);
    IEnumerable<PriceRecord> GetBestDeals(int count = 10);
    IEnumerable<PriceRecord> ComparePrices(ObjectId itemId);
    int GetTotalRecordsCount();
    int GetRecordsCountThisWeek();
    int GetSaleRecordsCount();
    IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10);
    Dictionary<string, int> GetPriceRecordsBySource();
}