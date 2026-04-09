using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.TimeSeries;
using Microsoft.ML.Transforms.TimeSeries;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for forecasting future grocery prices using ML.NET Time Series analysis (SSA)
/// </summary>
public class PriceForecastingService
{
    private readonly MLContext _mlContext;
    private readonly Action<string>? _logInfo;
    private readonly Action<string, Exception>? _logError;
    private readonly Action<string>? _logWarning;
    private readonly string _modelPath;

    /// <summary>
    /// Minimum number of data points required for forecasting
    /// </summary>
    public const int MinimumDataPoints = 14;

    /// <summary>
    /// Recommended number of data points for accurate forecasting
    /// </summary>
    public const int RecommendedDataPoints = 90;

    /// <summary>
    /// Default forecast horizon (days)
    /// </summary>
    public const int DefaultForecastHorizon = 30;

    /// <summary>
    /// Creates a new instance of PriceForecastingService
    /// </summary>
    public PriceForecastingService(
        string modelPath,
        Action<string>? logInfo = null,
        Action<string, Exception>? logError = null,
        Action<string>? logWarning = null)
    {
        _mlContext = new MLContext(seed: 0);
        _modelPath = modelPath;
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;
    }

    /// <summary>
    /// Gets the path to the model storage directory
    /// </summary>
    public string ModelPath => _modelPath;

