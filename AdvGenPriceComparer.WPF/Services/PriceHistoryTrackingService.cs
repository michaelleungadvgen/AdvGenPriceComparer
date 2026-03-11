using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service implementation for tracking historical price changes and providing price analysis
/// </summary>
public class PriceHistoryTrackingService : IPriceHistoryTrackingService
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly ILoggerService _logger;

    public PriceHistoryTrackingService(
        IPriceRecordRepository priceRecordRepository,
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        ILoggerService logger)
    {
        _priceRecordRepository = priceRecordRepository;
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public PriceRecord? RecordPriceObservation(string itemId, string placeId, decimal price, 
        decimal? originalPrice = null, bool isOnSale = false, string? source = null)
    {
        try
        {
            // Get the most recent price for this item at this store
            var latestPrice = _priceRecordRepository.GetLatestPrice(itemId, placeId);

            // If price hasn't changed, don't create a new record (idempotent)
            if (latestPrice != null && latestPrice.Price == price && latestPrice.IsOnSale == isOnSale)
            {
                _logger.LogInfo($"Price unchanged for item {itemId} at store {placeId}: ${price:F2}");
                return null;
            }

            // Create new price record
            var priceRecord = new PriceRecord
            {
                ItemId = itemId,
                PlaceId = placeId,
                Price = price,
                OriginalPrice = originalPrice,
                IsOnSale = isOnSale,
                Source = source ?? "manual",
                DateRecorded = DateTime.UtcNow,
                ValidFrom = DateTime.UtcNow,
                ValidTo = DateTime.UtcNow.AddDays(7)
            };

            var id = _priceRecordRepository.Add(priceRecord);
            priceRecord.Id = id;

            var changeType = latestPrice == null ? "new" : 
                price > latestPrice.Price ? "increased" : 
                price < latestPrice.Price ? "decreased" : "unchanged";

            _logger.LogInfo($"Price recorded for item {itemId} at store {placeId}: ${price:F2} ({changeType})");

            return priceRecord;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error recording price observation for item {itemId}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        return _priceRecordRepository.GetPriceHistory(itemId, fromDate, toDate);
    }

    /// <inheritdoc />
    public IEnumerable<PriceRecord> GetPriceHistoryForStore(string itemId, string placeId, 
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var history = _priceRecordRepository.GetByItemAndPlace(itemId, placeId);
        
        if (fromDate.HasValue)
            history = history.Where(r => r.DateRecorded >= fromDate.Value);
        
        if (toDate.HasValue)
            history = history.Where(r => r.DateRecorded <= toDate.Value);
        
        return history.OrderBy(r => r.DateRecorded);
    }

    /// <inheritdoc />
    public PriceStatistics GetPriceStatistics(string itemId, int daysBack = 90)
    {
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        var history = _priceRecordRepository.GetPriceHistory(itemId, fromDate).ToList();

        if (!history.Any())
        {
            return new PriceStatistics 
            { 
                ItemId = itemId,
                RecordCount = 0 
            };
        }

        var prices = history.Select(r => r.Price).ToList();
        var sortedPrices = prices.OrderBy(p => p).ToList();
        var count = prices.Count;

        // Calculate median
        decimal median;
        if (count % 2 == 0)
        {
            median = (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2;
        }
        else
        {
            median = sortedPrices[count / 2];
        }

        // Count actual price changes (excluding duplicate consecutive prices)
        var priceChanges = 0;
        for (int i = 1; i < history.Count; i++)
        {
            if (history[i].Price != history[i - 1].Price)
                priceChanges++;
        }

        var firstPrice = history.First().Price;
        var currentPrice = history.Last().Price;
        var priceChangePercent = firstPrice > 0 ? ((currentPrice - firstPrice) / firstPrice) * 100 : 0;

        return new PriceStatistics
        {
            ItemId = itemId,
            LowestPrice = prices.Min(),
            HighestPrice = prices.Max(),
            AveragePrice = prices.Average(),
            MedianPrice = median,
            CurrentPrice = currentPrice,
            RecordCount = count,
            FirstRecordedDate = history.First().DateRecorded,
            LastRecordedDate = history.Last().DateRecorded,
            PriceChangePercent = priceChangePercent,
            PriceChangeCount = priceChanges
        };
    }

    /// <inheritdoc />
    public PriceStatistics GetPriceStatisticsForStore(string itemId, string placeId, int daysBack = 90)
    {
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        var history = GetPriceHistoryForStore(itemId, placeId, fromDate).ToList();

        if (!history.Any())
        {
            return new PriceStatistics 
            { 
                ItemId = itemId,
                PlaceId = placeId,
                RecordCount = 0 
            };
        }

        var prices = history.Select(r => r.Price).ToList();
        var sortedPrices = prices.OrderBy(p => p).ToList();
        var count = prices.Count;

        // Calculate median
        decimal median;
        if (count % 2 == 0)
        {
            median = (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2;
        }
        else
        {
            median = sortedPrices[count / 2];
        }

        // Count actual price changes
        var priceChanges = 0;
        for (int i = 1; i < history.Count; i++)
        {
            if (history[i].Price != history[i - 1].Price)
                priceChanges++;
        }

        var firstPrice = history.First().Price;
        var currentPrice = history.Last().Price;
        var priceChangePercent = firstPrice > 0 ? ((currentPrice - firstPrice) / firstPrice) * 100 : 0;

        return new PriceStatistics
        {
            ItemId = itemId,
            PlaceId = placeId,
            LowestPrice = prices.Min(),
            HighestPrice = prices.Max(),
            AveragePrice = prices.Average(),
            MedianPrice = median,
            CurrentPrice = currentPrice,
            RecordCount = count,
            FirstRecordedDate = history.First().DateRecorded,
            LastRecordedDate = history.Last().DateRecorded,
            PriceChangePercent = priceChangePercent,
            PriceChangeCount = priceChanges
        };
    }

    /// <inheritdoc />
    public PriceChangeDetectionResult DetectPriceChange(string itemId, string placeId, 
        decimal currentPrice, decimal thresholdPercent = 10m)
    {
        var stats = GetPriceStatisticsForStore(itemId, placeId, 90);
        
        if (stats.RecordCount < 2)
        {
            return new PriceChangeDetectionResult
            {
                IsSignificantChange = false,
                ChangeType = PriceChangeType.NoChange,
                CurrentPrice = currentPrice,
                HistoricalAveragePrice = currentPrice,
                PercentageDifference = 0,
                IsAboveAverage = false,
                IsNearHistoricalLow = false,
                IsNearHistoricalHigh = false
            };
        }

        var percentageDiff = stats.AveragePrice > 0 
            ? ((currentPrice - stats.AveragePrice) / stats.AveragePrice) * 100 
            : 0;

        var isSignificant = Math.Abs(percentageDiff) >= thresholdPercent;
        
        var priceRange = stats.HighestPrice - stats.LowestPrice;
        var nearLowThreshold = priceRange * 0.1m; // Within 10% of price range from low
        var nearHighThreshold = priceRange * 0.1m; // Within 10% of price range from high

        return new PriceChangeDetectionResult
        {
            IsSignificantChange = isSignificant,
            ChangeType = percentageDiff > 0 ? PriceChangeType.Increase : 
                        percentageDiff < 0 ? PriceChangeType.Decrease : PriceChangeType.NoChange,
            CurrentPrice = currentPrice,
            HistoricalAveragePrice = stats.AveragePrice,
            PercentageDifference = percentageDiff,
            IsAboveAverage = currentPrice > stats.AveragePrice,
            IsNearHistoricalLow = currentPrice <= stats.LowestPrice + nearLowThreshold,
            IsNearHistoricalHigh = currentPrice >= stats.HighestPrice - nearHighThreshold
        };
    }

    /// <inheritdoc />
    public IEnumerable<PriceRecord> GetRecentPriceDrops(int daysBack = 7, decimal minDiscountPercent = 10m)
    {
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        var allRecords = _priceRecordRepository.GetAll()
            .Where(r => r.DateRecorded >= fromDate && r.IsOnSale && r.OriginalPrice.HasValue)
            .ToList();

        var priceDrops = new List<PriceRecord>();

        foreach (var record in allRecords)
        {
            if (record.OriginalPrice.HasValue && record.OriginalPrice > 0)
            {
                var discountPercent = ((record.OriginalPrice.Value - record.Price) / record.OriginalPrice.Value) * 100;
                if (discountPercent >= minDiscountPercent)
                {
                    priceDrops.Add(record);
                }
            }
        }

        return priceDrops.OrderByDescending(r => r.DateRecorded);
    }

    /// <inheritdoc />
    public PriceTrend GetPriceTrend(string itemId, string? placeId = null, int daysBack = 30)
    {
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        
        IEnumerable<PriceRecord> history;
        if (string.IsNullOrEmpty(placeId))
        {
            // Average across all stores
            history = _priceRecordRepository.GetPriceHistory(itemId, fromDate);
        }
        else
        {
            history = GetPriceHistoryForStore(itemId, placeId, fromDate);
        }

        var historyList = history.ToList();

        if (historyList.Count < 2)
        {
            return new PriceTrend
            {
                ItemId = itemId,
                Direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Stable,
                TrendStrength = 0,
                PercentageChange = 0,
                DaysAnalyzed = daysBack,
                Description = "Insufficient data to determine trend"
            };
        }

        var startPrice = historyList.First().Price;
        var endPrice = historyList.Last().Price;
        var percentageChange = startPrice > 0 ? ((endPrice - startPrice) / startPrice) * 100 : 0;

        // Calculate volatility (standard deviation)
        var prices = historyList.Select(r => r.Price).ToList();
        var avgPrice = prices.Average();
        var variance = prices.Average(p => (double)((p - avgPrice) * (p - avgPrice)));
        var stdDev = (decimal)Math.Sqrt(variance);
        var volatility = avgPrice > 0 ? (stdDev / avgPrice) * 100 : 0;

        // Determine trend direction and strength
        var direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Stable;
        decimal trendStrength;
        string description;

        if (volatility > 15m)
        {
            direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Stable;  // High volatility
            trendStrength = Math.Min(volatility / 30m, 1m);
            description = $"Price is volatile with {volatility:F1}% standard deviation";
        }
        else if (Math.Abs(percentageChange) < 2m)
        {
            direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Stable;
            trendStrength = 0;
            description = "Price is stable with minimal change";
        }
        else if (percentageChange > 0)
        {
            direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Rising;
            trendStrength = Math.Min(Math.Abs(percentageChange) / 20m, 1m);
            description = $"Price is rising ({percentageChange:F1}% over {daysBack} days)";
        }
        else
        {
            direction = AdvGenPriceComparer.Core.Interfaces.PriceTrendDirection.Falling;
            trendStrength = Math.Min(Math.Abs(percentageChange) / 20m, 1m);
            description = $"Price is falling ({Math.Abs(percentageChange):F1}% over {daysBack} days)";
        }

        return new PriceTrend
        {
            ItemId = itemId,
            Direction = direction,
            TrendStrength = trendStrength,
            PercentageChange = percentageChange,
            StartPrice = startPrice,
            EndPrice = endPrice,
            DaysAnalyzed = daysBack,
            Description = description
        };
    }

    /// <inheritdoc />
    public decimal CalculatePriceVolatility(string itemId, int daysBack = 90)
    {
        var stats = GetPriceStatistics(itemId, daysBack);
        
        if (stats.RecordCount < 2)
            return 0;

        var history = GetPriceHistory(itemId, DateTime.UtcNow.AddDays(-daysBack)).ToList();
        var prices = history.Select(r => r.Price).ToList();
        var avgPrice = prices.Average();
        
        if (avgPrice == 0)
            return 0;

        // Calculate standard deviation
        var variance = prices.Average(p => (double)((p - avgPrice) * (p - avgPrice)));
        var stdDev = (decimal)Math.Sqrt(variance);
        
        // Return coefficient of variation (standard deviation as percentage of mean)
        return (stdDev / avgPrice) * 100;
    }

    /// <inheritdoc />
    public BestBuyingOpportunity GetBestBuyingOpportunity(string itemId, int daysBack = 365)
    {
        var fromDate = DateTime.UtcNow.AddDays(-daysBack);
        var history = _priceRecordRepository.GetPriceHistory(itemId, fromDate).ToList();

        if (!history.Any())
        {
            return new BestBuyingOpportunity
            {
                ItemId = itemId,
                Recommendation = "No price history available"
            };
        }

        // Find best price
        var bestPriceRecord = history.OrderBy(r => r.Price).First();
        var averagePrice = history.Average(r => r.Price);
        var potentialSavings = averagePrice - bestPriceRecord.Price;

        // Analyze by month and day of week
        var byMonth = history
            .GroupBy(r => r.DateRecorded.Month)
            .Select(g => new { Month = g.Key, AvgPrice = g.Average(r => r.Price) })
            .OrderBy(x => x.AvgPrice)
            .FirstOrDefault();

        var byDayOfWeek = history
            .GroupBy(r => (int)r.DateRecorded.DayOfWeek)
            .Select(g => new { Day = g.Key, AvgPrice = g.Average(r => r.Price) })
            .OrderBy(x => x.AvgPrice)
            .FirstOrDefault();

        // Get store name for best price
        string? storeName = null;
        try
        {
            var store = _placeRepository.GetById(bestPriceRecord.PlaceId);
            storeName = store?.Name;
        }
        catch
        {
            // Ignore if store not found
        }

        var currentStats = GetPriceStatistics(itemId, 30);
        var recommendation = currentStats.CurrentPrice <= bestPriceRecord.Price * 1.05m 
            ? "Buy now - price is near historical low!" 
            : $"Wait for sale. Historical low was ${bestPriceRecord.Price:F2} ({potentialSavings / averagePrice * 100:F0}% below average)";

        return new BestBuyingOpportunity
        {
            ItemId = itemId,
            BestPrice = bestPriceRecord.Price,
            BestPriceDate = bestPriceRecord.DateRecorded,
            BestPriceStore = storeName,
            AveragePrice = averagePrice,
            PotentialSavings = potentialSavings,
            BestMonth = byMonth?.Month,
            BestDayOfWeek = byDayOfWeek?.Day,
            Recommendation = recommendation
        };
    }

    /// <inheritdoc />
    public IEnumerable<PriceRecord> ExportPriceHistory(string? itemId = null, 
        DateTime? fromDate = null, DateTime? toDate = null)
    {
        var records = _priceRecordRepository.GetAll();

        if (!string.IsNullOrEmpty(itemId))
            records = records.Where(r => r.ItemId == itemId);

        if (fromDate.HasValue)
            records = records.Where(r => r.DateRecorded >= fromDate.Value);

        if (toDate.HasValue)
            records = records.Where(r => r.DateRecorded <= toDate.Value);

        return records.OrderBy(r => r.DateRecorded);
    }

    /// <inheritdoc />
    public int GetTotalPriceRecordsCount()
    {
        return _priceRecordRepository.GetTotalRecordsCount();
    }

    /// <inheritdoc />
    public int GetRecentPriceRecordsCount(int days = 30)
    {
        return _priceRecordRepository.GetAll()
            .Count(r => r.DateRecorded >= DateTime.UtcNow.AddDays(-days));
    }
}
