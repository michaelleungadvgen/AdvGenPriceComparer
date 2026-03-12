using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service implementation for identifying and highlighting best prices
/// </summary>
public class BestPriceService : IBestPriceService
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILoggerService _logger;

    public BestPriceService(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILoggerService logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public BestPriceInfo? GetBestPriceInfo(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                _logger.LogWarning($"Item not found: {itemId}");
                return null;
            }

            // Get all current prices for this item
            var allPrices = _priceRecordRepository.ComparePrices(itemId).ToList();
            if (!allPrices.Any())
            {
                _logger.LogWarning($"No price records found for item: {itemId}");
                return null;
            }

            // Get historical stats
            var historicalLow = _priceRecordRepository.GetLowestPrice(itemId);
            var historicalHigh = _priceRecordRepository.GetHighestPrice(itemId);
            var averagePrice = _priceRecordRepository.GetAveragePrice(itemId);

            // Find best current price
            var bestCurrent = allPrices.OrderBy(p => p.Price).First();
            var store = _placeRepository.GetById(bestCurrent.PlaceId);

            // Get the latest price record for best price info
            var latestRecord = allPrices.FirstOrDefault();
            
            var info = new BestPriceInfo
            {
                ItemId = itemId,
                ItemName = item.Name,
                Brand = item.Brand,
                Category = item.Category,
                
                CurrentPrice = bestCurrent.Price,
                CurrentStoreId = bestCurrent.PlaceId,
                CurrentStoreName = store?.Name ?? "Unknown",
                
                BestPrice = bestCurrent.Price,
                BestStoreId = bestCurrent.PlaceId,
                BestStoreName = store?.Name ?? "Unknown",
                BestPriceDate = bestCurrent.DateRecorded,
                
                HistoricalLow = historicalLow,
                HistoricalHigh = historicalHigh,
                AveragePrice = averagePrice,
                
                IsOnSale = bestCurrent.IsOnSale,
                IsHistoricalLow = historicalLow.HasValue && bestCurrent.Price <= historicalLow.Value * 1.05m,
                IsBestDeal = historicalLow.HasValue && bestCurrent.Price <= historicalLow.Value * 1.02m
            };

            // Calculate savings
            if (averagePrice.HasValue && averagePrice > 0)
            {
                info.SavingsAmount = averagePrice - bestCurrent.Price;
                info.SavingsPercent = ((averagePrice - bestCurrent.Price) / averagePrice) * 100;
            }

            // Determine highlight level
            info.HighlightLevel = DetermineHighlightLevel(info);

            return info;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting best price info for item {itemId}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public IEnumerable<BestPriceInfo> GetBestPricesAtStore(string placeId)
    {
        try
        {
            var store = _placeRepository.GetById(placeId);
            if (store == null)
            {
                _logger.LogWarning($"Store not found: {placeId}");
                return Enumerable.Empty<BestPriceInfo>();
            }

            var priceRecords = _priceRecordRepository.GetByPlace(placeId);
            var bestPriceInfos = new List<BestPriceInfo>();

            foreach (var record in priceRecords)
            {
                var info = GetBestPriceInfo(record.ItemId);
                if (info != null)
                {
                    bestPriceInfos.Add(info);
                }
            }

            return bestPriceInfos.OrderByDescending(b => b.HighlightLevel);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting best prices at store {placeId}", ex);
            return Enumerable.Empty<BestPriceInfo>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<BestPriceInfo> GetAllBestDeals(int count = 20)
    {
        try
        {
            var allItems = _itemRepository.GetAll();
            var bestDeals = new List<BestPriceInfo>();

            foreach (var item in allItems)
            {
                var info = GetBestPriceInfo(item.Id);
                if (info != null && (info.IsBestDeal || info.IsHistoricalLow || info.SavingsPercent > 10))
                {
                    bestDeals.Add(info);
                }
            }

            return bestDeals
                .OrderByDescending(b => b.HighlightLevel)
                .ThenByDescending(b => b.SavingsPercent)
                .Take(count);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error getting all best deals", ex);
            return Enumerable.Empty<BestPriceInfo>();
        }
    }

    /// <inheritdoc />
    public IEnumerable<BestPriceInfo> GetBestSavings(decimal minSavingsPercent = 20)
    {
        try
        {
            var allItems = _itemRepository.GetAll();
            var bestSavings = new List<BestPriceInfo>();

            foreach (var item in allItems)
            {
                var info = GetBestPriceInfo(item.Id);
                if (info != null && info.SavingsPercent >= minSavingsPercent)
                {
                    bestSavings.Add(info);
                }
            }

            return bestSavings
                .OrderByDescending(b => b.SavingsPercent)
                .ThenBy(b => b.ItemName);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting best savings with threshold {minSavingsPercent}%", ex);
            return Enumerable.Empty<BestPriceInfo>();
        }
    }

    /// <inheritdoc />
    public PriceComparisonResult ComparePricesAcrossStores(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                return new PriceComparisonResult { ItemId = itemId };
            }

            var priceRecords = _priceRecordRepository.ComparePrices(itemId).ToList();
            var historicalLow = _priceRecordRepository.GetLowestPrice(itemId);
            var historicalHigh = _priceRecordRepository.GetHighestPrice(itemId);
            var averagePrice = _priceRecordRepository.GetAveragePrice(itemId);

            var storePrices = new List<StorePriceInfo>();
            StorePriceInfo? bestPrice = null;

            foreach (var record in priceRecords)
            {
                var store = _placeRepository.GetById(record.PlaceId);
                var isHistoricalLow = historicalLow.HasValue && record.Price <= historicalLow.Value * 1.01m;
                
                var storePrice = new StorePriceInfo
                {
                    StoreId = record.PlaceId,
                    StoreName = store?.Name ?? "Unknown",
                    Price = record.Price,
                    PriceDate = record.DateRecorded,
                    IsOnSale = record.IsOnSale,
                    OriginalPrice = record.OriginalPrice,
                    IsHistoricalLow = isHistoricalLow,
                    SavingsVsAverage = averagePrice.HasValue ? averagePrice - record.Price : null
                };

                storePrices.Add(storePrice);

                if (bestPrice == null || record.Price < bestPrice.Price)
                {
                    bestPrice = storePrice;
                }
            }

            if (bestPrice != null)
            {
                bestPrice.IsBestPrice = true;
            }

            return new PriceComparisonResult
            {
                ItemId = itemId,
                ItemName = item.Name,
                StorePrices = storePrices,
                BestCurrentPrice = bestPrice,
                HistoricalLow = historicalLow,
                HistoricalHigh = historicalHigh,
                AveragePrice = averagePrice
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error comparing prices across stores for item {itemId}", ex);
            return new PriceComparisonResult { ItemId = itemId };
        }
    }

    /// <inheritdoc />
    public bool IsCurrentPriceBest(string itemId, string placeId)
    {
        try
        {
            var historicalLow = _priceRecordRepository.GetLowestPrice(itemId);
            if (!historicalLow.HasValue)
            {
                return false;
            }

            var currentPrice = _priceRecordRepository.GetLatestPrice(itemId, placeId);
            if (currentPrice == null)
            {
                return false;
            }

            // Consider it "best" if within 2% of historical low
            return currentPrice.Price <= historicalLow.Value * 1.02m;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking if current price is best for item {itemId} at store {placeId}", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public PriceTrendInfo GetPriceTrend(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                return new PriceTrendInfo { ItemId = itemId };
            }

            // Get price history for last 90 days
            var fromDate = DateTime.Now.AddDays(-90);
            var history = _priceRecordRepository.GetPriceHistory(itemId, fromDate).ToList();

            if (history.Count < 2)
            {
                return new PriceTrendInfo 
                { 
                    ItemId = itemId, 
                    ItemName = item.Name,
                    Trend = PriceTrendDirection.Unknown 
                };
            }

            var currentPrice = history.Last();
            var previousPrice = history.Count > 1 ? history[^2] : null;
            
            var prices = history.Select(h => h.Price).ToList();
            var average30Day = history
                .Where(h => h.DateRecorded >= DateTime.Now.AddDays(-30))
                .Select(h => h.Price)
                .DefaultIfEmpty(0)
                .Average();
            
            var average90Day = prices.Average();

            // Calculate trend
            var trend = PriceTrendDirection.Stable;
            decimal trendPercent = 0;

            if (previousPrice != null && previousPrice.Price > 0)
            {
                var change = currentPrice.Price - previousPrice.Price;
                trendPercent = (change / previousPrice.Price) * 100;

                if (trendPercent > 5)
                    trend = PriceTrendDirection.Rising;
                else if (trendPercent < -5)
                    trend = PriceTrendDirection.Falling;
            }

            // Generate recommendation
            string? recommendation = null;
            if (trend == PriceTrendDirection.Rising)
            {
                recommendation = "Price is rising. Consider buying soon if you need this item.";
            }
            else if (trend == PriceTrendDirection.Falling)
            {
                recommendation = "Price is falling. Waiting may result in better deals.";
            }
            else
            {
                recommendation = "Price is stable. Buy when convenient.";
            }

            return new PriceTrendInfo
            {
                ItemId = itemId,
                ItemName = item.Name,
                Trend = trend,
                TrendPercent = trendPercent,
                CurrentPrice = currentPrice.Price,
                PreviousPrice = previousPrice?.Price,
                PreviousPriceDate = previousPrice?.DateRecorded,
                Average30Day = average30Day > 0 ? average30Day : null,
                Average90Day = average90Day,
                DataPoints = history.Count,
                Recommendation = recommendation
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting price trend for item {itemId}", ex);
            return new PriceTrendInfo { ItemId = itemId };
        }
    }

    /// <summary>
    /// Determine the highlight level based on price comparison
    /// </summary>
    private PriceHighlightLevel DetermineHighlightLevel(BestPriceInfo info)
    {
        // Best price - historical low or very close
        if (info.IsHistoricalLow || info.IsBestDeal)
        {
            return PriceHighlightLevel.BestPrice;
        }

        // Great deal - significant savings (>20%)
        if (info.SavingsPercent >= 20)
        {
            return PriceHighlightLevel.GreatDeal;
        }

        // Good deal - moderate savings (>10%) or on sale
        if (info.SavingsPercent >= 10 || (info.IsOnSale && info.SavingsPercent > 5))
        {
            return PriceHighlightLevel.GoodDeal;
        }

        return PriceHighlightLevel.None;
    }
}
