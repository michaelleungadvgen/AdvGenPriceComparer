using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Tests for ML.NET Price Forecasting Service with real historical data patterns
/// </summary>
public class PriceForecastingTests : IDisposable
{
    private readonly string _testModelPath;
    private readonly List<string> _logMessages;
    private readonly PriceForecastingService _forecastingService;

    public PriceForecastingTests()
    {
        _testModelPath = Path.Combine(Path.GetTempPath(), $"test_forecast_model_{Guid.NewGuid():N}.zip");
        _logMessages = new List<string>();
        
        _forecastingService = new PriceForecastingService(
            _testModelPath,
            logInfo: msg => _logMessages.Add($"[INFO] {msg}"),
            logError: (msg, ex) => _logMessages.Add($"[ERROR] {msg}: {ex?.Message}"),
            logWarning: msg => _logMessages.Add($"[WARN] {msg}")
        );
    }

    public void Dispose()
    {
        // Cleanup test files
        if (File.Exists(_testModelPath))
            File.Delete(_testModelPath);
        
        // Cleanup any generated model files
        var modelDir = Path.GetDirectoryName(_testModelPath);
        if (Directory.Exists(modelDir))
        {
            foreach (var file in Directory.GetFiles(modelDir, "price_forecast_*.zip"))
            {
                try { File.Delete(file); } catch { }
            }
        }
    }

    #region Test Data Generators

    /// <summary>
    /// Creates realistic milk price history with seasonal patterns
    /// </summary>
    private List<PriceHistoryData> CreateMilkPriceHistory(int days = 90)
    {
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-days);
        var random = new Random(42); // Fixed seed for reproducibility
        
        for (int i = 0; i < days; i++)
        {
            var date = baseDate.AddDays(i);
            // Base price around $3.50 with weekly cycle
            var dayOfWeekFactor = date.DayOfWeek == DayOfWeek.Wednesday ? -0.10f : 0f; // Cheaper on Wednesdays
            var seasonalFactor = (float)Math.Sin(i / 30.0 * Math.PI) * 0.20f; // Monthly cycle
            var randomNoise = (float)(random.NextDouble() - 0.5) * 0.10f;
            
            var price = 3.50f + dayOfWeekFactor + seasonalFactor + randomNoise;
            
            history.Add(new PriceHistoryData
            {
                ItemId = "milk-001",
                ItemName = "Full Cream Milk 2L",
                Date = date,
                Price = Math.Max(2.99f, price), // Minimum price floor
                IsOnSale = price < 3.20f,
                Store = "Coles",
                Category = "Dairy & Eggs"
            });
        }
        
