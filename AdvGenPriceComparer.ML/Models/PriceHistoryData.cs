using Microsoft.ML.Data;

namespace AdvGenPriceComparer.ML.Models;

/// <summary>
/// Input data model for price history time series analysis
/// </summary>
public class PriceHistoryData
{
    [LoadColumn(0)]
    public string ItemId { get; set; } = string.Empty;

    [LoadColumn(1)]
    public string ItemName { get; set; } = string.Empty;

    [LoadColumn(2)]
    public DateTime Date { get; set; }

    [LoadColumn(3)]
    public float Price { get; set; }

    [LoadColumn(4)]
    public bool IsOnSale { get; set; }

    [LoadColumn(5)]
    public string Store { get; set; } = string.Empty;

    [LoadColumn(6)]
    public string Category { get; set; } = string.Empty;

    [LoadColumn(7)]
    public float DayOfWeek { get; set; }

    [LoadColumn(8)]
    public float WeekOfYear { get; set; }

    [LoadColumn(9)]
    public float Month { get; set; }

    /// <summary>
    /// 7-day moving average (calculated feature)
    /// </summary>
    public float MovingAverage7Days { get; set; }

    /// <summary>
    /// 30-day moving average (calculated feature)
    /// </summary>
    public float MovingAverage30Days { get; set; }

    /// <summary>
    /// Price change from previous day
    /// </summary>
    public float PriceChange { get; set; }

    /// <summary>
    /// Price volatility (standard deviation over 7 days)
    /// </summary>
    public float Volatility7Days { get; set; }
}

/// <summary>
/// Output model for price forecasting using ML.NET TimeSeries
/// </summary>
public class PriceForecastOutput
{
    /// <summary>
    /// Forecasted price value
    /// </summary>
    [ColumnName("Score")]
    public float[] ForecastedPrices { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Lower bound of confidence interval
    /// </summary>
    [ColumnName("LowerBound")]
    public float[] LowerBounds { get; set; } = Array.Empty<float>();

    /// <summary>
    /// Upper bound of confidence interval
    /// </summary>
    [ColumnName("UpperBound")]
    public float[] UpperBounds { get; set; } = Array.Empty<float>();
}

/// <summary>
/// Price trend direction
/// </summary>
public enum PriceTrend
{
    Rising,
    Falling,
    Stable
}

/// <summary>
/// Buying recommendation based on price forecasting
/// </summary>
public enum BuyingRecommendation
{
    BuyNow,      // Price is at or near historical low
    Wait,        // Price expected to drop soon
    NormalTime,  // No significant trend
    AvoidHighPrice // Currently unusually high
}

/// <summary>
/// Single price forecast for a specific date
/// </summary>
public class PriceForecast
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime ForecastDate { get; set; }

    /// <summary>
    /// Predicted price
    /// </summary>
    public float PredictedPrice { get; set; }

    /// <summary>
    /// Confidence interval (upper bound - lower bound)
    /// </summary>
    public float ConfidenceInterval { get; set; }

    /// <summary>
    /// Lower bound of 95% confidence interval
    /// </summary>
    public float LowerBound { get; set; }

    /// <summary>
    /// Upper bound of 95% confidence interval
    /// </summary>
    public float UpperBound { get; set; }

    /// <summary>
    /// Price trend direction
    /// </summary>
    public PriceTrend Trend { get; set; }

    /// <summary>
    /// Trend strength from 0.0 to 1.0
    /// </summary>
    public float TrendStrength { get; set; }

    /// <summary>
    /// Buying recommendation
    /// </summary>
    public BuyingRecommendation Recommendation { get; set; }

    /// <summary>
    /// Human-readable recommendation reason
    /// </summary>
    public string RecommendationReason { get; set; } = string.Empty;
}

/// <summary>
/// Anomaly type classification
/// </summary>
public enum AnomalyType
{
    PriceSpike,      // Unusual price increase
    PriceDrop,       // Unusual price decrease
    Seasonal,        // Expected seasonal variation
    IllusoryDiscount // "Sale" price is actually normal or high
}

/// <summary>
/// Price anomaly detection result
/// </summary>
public class PriceAnomaly
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public float ActualPrice { get; set; }
    public float ExpectedPrice { get; set; }
    public float Deviation { get; set; }

    /// <summary>
    /// Whether this is an anomaly
    /// </summary>
    [ColumnName("PredictedLabel")]
    public bool IsAnomaly { get; set; }

    /// <summary>
    /// Anomaly confidence score
    /// </summary>
    [ColumnName("Score")]
    public float AnomalyScore { get; set; }

    /// <summary>
    /// Type of anomaly
    /// </summary>
    public AnomalyType Type { get; set; }

    /// <summary>
    /// Human-readable description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// P-value for statistical significance
    /// </summary>
    public double PValue { get; set; }
}

/// <summary>
/// Summary statistics for price history
/// </summary>
public class PriceStatistics
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public int DataPoints { get; set; }
    public float AveragePrice { get; set; }
    public float MinPrice { get; set; }
    public float MaxPrice { get; set; }
    public float MedianPrice { get; set; }
    public float StandardDeviation { get; set; }
    public DateTime EarliestDate { get; set; }
    public DateTime LatestDate { get; set; }
    public float PriceRange => MaxPrice - MinPrice;
    public float CoefficientOfVariation => AveragePrice > 0 ? StandardDeviation / AveragePrice : 0;
}

/// <summary>
/// Complete forecasting result for an item
/// </summary>
public class PriceForecastResult
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public List<PriceForecast> Forecasts { get; set; } = new();
    public List<PriceAnomaly> Anomalies { get; set; } = new();
    public PriceStatistics Statistics { get; set; } = new();

    /// <summary>
    /// Best date to buy within forecast window
    /// </summary>
    public DateTime? OptimalBuyingDate { get; set; }

    /// <summary>
    /// Expected price at optimal buying date
    /// </summary>
    public float? OptimalBuyingPrice { get; set; }

    /// <summary>
    /// Overall recommendation for this item
    /// </summary>
    public BuyingRecommendation OverallRecommendation { get; set; }

    /// <summary>
    /// Whether the forecast was generated successfully
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Error message if forecast failed
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Model training information
    /// </summary>
    public TrainingResult? TrainingInfo { get; set; }
}

/// <summary>
/// SSA (Singular Spectrum Analysis) model parameters
/// </summary>
public class SsaModelParameters
{
    /// <summary>
    /// Window size for SSA (default: 7 days)
    /// </summary>
    public int WindowSize { get; set; } = 7;

    /// <summary>
    /// Series length for training (default: use all data)
    /// </summary>
    public int? SeriesLength { get; set; }

    /// <summary>
    /// Number of training examples
    /// </summary>
    public int TrainSize { get; set; }

    /// <summary>
    /// Forecast horizon (number of periods to predict)
    /// </summary>
    public int Horizon { get; set; } = 30;

    /// <summary>
    /// Confidence level for intervals (default: 0.95)
    /// </summary>
    public float ConfidenceLevel { get; set; } = 0.95f;

    /// <summary>
    /// Whether to detect seasonality
    /// </summary>
    public bool DetectSeasonality { get; set; } = true;
}
