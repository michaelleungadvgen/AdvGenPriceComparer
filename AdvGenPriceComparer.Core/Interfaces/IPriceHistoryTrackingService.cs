using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Service for tracking historical price changes and providing price analysis
/// </summary>
public interface IPriceHistoryTrackingService
{
    /// <summary>
    /// Records a price observation for an item. If the price has changed from the last recorded price,
    /// a new PriceRecord is created. If the price is the same, no record is created (idempotent).
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="placeId">The store/place ID</param>
    /// <param name="price">The current price</param>
    /// <param name="originalPrice">Optional original price (for sales)</param>
    /// <param name="isOnSale">Whether the item is on sale</param>
    /// <param name="source">Source of the price data (e.g., "import", "manual", "api")</param>
    /// <returns>The created PriceRecord, or null if price hasn't changed</returns>
    PriceRecord? RecordPriceObservation(string itemId, string placeId, decimal price, 
        decimal? originalPrice = null, bool isOnSale = false, string? source = null);

    /// <summary>
    /// Gets the price history for a specific item across all stores
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>List of price records ordered by date</returns>
    IEnumerable<PriceRecord> GetPriceHistory(string itemId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets the price history for a specific item at a specific store
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="placeId">The store/place ID</param>
    /// <param name="fromDate">Optional start date filter</param>
    /// <param name="toDate">Optional end date filter</param>
    /// <returns>List of price records ordered by date</returns>
    IEnumerable<PriceRecord> GetPriceHistoryForStore(string itemId, string placeId, 
        DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets price statistics for an item (lowest, highest, average prices)
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="daysBack">Number of days to look back (default: 90)</param>
    /// <returns>Price statistics</returns>
    PriceStatistics GetPriceStatistics(string itemId, int daysBack = 90);

    /// <summary>
    /// Gets price statistics for an item at a specific store
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="placeId">The store/place ID</param>
    /// <param name="daysBack">Number of days to look back (default: 90)</param>
    /// <returns>Price statistics</returns>
    PriceStatistics GetPriceStatisticsForStore(string itemId, string placeId, int daysBack = 90);

    /// <summary>
    /// Detects if the current price is significantly different from the historical average
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="placeId">The store/place ID</param>
    /// <param name="currentPrice">The current price to compare</param>
    /// <param name="thresholdPercent">Percentage threshold for significance (default: 10%)</param>
    /// <returns>Price change detection result</returns>
    PriceChangeDetectionResult DetectPriceChange(string itemId, string placeId, 
        decimal currentPrice, decimal thresholdPercent = 10m);

    /// <summary>
    /// Gets items with price drops (sales) within the specified period
    /// </summary>
    /// <param name="daysBack">Number of days to look back</param>
    /// <param name="minDiscountPercent">Minimum discount percentage</param>
    /// <returns>List of price records representing price drops</returns>
    IEnumerable<PriceRecord> GetRecentPriceDrops(int daysBack = 7, decimal minDiscountPercent = 10m);

    /// <summary>
    /// Gets price trend for an item (rising, falling, or stable)
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="placeId">Optional store/place ID (if null, averages across all stores)</param>
    /// <param name="daysBack">Number of days to analyze</param>
    /// <returns>Price trend information</returns>
    PriceTrend GetPriceTrend(string itemId, string? placeId = null, int daysBack = 30);

    /// <summary>
    /// Calculates the price volatility (standard deviation of prices) for an item
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="daysBack">Number of days to analyze</param>
    /// <returns>Volatility as a percentage of average price</returns>
    decimal CalculatePriceVolatility(string itemId, int daysBack = 90);

    /// <summary>
    /// Gets the best time to buy (historically lowest price period) for an item
    /// </summary>
    /// <param name="itemId">The item ID</param>
    /// <param name="daysBack">Number of days to analyze</param>
    /// <returns>Information about the best buying opportunity</returns>
    BestBuyingOpportunity GetBestBuyingOpportunity(string itemId, int daysBack = 365);

    /// <summary>
    /// Exports price history data for analysis
    /// </summary>
    /// <param name="itemId">Optional item ID filter (if null, exports all)</param>
    /// <param name="fromDate">Optional start date</param>
    /// <param name="toDate">Optional end date</param>
    /// <returns>List of price records for export</returns>
    IEnumerable<PriceRecord> ExportPriceHistory(string? itemId = null, 
        DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Gets the number of price records in the database
    /// </summary>
    /// <returns>Total count of price records</returns>
    int GetTotalPriceRecordsCount();

    /// <summary>
    /// Gets the number of price records created in the last N days
    /// </summary>
    /// <param name="days">Number of days</param>
    /// <returns>Count of recent price records</returns>
    int GetRecentPriceRecordsCount(int days = 30);
}

/// <summary>
/// Price statistics for an item
/// </summary>
public class PriceStatistics
{
    /// <summary>
    /// The item ID
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// The store/place ID (if store-specific)
    /// </summary>
    public string? PlaceId { get; set; }

    /// <summary>
    /// Lowest price recorded
    /// </summary>
    public decimal LowestPrice { get; set; }

    /// <summary>
    /// Highest price recorded
    /// </summary>
    public decimal HighestPrice { get; set; }

    /// <summary>
    /// Average price
    /// </summary>
    public decimal AveragePrice { get; set; }

    /// <summary>
    /// Median price
    /// </summary>
    public decimal MedianPrice { get; set; }

    /// <summary>
    /// Current/latest price
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Number of price records
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Date of first price record
    /// </summary>
    public DateTime FirstRecordedDate { get; set; }

    /// <summary>
    /// Date of most recent price record
    /// </summary>
    public DateTime LastRecordedDate { get; set; }

    /// <summary>
    /// Price change from first to current (as percentage)
    /// </summary>
    public decimal PriceChangePercent { get; set; }

    /// <summary>
    /// Number of times the price has changed
    /// </summary>
    public int PriceChangeCount { get; set; }
}

/// <summary>
/// Result of price change detection
/// </summary>
public class PriceChangeDetectionResult
{
    /// <summary>
    /// Whether a significant price change was detected
    /// </summary>
    public bool IsSignificantChange { get; set; }

    /// <summary>
    /// Type of change (increase, decrease, or no change)
    /// </summary>
    public PriceChangeType ChangeType { get; set; }

    /// <summary>
    /// The current price being compared
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// The historical average price
    /// </summary>
    public decimal HistoricalAveragePrice { get; set; }

    /// <summary>
    /// Percentage difference from historical average
    /// </summary>
    public decimal PercentageDifference { get; set; }

    /// <summary>
    /// Whether the current price is higher than average
    /// </summary>
    public bool IsAboveAverage { get; set; }

    /// <summary>
    /// Whether the current price is at or near historical low
    /// </summary>
    public bool IsNearHistoricalLow { get; set; }

    /// <summary>
    /// Whether the current price is at or near historical high
    /// </summary>
    public bool IsNearHistoricalHigh { get; set; }
}

/// <summary>
/// Type of price change
/// </summary>
public enum PriceChangeType
{
    NoChange,
    Increase,
    Decrease
}

/// <summary>
/// Price trend direction
/// </summary>
public enum PriceTrendDirection
{
    Rising,
    Falling,
    Stable,
    Volatile
}

/// <summary>
/// Price trend information
/// </summary>
public class PriceTrend
{
    /// <summary>
    /// The item ID
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Direction of the trend
    /// </summary>
    public PriceTrendDirection Direction { get; set; }

    /// <summary>
    /// Trend strength (0.0 to 1.0, where 1.0 is very strong)
    /// </summary>
    public decimal TrendStrength { get; set; }

    /// <summary>
    /// Percentage change over the period
    /// </summary>
    public decimal PercentageChange { get; set; }

    /// <summary>
    /// Starting price of the period
    /// </summary>
    public decimal StartPrice { get; set; }

    /// <summary>
    /// Ending price of the period
    /// </summary>
    public decimal EndPrice { get; set; }

    /// <summary>
    /// Number of days analyzed
    /// </summary>
    public int DaysAnalyzed { get; set; }

    /// <summary>
    /// Human-readable description of the trend
    /// </summary>
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Information about the best buying opportunity
/// </summary>
public class BestBuyingOpportunity
{
    /// <summary>
    /// The item ID
    /// </summary>
    public string ItemId { get; set; } = string.Empty;

    /// <summary>
    /// Best (lowest) price recorded
    /// </summary>
    public decimal BestPrice { get; set; }

    /// <summary>
    /// Date when best price was recorded
    /// </summary>
    public DateTime BestPriceDate { get; set; }

    /// <summary>
    /// Store where best price was found
    /// </summary>
    public string? BestPriceStore { get; set; }

    /// <summary>
    /// Average price over the period
    /// </summary>
    public decimal AveragePrice { get; set; }

    /// <summary>
    /// Savings if buying at best price vs average
    /// </summary>
    public decimal PotentialSavings { get; set; }

    /// <summary>
    /// Best month to buy (based on historical data)
    /// </summary>
    public int? BestMonth { get; set; }

    /// <summary>
    /// Best day of week to buy (0 = Sunday, 6 = Saturday)
    /// </summary>
    public int? BestDayOfWeek { get; set; }

    /// <summary>
    /// Recommended action based on analysis
    /// </summary>
    public string Recommendation { get; set; } = string.Empty;
}
