using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

public interface IPriceRecordRepository
{
    string Add(PriceRecord priceRecord);
    bool Update(PriceRecord priceRecord);
    bool Delete(string id);
    PriceRecord? GetById(string id);
    IEnumerable<PriceRecord> GetAll();
    IEnumerable<PriceRecord> GetByItem(string itemId);
    IEnumerable<PriceRecord> GetByPlace(string placeId);
    IEnumerable<PriceRecord> GetByItemAndPlace(string itemId, string placeId);
    PriceRecord? GetLatestPrice(string itemId, string placeId);
    IEnumerable<PriceRecord> GetCurrentSales();
    IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? fromDate = null, DateTime? toDate = null);
    decimal? GetLowestPrice(string itemId);
    decimal? GetHighestPrice(string itemId);
    decimal? GetAveragePrice(string itemId);
    IEnumerable<PriceRecord> GetBestDeals(int count = 10);
    IEnumerable<PriceRecord> ComparePrices(string itemId);
    int GetTotalRecordsCount();
    int GetRecordsCountThisWeek();
    int GetSaleRecordsCount();
    IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10);
    Dictionary<string, int> GetPriceRecordsBySource();
}