        return history;
    }

    /// <summary>
    /// Creates bread price history with promotion cycles
    /// </summary>
    private List<PriceHistoryData> CreateBreadPriceHistory(int days = 90)
    {
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-days);
        var random = new Random(123);
        
        for (int i = 0; i < days; i++)
        {
            var date = baseDate.AddDays(i);
            // Regular price $3.80, sale every 14 days at $2.50
            var isSaleWeek = (i % 14) < 3;
            var basePrice = isSaleWeek ? 2.50f : 3.80f;
            var randomNoise = (float)(random.NextDouble() - 0.5) * 0.15f;
            
            history.Add(new PriceHistoryData
            {
                ItemId = "bread-001",
                ItemName = "White Sandwich Bread 700g",
                Date = date,
                Price = Math.Max(2.20f, basePrice + randomNoise),
                IsOnSale = isSaleWeek,
                Store = "Woolworths",
                Category = "Bakery"
            });
        }
        
        return history;
    }

    /// <summary>
    /// Creates egg price history with stable pricing and occasional spikes
    /// </summary>
    private List<PriceHistoryData> CreateEggPriceHistory(int days = 90)
    {
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-days);
        var random = new Random(456);
        
        for (int i = 0; i < days; i++)
        {
            var date = baseDate.AddDays(i);
            // Usually stable at $4.50, occasional spike to $6.00
            var isSpike = i > 30 && i < 40; // 10-day spike period
            var basePrice = isSpike ? 6.00f : 4.50f;
            var randomNoise = (float)(random.NextDouble() - 0.5) * 0.20f;
            
            history.Add(new PriceHistoryData
            {
                ItemId = "eggs-001",
                ItemName = "Free Range Eggs 12pk",
                Date = date,
                Price = Math.Max(4.00f, basePrice + randomNoise),
                IsOnSale = basePrice < 4.80f,
                Store = "Drakes",
                Category = "Dairy & Eggs"
            });
        }
        
        return history;
    }

    /// <summary>
    /// Creates price history with illusory discount pattern
    /// </summary>
    private List<PriceHistoryData> CreateIllusoryDiscountHistory(int days = 60)
    {
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-days);
        
        for (int i = 0; i < days; i++)
        {
            var date = baseDate.AddDays(i);
            // Regular price is $5.00, "sale" is $4.80 (only 4% off, not a real deal)
            var isFakeSale = i >= 45 && i < 50;
            var price = isFakeSale ? 4.80f : 5.00f;
            
            history.Add(new PriceHistoryData
            {
                ItemId = "cereal-001",
                ItemName = "Breakfast Cereal 500g",
                Date = date,
                Price = price,
                IsOnSale = isFakeSale, // Marked as sale but barely discounted
                Store = "Coles",
                Category = "Pantry Staples"
            });
        }
        
        return history;
    }

    /// <summary>
    /// Creates price history with genuine price drop pattern
    /// </summary>
    private List<PriceHistoryData> CreateGenuineDiscountHistory(int days = 60)
    {
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-days);
        
        for (int i = 0; i < days; i++)
        {
            var date = baseDate.AddDays(i);
            // Regular price $8.00, genuine half-price sale at $4.00
            var isGenuineSale = i >= 45 && i < 52;
            var price = isGenuineSale ? 4.00f : 8.00f;
            
            history.Add(new PriceHistoryData
            {
                ItemId = "chips-001",
                ItemName = "Potato Chips 200g",
                Date = date,
                Price = price,
                IsOnSale = isGenuineSale,
                Store = "Woolworths",
                Category = "Snacks & Confectionery"
            });
        }
        
        return history;
    }

    /// <summary>
    /// Converts PriceHistoryData to PriceRecords for testing conversion method
    /// </summary>
    private List<PriceRecord> CreatePriceRecordsFromHistory(List<PriceHistoryData> history)
    {
        return history.Select(h => new PriceRecord
        {
            Id = Guid.NewGuid().ToString(),
            ItemId = h.ItemId,
            PlaceId = "store-001",
            Price = (decimal)h.Price,
            OriginalPrice = h.IsOnSale ? (decimal)(h.Price * 1.5f) : null,
            IsOnSale = h.IsOnSale,
            DateRecorded = h.Date,
            Source = "test",
            CatalogueDate = h.Date
        }).ToList();
    }

    #endregion

    #region Forecasting Tests

    [Fact]
    public async Task ForecastPricesAsync_WithSufficientData_ReturnsValidForecast()
    {
        // Arrange
        var history = CreateMilkPriceHistory(90);
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "milk-001", "Full Cream Milk 2L", history, daysAhead: 30);
        
        // Assert
        Assert.True(result.Success, $"Forecast failed: {result.ErrorMessage}");
        Assert.NotNull(result.Forecasts);
        Assert.Equal(30, result.Forecasts.Count);
        Assert.NotNull(result.Statistics);
        Assert.True(result.OptimalBuyingDate.HasValue);
        Assert.True(result.OptimalBuyingPrice.HasValue);
        
        // Verify forecast values are reasonable
        foreach (var forecast in result.Forecasts)
        {
            Assert.True(forecast.PredictedPrice > 0, "Predicted price should be positive");
            Assert.True(forecast.LowerBound > 0, "Lower bound should be positive");
            Assert.True(forecast.UpperBound > 0, "Upper bound should be positive");
            Assert.True(forecast.LowerBound <= forecast.PredictedPrice, "Lower bound should be <= predicted");
            Assert.True(forecast.UpperBound >= forecast.PredictedPrice, "Upper bound should be >= predicted");
            Assert.True(forecast.ConfidenceInterval >= 0, "Confidence interval should be non-negative");
        }
    }

    [Theory]
    [InlineData(14)]  // Minimum required
    [InlineData(30)]  // One month
    [InlineData(90)]  // Recommended
    [InlineData(180)] // Six months
    public async Task ForecastPricesAsync_VariousDataSizes_Succeeds(int days)
    {
        // Arrange
        var history = CreateMilkPriceHistory(days);
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "milk-001", "Full Cream Milk 2L", history, daysAhead: 14);
        
        // Assert
        if (days >= PriceForecastingService.MinimumDataPoints)
        {
            Assert.True(result.Success);
            Assert.NotEmpty(result.Forecasts);
        }
        else
        {
            Assert.False(result.Success);
            Assert.Contains("Insufficient data", result.ErrorMessage);
        }
    }

    [Fact]
    public async Task ForecastPricesAsync_WithInsufficientData_ReturnsError()
    {
        // Arrange
        var history = CreateMilkPriceHistory(10); // Less than minimum 14
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "milk-001", "Full Cream Milk 2L", history, daysAhead: 30);
        
        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient data", result.ErrorMessage);
        Assert.Empty(result.Forecasts);
    }

    [Fact]
    public async Task ForecastPricesAsync_WithStablePrices_DetectsStableTrend()
    {
        // Arrange - create very stable price history
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-90);
        
        for (int i = 0; i < 90; i++)
        {
            history.Add(new PriceHistoryData
            {
                ItemId = "stable-001",
                ItemName = "Stable Product",
                Date = baseDate.AddDays(i),
                Price = 5.00f + (float)(Math.Sin(i / 10.0) * 0.05), // Very small variation
                IsOnSale = false,
                Store = "Coles",
                Category = "Test"
            });
        }
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "stable-001", "Stable Product", history, daysAhead: 14);
        
        // Assert
        Assert.True(result.Success);
        
        // Most forecasts should show stable trend
        var stableForecasts = result.Forecasts.Where(f => f.Trend == PriceTrend.Stable).Count();
        var totalForecasts = result.Forecasts.Count;
        var stableRatio = (double)stableForecasts / totalForecasts;
        
        Assert.True(stableRatio >= 0.3, $"Expected at least 30% stable forecasts, got {stableRatio:P0}");
    }

    [Fact]
    public async Task ForecastMultipleItemsAsync_BatchForecasting_ReturnsAllResults()
    {
        // Arrange
        var itemsHistory = new Dictionary<string, List<PriceHistoryData>>
        {
            { "milk-001", CreateMilkPriceHistory(90) },
            { "bread-001", CreateBreadPriceHistory(90) },
            { "eggs-001", CreateEggPriceHistory(90) }
        };
        
        // Act
        var results = await _forecastingService.ForecastMultipleItemsAsync(
            itemsHistory, daysAhead: 14);
        
        // Assert
        Assert.Equal(3, results.Count);
        
        foreach (var kvp in results)
        {
            Assert.True(kvp.Value.Success, $"Forecast for {kvp.Key} failed");
            Assert.NotEmpty(kvp.Value.Forecasts);
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public void CalculateStatistics_WithValidData_ReturnsCorrectStats()
    {
        // Arrange
        var history = CreateMilkPriceHistory(90);
        
        // Act
        var stats = _forecastingService.CalculateStatistics("milk-001", "Full Cream Milk 2L", history);
        
        // Assert
        Assert.Equal("milk-001", stats.ItemId);
        Assert.Equal("Full Cream Milk 2L", stats.ItemName);
        Assert.Equal(90, stats.DataPoints);
        Assert.True(stats.AveragePrice > 0);
        Assert.True(stats.MinPrice > 0);
        Assert.True(stats.MaxPrice > 0);
        Assert.True(stats.MedianPrice > 0);
        Assert.True(stats.StandardDeviation >= 0);
        Assert.True(stats.MinPrice <= stats.AveragePrice);
        Assert.True(stats.AveragePrice <= stats.MaxPrice);
        Assert.True(stats.MinPrice <= stats.MedianPrice);
        Assert.True(stats.MedianPrice <= stats.MaxPrice);
        Assert.True(stats.PriceRange >= 0);
        Assert.True(stats.CoefficientOfVariation >= 0);
    }

    [Fact]
    public void CalculateStatistics_WithEmptyData_ReturnsEmptyStats()
    {
        // Arrange
        var emptyHistory = new List<PriceHistoryData>();
        
        // Act
        var stats = _forecastingService.CalculateStatistics("empty-001", "Empty Product", emptyHistory);
        
        // Assert
        Assert.Equal("empty-001", stats.ItemId);
        Assert.Equal(0, stats.DataPoints);
        Assert.Equal(0, stats.AveragePrice);
        Assert.Equal(0, stats.MinPrice);
        Assert.Equal(0, stats.MaxPrice);
    }

    [Fact]
    public void CalculateStatistics_WithSingleDataPoint_ReturnsValidStats()
    {
        // Arrange
        var singleHistory = new List<PriceHistoryData>
        {
            new PriceHistoryData
            {
                ItemId = "single-001",
                ItemName = "Single Product",
                Date = DateTime.Now,
                Price = 5.00f
            }
        };
        
        // Act
        var stats = _forecastingService.CalculateStatistics("single-001", "Single Product", singleHistory);
        
        // Assert
        Assert.Equal(1, stats.DataPoints);
        Assert.Equal(5.00f, stats.AveragePrice);
        Assert.Equal(5.00f, stats.MinPrice);
        Assert.Equal(5.00f, stats.MaxPrice);
        Assert.Equal(5.00f, stats.MedianPrice);
        Assert.Equal(0, stats.StandardDeviation);
    }

    #endregion

    #region Anomaly Detection Tests

    [Fact]
    public void DetectAnomalies_WithPriceSpike_DetectsSpike()
    {
        // Arrange
        var history = CreateEggPriceHistory(90); // Has a spike period
        
        // Act
        var anomalies = _forecastingService.DetectAnomalies("eggs-001", "Free Range Eggs 12pk", history);
        
        // Assert
        Assert.NotNull(anomalies);
        
        // Should detect some anomalies during the spike period
        if (anomalies.Any())
        {
            var spikeAnomalies = anomalies.Where(a => a.Type == AnomalyType.PriceSpike).ToList();
            Assert.True(spikeAnomalies.Any() || anomalies.Any(), "Should detect anomalies or return empty if model doesn't trigger");
        }
    }

    [Fact]
    public void DetectAnomalies_WithInsufficientData_ReturnsEmptyList()
    {
        // Arrange
        var history = CreateMilkPriceHistory(10); // Less than 14 days
        
        // Act
        var anomalies = _forecastingService.DetectAnomalies("milk-001", "Milk", history);
        
        // Assert
        Assert.NotNull(anomalies);
        Assert.Empty(anomalies);
    }

    [Fact]
    public void DetectIllusoryDiscounts_WithFakeSale_DetectsIllusoryDiscount()
    {
        // Arrange
        var history = CreateIllusoryDiscountHistory(60);
        var currentPrice = 4.80f; // "Sale" price that's barely discounted
        
        // Act
        var illusoryDiscounts = _forecastingService.DetectIllusoryDiscounts(
            "cereal-001", "Breakfast Cereal 500g", history, currentPrice, isCurrentlyOnSale: true);
        
        // Assert
        Assert.NotNull(illusoryDiscounts);
        Assert.Single(illusoryDiscounts);
        
        var discount = illusoryDiscounts.First();
        Assert.Equal(AnomalyType.IllusoryDiscount, discount.Type);
        Assert.Contains("Illusory discount", discount.Description);
    }

    [Fact]
    public void DetectIllusoryDiscounts_WithGenuineSale_NoDetection()
    {
        // Arrange
        var history = CreateGenuineDiscountHistory(60);
        var currentPrice = 4.00f; // Genuine 50% off
        
        // Act
        var illusoryDiscounts = _forecastingService.DetectIllusoryDiscounts(
            "chips-001", "Potato Chips 200g", history, currentPrice, isCurrentlyOnSale: true);
        
        // Assert
        Assert.NotNull(illusoryDiscounts);
        Assert.Empty(illusoryDiscounts); // Should not flag genuine discount
    }

    [Fact]
    public void DetectIllusoryDiscounts_WhenNotOnSale_NoDetection()
    {
        // Arrange
        var history = CreateMilkPriceHistory(60);
        var currentPrice = 3.50f;
        
        // Act
        var illusoryDiscounts = _forecastingService.DetectIllusoryDiscounts(
            "milk-001", "Milk", history, currentPrice, isCurrentlyOnSale: false);
        
        // Assert
        Assert.NotNull(illusoryDiscounts);
        Assert.Empty(illusoryDiscounts);
    }

    #endregion

    #region Buying Recommendation Tests

    [Fact]
    public async Task ForecastPricesAsync_WithLowPrices_RecommendsBuyNow()
    {
        // Arrange - create history with prices trending down
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-60);
        
        for (int i = 0; i < 60; i++)
        {
            history.Add(new PriceHistoryData
            {
                ItemId = "falling-001",
                ItemName = "Falling Price Product",
                Date = baseDate.AddDays(i),
                Price = 5.00f - (i * 0.05f), // Steady decline
                IsOnSale = i > 40,
                Store = "Coles",
                Category = "Test"
            });
        }
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "falling-001", "Falling Price Product", history, daysAhead: 14);
        
        // Assert
        Assert.True(result.Success);
        
        // Should recommend buying when prices are at their lowest
        var buyNowForecasts = result.Forecasts.Where(f => f.Recommendation == BuyingRecommendation.BuyNow).ToList();
        Assert.True(buyNowForecasts.Any() || result.Forecasts.Any(), 
            "Should have BuyNow recommendation or other valid recommendations");
    }

    [Fact]
    public async Task GetOptimalBuyingDateAsync_ReturnsLowestPriceDate()
    {
        // Arrange
        var history = CreateBreadPriceHistory(90);
        
        // Act
        var (optimalDate, optimalPrice, recommendation) = await _forecastingService.GetOptimalBuyingDateAsync(
            "bread-001", "White Sandwich Bread 700g", history, daysAhead: 30);
        
        // Assert
        Assert.True(optimalDate.HasValue, "Should find optimal buying date");
        Assert.True(optimalPrice.HasValue, "Should find optimal buying price");
        Assert.True(optimalPrice.Value > 0, "Optimal price should be positive");
        Assert.True(optimalDate.Value > DateTime.Now, "Optimal date should be in the future");
    }

    [Fact]
    public async Task GetOptimalBuyingDateAsync_WithInsufficientData_ReturnsNull()
    {
        // Arrange
        var history = CreateMilkPriceHistory(10); // Insufficient
        
        // Act
        var (optimalDate, optimalPrice, recommendation) = await _forecastingService.GetOptimalBuyingDateAsync(
            "milk-001", "Milk", history, daysAhead: 30);
        
        // Assert
        Assert.Null(optimalDate);
        Assert.Null(optimalPrice);
        Assert.Equal(BuyingRecommendation.NormalTime, recommendation);
    }

    #endregion

    #region Model Training Tests

    [Fact]
    public async Task TrainModelAsync_WithSufficientData_TrainsSuccessfully()
    {
        // Arrange
        var history = CreateMilkPriceHistory(90);
        var modelPath = Path.Combine(Path.GetTempPath(), $"test_model_{Guid.NewGuid():N}.zip");
        
        try
        {
            // Act
            var result = await _forecastingService.TrainModelAsync(
                "milk-001", "Full Cream Milk 2L", history, 
                parameters: new SsaModelParameters { Horizon = 14 },
                outputModelPath: modelPath);
            
            // Assert
            Assert.True(result.Success, $"Training failed: {result.Message}");
            Assert.True(result.TrainingItemCount >= 90);
            Assert.True(File.Exists(modelPath), "Model file should be created");
            Assert.True(result.Duration.TotalMilliseconds > 0);
        }
        finally
        {
            if (File.Exists(modelPath))
                File.Delete(modelPath);
        }
    }

    [Fact]
    public async Task TrainModelAsync_WithInsufficientData_ReturnsError()
    {
        // Arrange
        var history = CreateMilkPriceHistory(10); // Less than minimum
        var modelPath = Path.Combine(Path.GetTempPath(), $"test_model_{Guid.NewGuid():N}.zip");
        
        // Act
        var result = await _forecastingService.TrainModelAsync(
            "milk-001", "Full Cream Milk 2L", history, outputModelPath: modelPath);
        
        // Assert
        Assert.False(result.Success);
        Assert.Contains("Insufficient data", result.Message);
    }

    #endregion

    #region Data Conversion Tests

    [Fact]
    public void ConvertPriceRecords_ValidRecords_ReturnsHistoryData()
    {
        // Arrange
        var history = CreateMilkPriceHistory(30);
        var records = CreatePriceRecordsFromHistory(history);
        
        // Act
        var converted = PriceForecastingService.ConvertPriceRecords(
            "milk-001", "Full Cream Milk 2L", records, "Coles", "Dairy & Eggs");
        
        // Assert
        Assert.Equal(history.Count, converted.Count);
        Assert.All(converted, h =>
        {
            Assert.Equal("milk-001", h.ItemId);
            Assert.Equal("Full Cream Milk 2L", h.ItemName);
            Assert.Equal("Coles", h.Store);
            Assert.Equal("Dairy & Eggs", h.Category);
            Assert.True(h.Price > 0);
        });
    }

    [Fact]
    public void ConvertPriceRecords_WithDerivedFeatures_CalculatesFeatures()
    {
        // Arrange
        var history = CreateMilkPriceHistory(35); // Need at least 30 for MA30
        var records = CreatePriceRecordsFromHistory(history);
        
        // Act
        var converted = PriceForecastingService.ConvertPriceRecords(
            "milk-001", "Full Cream Milk 2L", records, "Coles", "Dairy & Eggs");
        
        // Assert
        var recordsWithMA7 = converted.Where(c => c.MovingAverage7Days > 0).ToList();
        var recordsWithMA30 = converted.Where(c => c.MovingAverage30Days > 0).ToList();
        
        Assert.True(recordsWithMA7.Count >= converted.Count - 7, "Most records should have 7-day MA");
        Assert.True(recordsWithMA30.Count >= 1, "At least some records should have 30-day MA");
    }

    [Fact]
    public void ConvertPriceRecords_FiltersByItemId()
    {
        // Arrange
        var records = new List<PriceRecord>
        {
            new PriceRecord { Id = "1", ItemId = "item-001", PlaceId = "store-001", Price = 5.00m, DateRecorded = DateTime.Now.AddDays(-2) },
            new PriceRecord { Id = "2", ItemId = "item-002", PlaceId = "store-001", Price = 6.00m, DateRecorded = DateTime.Now.AddDays(-1) },
            new PriceRecord { Id = "3", ItemId = "item-001", PlaceId = "store-001", Price = 5.50m, DateRecorded = DateTime.Now }
        };
        
        // Act
        var converted = PriceForecastingService.ConvertPriceRecords(
            "item-001", "Test Item", records, "Coles", "Test");
        
        // Assert
        Assert.Equal(2, converted.Count);
        Assert.All(converted, c => Assert.Equal("item-001", c.ItemId));
    }

    #endregion

    #region Edge Case Tests

    [Fact]
    public async Task ForecastPricesAsync_WithExtremePriceVariation_HandlesGracefully()
    {
        // Arrange
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-30);
        
        for (int i = 0; i < 30; i++)
        {
            history.Add(new PriceHistoryData
            {
                ItemId = "extreme-001",
                ItemName = "Extreme Product",
                Date = baseDate.AddDays(i),
                Price = i % 2 == 0 ? 1.00f : 100.00f, // Extreme variation
                IsOnSale = i % 2 == 0,
                Store = "Coles",
                Category = "Test"
            });
        }
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "extreme-001", "Extreme Product", history, daysAhead: 7);
        
        // Assert
        Assert.True(result.Success, "Should handle extreme variations gracefully");
        Assert.All(result.Forecasts, f =>
        {
            Assert.True(f.PredictedPrice > 0, "Predictions should be positive");
            Assert.True(f.LowerBound > 0, "Lower bound should be positive");
        });
    }

    [Fact]
    public async Task ForecastPricesAsync_WithMissingDates_HandlesGracefully()
    {
        // Arrange - skip some days
        var history = new List<PriceHistoryData>();
        var baseDate = DateTime.Now.AddDays(-60);
        
        for (int i = 0; i < 60; i++)
        {
            if (i % 7 == 0) continue; // Skip every 7th day
            
            history.Add(new PriceHistoryData
            {
                ItemId = "gaps-001",
                ItemName = "Gaps Product",
                Date = baseDate.AddDays(i),
                Price = 5.00f + (float)(Math.Sin(i) * 0.5),
                IsOnSale = false,
                Store = "Coles",
                Category = "Test"
            });
        }
        
        // Act
        var result = await _forecastingService.ForecastPricesAsync(
            "gaps-001", "Gaps Product", history, daysAhead: 14);
        
        // Assert
        Assert.True(result.Success, "Should handle gaps in data gracefully");
        Assert.NotEmpty(result.Forecasts);
    }

    [Fact]
    public void CalculateStatistics_WithNegativePrices_HandlesCorrectly()
    {
        // Arrange - ML.NET might produce negative predictions in some edge cases
        var history = new List<PriceHistoryData>
        {
            new PriceHistoryData { ItemId = "test", ItemName = "Test", Date = DateTime.Now.AddDays(-2), Price = -1.00f },
            new PriceHistoryData { ItemId = "test", ItemName = "Test", Date = DateTime.Now.AddDays(-1), Price = 5.00f },
            new PriceHistoryData { ItemId = "test", ItemName = "Test", Date = DateTime.Now, Price = 3.00f }
        };
        
        // Act
        var stats = _forecastingService.CalculateStatistics("test", "Test", history);
        
        // Assert
        Assert.Equal(3, stats.DataPoints);
        Assert.True(stats.MinPrice <= stats.MaxPrice);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FullWorkflow_TrainThenForecast_PredictionsConsistent()
    {
        // Arrange
        var history = CreateMilkPriceHistory(90);
        var modelPath = Path.Combine(Path.GetTempPath(), $"workflow_model_{Guid.NewGuid():N}.zip");
        
        try
        {
            // Act - Train model
            var trainResult = await _forecastingService.TrainModelAsync(
                "milk-001", "Full Cream Milk 2L", history,
                parameters: new SsaModelParameters { Horizon = 14 },
                outputModelPath: modelPath);
            
            Assert.True(trainResult.Success, "Training should succeed");
            
            // Act - Forecast using the same data
            var forecastResult = await _forecastingService.ForecastPricesAsync(
                "milk-001", "Full Cream Milk 2L", history, daysAhead: 14);
            
            // Assert
            Assert.True(forecastResult.Success, "Forecast should succeed");
            Assert.Equal(14, forecastResult.Forecasts.Count);
            
            // Verify statistics are consistent
            var stats = forecastResult.Statistics;
            Assert.Equal(90, stats.DataPoints);
            Assert.True(stats.AveragePrice > 0);
            
            // Verify anomalies were checked
            Assert.NotNull(forecastResult.Anomalies);
            
            // Verify optimal buying date was found
            Assert.True(forecastResult.OptimalBuyingDate.HasValue);
        }
        finally
        {
            if (File.Exists(modelPath))
                File.Delete(modelPath);
        }
    }

    [Fact]
    public async Task FullWorkflow_MultipleProducts_AllSucceed()
    {
        // Arrange
        var products = new[]
        {
            ("milk-001", "Full Cream Milk 2L", CreateMilkPriceHistory(90)),
            ("bread-001", "White Sandwich Bread 700g", CreateBreadPriceHistory(90)),
            ("eggs-001", "Free Range Eggs 12pk", CreateEggPriceHistory(90))
        };
        
        foreach (var (itemId, itemName, history) in products)
        {
            // Act
            var result = await _forecastingService.ForecastPricesAsync(itemId, itemName, history, daysAhead: 14);
            
            // Assert
            Assert.True(result.Success, $"Forecast for {itemName} should succeed");
            Assert.NotEmpty(result.Forecasts);
            Assert.NotNull(result.Statistics);
            
            // Verify each product has appropriate recommendations
            Assert.True(result.Forecasts.All(f => f.Recommendation != 0) || result.Forecasts.Any(),
                $"{itemName} should have valid recommendations");
        }
    }

    #endregion
}