    /// <summary>
    /// Trains a price forecasting model for a specific item using SSA (Singular Spectrum Analysis)
    /// </summary>
    public Task<TrainingResult> TrainModelAsync(
        string itemId,
        string itemName,
        List<PriceHistoryData> priceHistory,
        SsaModelParameters? parameters = null,
        string? outputModelPath = null)
    {
        var startTime = DateTime.Now;
        _logInfo?.Invoke($"Starting price forecast model training for item: {itemName} ({itemId})");

        // Validate data
        if (priceHistory.Count < MinimumDataPoints)
        {
            return Task.FromResult(new TrainingResult
            {
                Success = false,
                Message = $"Insufficient data for forecasting. Need at least {MinimumDataPoints} days, found {priceHistory.Count}"
            });
        }

        var params1 = parameters ?? new SsaModelParameters();
        params1.TrainSize = priceHistory.Count;
        if (!params1.SeriesLength.HasValue)
        {
            params1.SeriesLength = priceHistory.Count;
        }

        try
        {
            // Prepare training data with feature engineering
            var trainingData = PrepareTrainingData(priceHistory);
            var dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Build SSA forecasting pipeline
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(PriceForecastOutput.ForecastedPrices),
                inputColumnName: nameof(PriceHistoryData.Price),
                windowSize: params1.WindowSize,
                seriesLength: params1.SeriesLength.Value,
                trainSize: params1.TrainSize,
                horizon: params1.Horizon,
                confidenceLevel: params1.ConfidenceLevel,
                confidenceLowerBoundColumn: nameof(PriceForecastOutput.LowerBounds),
                confidenceUpperBoundColumn: nameof(PriceForecastOutput.UpperBounds)
            );

            // Train model
            _logInfo?.Invoke($"Training SSA model with {priceHistory.Count} data points, horizon={params1.Horizon}");
            var model = pipeline.Fit(dataView);

            // Save model if path provided
            var modelFilePath = outputModelPath ?? GetDefaultModelPath(itemId);
            if (!string.IsNullOrEmpty(modelFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(modelFilePath)!);
                _mlContext.Model.Save(model, dataView.Schema, modelFilePath);
                _logInfo?.Invoke($"Price forecast model saved to {modelFilePath}");
            }

            var duration = DateTime.Now - startTime;
            _logInfo?.Invoke($"Price forecast model trained successfully in {duration.TotalSeconds:F1}s");

            return Task.FromResult(new TrainingResult
            {
                Success = true,
                Message = $"Model trained successfully for {itemName}",
                TrainingItemCount = priceHistory.Count,
                ModelPath = modelFilePath,
                Duration = duration
            });
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to train price forecast model for {itemName}", ex);
            return Task.FromResult(new TrainingResult
            {
                Success = false,
                Message = $"Training failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Generates price forecasts for a specific item
    /// </summary>
    public Task<PriceForecastResult> ForecastPricesAsync(
        string itemId,
        string itemName,
        List<PriceHistoryData> priceHistory,
        int daysAhead = DefaultForecastHorizon)
    {
        _logInfo?.Invoke($"Generating {daysAhead}-day price forecast for {itemName}");

        // Validate input data
        if (priceHistory.Count < MinimumDataPoints)
        {
            return Task.FromResult(new PriceForecastResult
            {
                ItemId = itemId,
                ItemName = itemName,
                Success = false,
                ErrorMessage = $"Insufficient data. Need at least {MinimumDataPoints} days, found {priceHistory.Count}"
            });
        }

        try
        {
            // Prepare data with calculated features
            var preparedData = PrepareTrainingData(priceHistory);
            var dataView = _mlContext.Data.LoadFromEnumerable(preparedData);

            // Calculate SSA parameters
            // seriesLength must be at least 2 * windowSize for SSA to work properly
            // Use conservative parameters to avoid ML.NET index errors
            var windowSize = 7;
            
            // seriesLength should be less than trainSize (preparedData.Count)
            // and at least 2 * windowSize
            var seriesLength = Math.Min(preparedData.Count - 1, Math.Max(windowSize * 3, 21));
            if (seriesLength < windowSize * 2)
            {
                seriesLength = windowSize * 2;
            }
            
            var trainSize = preparedData.Count;
            
            // Horizon cannot exceed seriesLength and should be reasonable
            var horizon = Math.Min(daysAhead, Math.Min(seriesLength / 2, 30));
            if (horizon < 1) horizon = 1;

            _logInfo?.Invoke($"Training SSA model: windowSize={windowSize}, seriesLength={seriesLength}, trainSize={trainSize}, horizon={horizon}, dataCount={preparedData.Count}");

            // Build and train SSA model
            var pipeline = _mlContext.Forecasting.ForecastBySsa(
                outputColumnName: nameof(PriceForecastOutput.ForecastedPrices),
                inputColumnName: nameof(PriceHistoryData.Price),
                windowSize: windowSize,
                seriesLength: seriesLength,
                trainSize: trainSize,
                horizon: horizon,
                confidenceLevel: 0.95f,
                confidenceLowerBoundColumn: nameof(PriceForecastOutput.LowerBounds),
                confidenceUpperBoundColumn: nameof(PriceForecastOutput.UpperBounds)
            );

            var model = pipeline.Fit(dataView);

            // Create time series forecast engine for SSA models
            // Note: CreatePredictionEngine doesn't work with SSA forecasting models
            // CreateTimeSeriesEngine is an extension method on ITransformer
            var forecastEngine = model.CreateTimeSeriesEngine<PriceHistoryData, PriceForecastOutput>(_mlContext);

            // Generate forecast - predict next 'horizon' periods
            var forecast = forecastEngine.Predict(horizon: horizon);

            // Build forecast results
            var forecasts = new List<PriceForecast>();
            var lastDate = priceHistory.Max(p => p.Date);
            var lastPrice = priceHistory.OrderBy(p => p.Date).Last().Price;

            // Ensure we have valid forecast arrays
            var forecastedPrices = forecast.ForecastedPrices ?? Array.Empty<float>();
            var lowerBounds = forecast.LowerBounds ?? Array.Empty<float>();
            var upperBounds = forecast.UpperBounds ?? Array.Empty<float>();

            // If ML.NET returned fewer forecasts than requested, pad with projected values
            if (forecastedPrices.Length < daysAhead)
            {
                var paddedForecasts = new float[daysAhead];
                var paddedLower = new float[daysAhead];
                var paddedUpper = new float[daysAhead];
                
                Array.Copy(forecastedPrices, paddedForecasts, forecastedPrices.Length);
                Array.Copy(lowerBounds, paddedLower, Math.Min(lowerBounds.Length, daysAhead));
                Array.Copy(upperBounds, paddedUpper, Math.Min(upperBounds.Length, daysAhead));
                
                // Pad remaining days using the last forecast value with slight adjustments
                var lastForecast = forecastedPrices.Length > 0 ? forecastedPrices.Last() : lastPrice;
                var lastLower = lowerBounds.Length > 0 ? lowerBounds.Last() : lastForecast * 0.9f;
                var lastUpper = upperBounds.Length > 0 ? upperBounds.Last() : lastForecast * 1.1f;
                
                for (int i = forecastedPrices.Length; i < daysAhead; i++)
                {
                    paddedForecasts[i] = lastForecast;
                    paddedLower[i] = lastLower;
                    paddedUpper[i] = lastUpper;
                }
                
                forecastedPrices = paddedForecasts;
                lowerBounds = paddedLower;
                upperBounds = paddedUpper;
            }

            for (int i = 0; i < daysAhead && i < forecastedPrices.Length; i++)
            {
                var predictedPrice = forecastedPrices[i];
                var lowerBound = lowerBounds.Length > i ? lowerBounds[i] : predictedPrice * 0.9f;
                var upperBound = upperBounds.Length > i ? upperBounds[i] : predictedPrice * 1.1f;

                var forecastDate = lastDate.AddDays(i + 1);
                var trend = DetermineTrend(lastPrice, predictedPrice, forecasts, i);
                var recommendation = GenerateRecommendation(predictedPrice, lowerBound, upperBound, priceHistory, trend);

                forecasts.Add(new PriceForecast
                {
                    ItemId = itemId,
                    ItemName = itemName,
                    ForecastDate = forecastDate,
                    PredictedPrice = predictedPrice,
                    LowerBound = lowerBound,
                    UpperBound = upperBound,
                    ConfidenceInterval = upperBound - lowerBound,
                    Trend = trend,
                    TrendStrength = CalculateTrendStrength(forecasts, i),
                    Recommendation = recommendation,
                    RecommendationReason = GetRecommendationReason(recommendation, predictedPrice, priceHistory, trend)
                });

                lastPrice = predictedPrice;
            }

            // Calculate statistics
            var stats = CalculateStatistics(itemId, itemName, priceHistory);

            // Find optimal buying date
            var optimalForecast = forecasts.OrderBy(f => f.PredictedPrice).FirstOrDefault();

            // Detect anomalies
            var anomalies = DetectAnomalies(itemId, itemName, priceHistory);

            // Determine overall recommendation
            var overallRecommendation = DetermineOverallRecommendation(forecasts, priceHistory);

            _logInfo?.Invoke($"Forecast generated successfully for {itemName}: {forecasts.Count} days, optimal date: {optimalForecast?.ForecastDate.ToShortDateString()}");

            return Task.FromResult(new PriceForecastResult
            {
                ItemId = itemId,
                ItemName = itemName,
                Forecasts = forecasts,
                Anomalies = anomalies,
                Statistics = stats,
                OptimalBuyingDate = optimalForecast?.ForecastDate,
                OptimalBuyingPrice = optimalForecast?.PredictedPrice,
                OverallRecommendation = overallRecommendation,
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to generate forecast for {itemName}", ex);
            return Task.FromResult(new PriceForecastResult
            {
                ItemId = itemId,
                ItemName = itemName,
                Success = false,
                ErrorMessage = $"Forecasting failed: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Forecasts prices for multiple items in batch
    /// </summary>
    public async Task<Dictionary<string, PriceForecastResult>> ForecastMultipleItemsAsync(
        Dictionary<string, List<PriceHistoryData>> itemsHistory,
        int daysAhead = DefaultForecastHorizon)
    {
        var results = new Dictionary<string, PriceForecastResult>();

        foreach (var kvp in itemsHistory)
        {
            var itemId = kvp.Key;
            var history = kvp.Value;

            if (history.Count > 0)
            {
                var firstRecord = history.First();
                var result = await ForecastPricesAsync(itemId, firstRecord.ItemName, history, daysAhead);
                results[itemId] = result;
            }
        }

        return results;
    }

    /// <summary>
    /// Gets the optimal buying date within the forecast window
    /// </summary>
    public async Task<(DateTime? Date, float? Price, BuyingRecommendation Recommendation)> GetOptimalBuyingDateAsync(
        string itemId,
        string itemName,
        List<PriceHistoryData> priceHistory,
        int daysAhead = DefaultForecastHorizon)
    {
        var forecast = await ForecastPricesAsync(itemId, itemName, priceHistory, daysAhead);

        if (!forecast.Success || !forecast.Forecasts.Any())
        {
            return (null, null, BuyingRecommendation.NormalTime);
        }

        var optimal = forecast.Forecasts.OrderBy(f => f.PredictedPrice).First();
        return (optimal.ForecastDate, optimal.PredictedPrice, optimal.Recommendation);
    }

    /// <summary>
    /// Calculates comprehensive price statistics for an item
    /// </summary>
    public PriceStatistics CalculateStatistics(string itemId, string itemName, List<PriceHistoryData> priceHistory)
    {
        if (!priceHistory.Any())
        {
            return new PriceStatistics { ItemId = itemId, ItemName = itemName };
        }

        var prices = priceHistory.Select(p => p.Price).ToList();
        var sortedPrices = prices.OrderBy(p => p).ToList();
        var count = prices.Count;

        var avg = prices.Average();
        var min = prices.Min();
        var max = prices.Max();
        var median = count % 2 == 0
            ? (sortedPrices[count / 2 - 1] + sortedPrices[count / 2]) / 2
            : sortedPrices[count / 2];

        // Calculate standard deviation
        var sumSquaredDiff = prices.Sum(p => (p - avg) * (p - avg));
        var stdDev = (float)Math.Sqrt(sumSquaredDiff / count);

        return new PriceStatistics
        {
            ItemId = itemId,
            ItemName = itemName,
            DataPoints = count,
            AveragePrice = avg,
            MinPrice = min,
            MaxPrice = max,
            MedianPrice = median,
            StandardDeviation = stdDev,
            EarliestDate = priceHistory.Min(p => p.Date),
            LatestDate = priceHistory.Max(p => p.Date)
        };
    }

    /// <summary>
    /// Detects price anomalies in historical data using spike detection
    /// </summary>
    public List<PriceAnomaly> DetectAnomalies(string itemId, string itemName, List<PriceHistoryData> priceHistory)
    {
        var anomalies = new List<PriceAnomaly>();

        if (priceHistory.Count < 14)
        {
            return anomalies;
        }

        try
        {
            var dataView = _mlContext.Data.LoadFromEnumerable(priceHistory);

            // Build spike detection pipeline using updated API with double confidence
            var pipeline = _mlContext.Transforms.DetectSpikeBySsa(
                outputColumnName: nameof(PriceAnomalyPrediction.IsAnomaly),
                inputColumnName: nameof(PriceHistoryData.Price),
                confidence: 95.0,
                pvalueHistoryLength: Math.Max(priceHistory.Count / 4, 7),
                trainingWindowSize: Math.Max(priceHistory.Count / 2, 14),
                seasonalityWindowSize: 7
            );

            var model = pipeline.Fit(dataView);
            var predictions = model.Transform(dataView);

            var detectedAnomalies = _mlContext.Data
                .CreateEnumerable<PriceAnomalyPrediction>(predictions, reuseRowObject: false)
                .ToList();

            var historyList = priceHistory.OrderBy(p => p.Date).ToList();
            var avgPrice = historyList.Average(p => p.Price);

            for (int i = 0; i < detectedAnomalies.Count && i < historyList.Count; i++)
            {
                var detection = detectedAnomalies[i];
                var history = historyList[i];

                if (detection.IsAnomaly)
                {
                    var anomaly = new PriceAnomaly
                    {
                        ItemId = itemId,
                        ItemName = itemName,
                        Date = history.Date,
                        ActualPrice = history.Price,
                        ExpectedPrice = avgPrice,
                        Deviation = Math.Abs(history.Price - avgPrice),
                        IsAnomaly = true,
                        AnomalyScore = detection.Score,
                        PValue = detection.PValue
                    };

                    // Classify anomaly type
                    if (history.IsOnSale && history.Price >= avgPrice * 0.95f)
                    {
                        anomaly.Type = AnomalyType.IllusoryDiscount;
                        anomaly.Description = $"Illusory discount: Sale price ${history.Price:F2} is near average ${avgPrice:F2}";
                    }
                    else if (history.Price > avgPrice * 1.2f)
                    {
                        anomaly.Type = AnomalyType.PriceSpike;
                        anomaly.Description = $"Price spike: ${history.Price:F2} vs average ${avgPrice:F2}";
                    }
                    else if (history.Price < avgPrice * 0.8f)
                    {
                        anomaly.Type = AnomalyType.PriceDrop;
                        anomaly.Description = $"Price drop: ${history.Price:F2} vs average ${avgPrice:F2}";
                    }
                    else
                    {
                        anomaly.Type = AnomalyType.Seasonal;
                        anomaly.Description = $"Seasonal variation: ${history.Price:F2}";
                    }

                    anomalies.Add(anomaly);
                }
            }

            _logInfo?.Invoke($"Detected {anomalies.Count} anomalies for {itemName}");
            return anomalies;
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to detect anomalies for {itemName}", ex);
            return anomalies;
        }
    }

    /// <summary>
    /// Identifies illusory discounts (fake sales) in current prices
    /// </summary>
    public List<PriceAnomaly> DetectIllusoryDiscounts(
        string itemId,
        string itemName,
        List<PriceHistoryData> priceHistory,
        float currentPrice,
        bool isCurrentlyOnSale)
    {
        var illusoryDiscounts = new List<PriceAnomaly>();

        if (!isCurrentlyOnSale || priceHistory.Count < 10)
        {
            return illusoryDiscounts;
        }

        var nonSalePrices = priceHistory.Where(p => !p.IsOnSale).Select(p => p.Price).ToList();
        if (!nonSalePrices.Any())
        {
            nonSalePrices = priceHistory.Select(p => p.Price).ToList();
        }

        var avgNonSalePrice = nonSalePrices.Average();

        // Flag as illusory if "sale" price is >= 95% of average non-sale price
        if (currentPrice >= avgNonSalePrice * 0.95f)
        {
            illusoryDiscounts.Add(new PriceAnomaly
            {
                ItemId = itemId,
                ItemName = itemName,
                Date = DateTime.Now,
                ActualPrice = currentPrice,
                ExpectedPrice = avgNonSalePrice,
                Deviation = currentPrice - avgNonSalePrice,
                IsAnomaly = true,
                Type = AnomalyType.IllusoryDiscount,
                Description = $"Illusory discount: Current 'sale' price ${currentPrice:F2} is {(currentPrice / avgNonSalePrice * 100):F1}% of average price ${avgNonSalePrice:F2}"
            });
        }

        return illusoryDiscounts;
    }

    /// <summary>
    /// Converts PriceRecord entities to PriceHistoryData
    /// </summary>
    public static List<PriceHistoryData> ConvertPriceRecords(
        string itemId,
        string itemName,
        IEnumerable<PriceRecord> records,
        string? storeName = null,
        string? category = null)
    {
        var history = records
            .Where(r => r.ItemId == itemId)
            .OrderBy(r => r.DateRecorded)
            .Select(r => new PriceHistoryData
            {
                ItemId = itemId,
                ItemName = itemName,
                Date = r.DateRecorded,
                Price = (float)r.Price,
                IsOnSale = r.IsOnSale,
                Store = storeName ?? string.Empty,
                Category = category ?? string.Empty,
                DayOfWeek = (float)r.DateRecorded.DayOfWeek,
                WeekOfYear = GetWeekOfYear(r.DateRecorded),
                Month = r.DateRecorded.Month
            })
            .ToList();

        // Calculate derived features
        return CalculateDerivedFeatures(history);
    }

    #region Private Helper Methods

    private List<PriceHistoryData> PrepareTrainingData(List<PriceHistoryData> history)
    {
        // Ensure data is sorted by date
        var sorted = history.OrderBy(h => h.Date).ToList();

        // Calculate derived features
        return CalculateDerivedFeatures(sorted);
    }

    private static List<PriceHistoryData> CalculateDerivedFeatures(List<PriceHistoryData> history)
    {
        for (int i = 0; i < history.Count; i++)
        {
            // 7-day moving average
            if (i >= 6)
            {
                history[i].MovingAverage7Days = history
                    .Skip(i - 6)
                    .Take(7)
                    .Average(p => p.Price);
            }

            // 30-day moving average
            if (i >= 29)
            {
                history[i].MovingAverage30Days = history
                    .Skip(i - 29)
                    .Take(30)
                    .Average(p => p.Price);
            }

            // Price change from previous day
            if (i > 0)
            {
                history[i].PriceChange = history[i].Price - history[i - 1].Price;
            }

            // 7-day volatility (standard deviation)
            if (i >= 6)
            {
                var window = history.Skip(i - 6).Take(7).Select(p => p.Price).ToList();
                var avg = window.Average();
                var variance = window.Average(p => (p - avg) * (p - avg));
                history[i].Volatility7Days = (float)Math.Sqrt(variance);
            }
        }

        return history;
    }

    private PriceTrend DetermineTrend(float lastPrice, float currentPrice, List<PriceForecast> recentForecasts, int currentIndex)
    {
        if (recentForecasts.Count < 2 || currentIndex < 2)
        {
            var change = Math.Abs(currentPrice - lastPrice) / lastPrice;
            if (change < 0.02f) return PriceTrend.Stable;
            return currentPrice > lastPrice ? PriceTrend.Rising : PriceTrend.Falling;
        }

        // Look at last 3 predictions including current
        var prices = recentForecasts
            .Skip(Math.Max(0, currentIndex - 2))
            .Take(3)
            .Select(f => f.PredictedPrice)
            .ToList();
        prices.Add(currentPrice);

        var isRising = prices[1] > prices[0] && prices[2] > prices[1] && prices[3] > prices[2];
        var isFalling = prices[1] < prices[0] && prices[2] < prices[1] && prices[3] < prices[2];

        if (isRising) return PriceTrend.Rising;
        if (isFalling) return PriceTrend.Falling;
        return PriceTrend.Stable;
    }

    private float CalculateTrendStrength(List<PriceForecast> recentForecasts, int currentIndex)
    {
        if (recentForecasts.Count < 2 || currentIndex < 1)
        {
            return 0.5f;
        }

        var prices = recentForecasts
            .Skip(Math.Max(0, currentIndex - 2))
            .Take(Math.Min(4, currentIndex + 1))
            .Select(f => f.PredictedPrice)
            .ToList();

        if (prices.Count < 2) return 0.5f;

        var maxPrice = prices.Max();
        var minPrice = prices.Min();
        var range = maxPrice - minPrice;

        if (range == 0) return 0f;

        var avgPrice = prices.Average();
        var normalizedRange = range / avgPrice;

        // Cap at 1.0 (100% strength)
        return Math.Min(normalizedRange * 5, 1.0f);
    }

    private BuyingRecommendation GenerateRecommendation(
        float predictedPrice,
        float lowerBound,
        float upperBound,
        List<PriceHistoryData> history,
        PriceTrend trend)
    {
        var avgPrice = history.Average(h => h.Price);
        var minPrice = history.Min(h => h.Price);
        var maxPrice = history.Max(h => h.Price);

        // Buy now if price is near historical minimum (within 10%)
        if (predictedPrice <= minPrice * 1.1f)
        {
            return BuyingRecommendation.BuyNow;
        }

        // Wait if price is falling
        if (trend == PriceTrend.Falling)
        {
            return BuyingRecommendation.Wait;
        }

        // Avoid if price is unusually high (within 10% of max)
        if (predictedPrice >= maxPrice * 0.9f)
        {
            return BuyingRecommendation.AvoidHighPrice;
        }

        return BuyingRecommendation.NormalTime;
    }

    private string GetRecommendationReason(BuyingRecommendation recommendation, float predictedPrice, List<PriceHistoryData> history, PriceTrend trend)
    {
        var avgPrice = history.Average(h => h.Price);
        var minPrice = history.Min(h => h.Price);
        var maxPrice = history.Max(h => h.Price);

        return recommendation switch
        {
            BuyingRecommendation.BuyNow => $"Price ${predictedPrice:F2} is at or near historical low (${minPrice:F2})",
            BuyingRecommendation.Wait => $"Price trend is {trend.ToString().ToLower()}, expected to drop further",
            BuyingRecommendation.AvoidHighPrice => $"Price ${predictedPrice:F2} is unusually high (max was ${maxPrice:F2})",
            _ => $"Price ${predictedPrice:F2} is within normal range (avg ${avgPrice:F2})"
        };
    }

    private BuyingRecommendation DetermineOverallRecommendation(List<PriceForecast> forecasts, List<PriceHistoryData> history)
    {
        if (!forecasts.Any())
        {
            return BuyingRecommendation.NormalTime;
        }

        var avgForecast = forecasts.Average(f => f.PredictedPrice);
        var avgHistory = history.Average(h => h.Price);
        var minHistory = history.Min(h => h.Price);

        // If average forecast is near historical low
        if (avgForecast <= minHistory * 1.15f)
        {
            return BuyingRecommendation.BuyNow;
        }

        // If prices are trending down overall
        var firstWeek = forecasts.Take(7).Average(f => f.PredictedPrice);
        var lastWeek = forecasts.Skip(Math.Max(0, forecasts.Count - 7)).Average(f => f.PredictedPrice);

        if (lastWeek < firstWeek * 0.95f)
        {
            return BuyingRecommendation.Wait;
        }

        // If average forecast is higher than historical average
        if (avgForecast > avgHistory * 1.1f)
        {
            return BuyingRecommendation.AvoidHighPrice;
        }

        return BuyingRecommendation.NormalTime;
    }

    private string GetDefaultModelPath(string itemId)
    {
        var modelDir = Path.GetDirectoryName(_modelPath) ?? Path.Combine(Environment.CurrentDirectory, "MLModels");
        return Path.Combine(modelDir, $"price_forecast_{itemId}.zip");
    }

    private static float GetWeekOfYear(DateTime date)
    {
        var culture = System.Globalization.CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(
            date,
            System.Globalization.CalendarWeekRule.FirstDay,
            DayOfWeek.Monday);
    }

    /// <summary>
    /// Internal class for ML.NET anomaly detection output
    /// </summary>
    private class PriceAnomalyPrediction
    {
        [ColumnName("PredictedLabel")]
        public bool IsAnomaly { get; set; }

        [ColumnName("Score")]
        public float Score { get; set; }

        [LoadColumn(2)]
        public double PValue { get; set; }
    }

    #endregion
}
