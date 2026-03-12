using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for identifying and highlighting best prices across stores
/// </summary>
public interface IBestPriceService
{
    /// <summary>
    /// Get best price information for a specific item
    /// </summary>
    BestPriceInfo? GetBestPriceInfo(string itemId);
    
    /// <summary>
    /// Get best price information for all items at a specific store
    /// </summary>
    IEnumerable<BestPriceInfo> GetBestPricesAtStore(string placeId);
    
    /// <summary>
    /// Get all best price deals (items that are at their lowest price)
    /// </summary>
    IEnumerable<BestPriceInfo> GetAllBestDeals(int count = 20);
    
    /// <summary>
    /// Get items with significant savings (above threshold percentage)
    /// </summary>
    IEnumerable<BestPriceInfo> GetBestSavings(decimal minSavingsPercent = 20);
    
    /// <summary>
    /// Compare prices for an item across all stores
    /// </summary>
    PriceComparisonResult ComparePricesAcrossStores(string itemId);
    
    /// <summary>
    /// Check if current price is the best (historical low)
    /// </summary>
    bool IsCurrentPriceBest(string itemId, string placeId);
    
    /// <summary>
    /// Get price trend analysis for an item
    /// </summary>
    PriceTrendInfo GetPriceTrend(string itemId);
}

/// <summary>
/// Information about the best price for an item
/// </summary>
public class BestPriceInfo
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    
    // Current price info
    public decimal CurrentPrice { get; set; }
    public string CurrentStoreId { get; set; } = string.Empty;
    public string CurrentStoreName { get; set; } = string.Empty;
    
    // Best price info
    public decimal BestPrice { get; set; }
    public string BestStoreId { get; set; } = string.Empty;
    public string BestStoreName { get; set; } = string.Empty;
    public DateTime? BestPriceDate { get; set; }
    
    // Historical info
    public decimal? HistoricalLow { get; set; }
    public decimal? HistoricalHigh { get; set; }
    public decimal? AveragePrice { get; set; }
    
    // Savings info
    public decimal? SavingsAmount { get; set; }
    public decimal? SavingsPercent { get; set; }
    public bool IsOnSale { get; set; }
    public bool IsHistoricalLow { get; set; }
    public bool IsBestDeal { get; set; }
    
    // Highlight level
    public PriceHighlightLevel HighlightLevel { get; set; }
}

/// <summary>
/// Price highlight levels for UI indication
/// </summary>
public enum PriceHighlightLevel
{
    None,
    GoodDeal,      // Green - better than average
    GreatDeal,     // Blue - significant savings
    BestPrice      // Gold/Orange - historical low
}

/// <summary>
/// Result of price comparison across stores
/// </summary>
public class PriceComparisonResult
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public List<StorePriceInfo> StorePrices { get; set; } = new();
    public StorePriceInfo? BestCurrentPrice { get; set; }
    public decimal? HistoricalLow { get; set; }
    public decimal? HistoricalHigh { get; set; }
    public decimal? AveragePrice { get; set; }
}

/// <summary>
/// Price information for a specific store
/// </summary>
public class StorePriceInfo
{
    public string StoreId { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime PriceDate { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? OriginalPrice { get; set; }
    public bool IsBestPrice { get; set; }
    public bool IsHistoricalLow { get; set; }
    public decimal? SavingsVsAverage { get; set; }
}

/// <summary>
/// Price trend information
/// </summary>
public class PriceTrendInfo
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public PriceTrendDirection Trend { get; set; }
    public decimal TrendPercent { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal? PreviousPrice { get; set; }
    public DateTime? PreviousPriceDate { get; set; }
    public decimal? Average30Day { get; set; }
    public decimal? Average90Day { get; set; }
    public int DataPoints { get; set; }
    public string? Recommendation { get; set; }
}

public enum PriceTrendDirection
{
    Rising,
    Falling,
    Stable,
    Unknown
}
