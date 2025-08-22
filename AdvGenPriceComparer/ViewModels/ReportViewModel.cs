using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using Microsoft.UI.Xaml.Media;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

public class ReportViewModel : BaseViewModel
{
    private readonly IGroceryDataService _groceryDataService;

    public ReportViewModel(IGroceryDataService groceryDataService)
    {
        _groceryDataService = groceryDataService;
        SelectedTimePeriod = "Last 7 Days";
        SelectedStoreChain = "All Stores";
    }

    #region Properties

    private int _totalProducts;
    public int TotalProducts
    {
        get => _totalProducts;
        set => SetProperty(ref _totalProducts, value);
    }

    private decimal _avgPriceChange;
    public decimal AvgPriceChange
    {
        get => _avgPriceChange;
        set => SetProperty(ref _avgPriceChange, value);
    }

    private string? _bestStore;
    public string? BestStore
    {
        get => _bestStore;
        set => SetProperty(ref _bestStore, value);
    }

    private int _priceUpdates;
    public int PriceUpdates
    {
        get => _priceUpdates;
        set => SetProperty(ref _priceUpdates, value);
    }

    public string SelectedTimePeriod { get; set; }
    public string SelectedStoreChain { get; set; }

    public List<PriceTrendPoint> PriceTrendsData { get; private set; } = new();
    public List<StoreComparisonPoint> StoreComparisonData { get; private set; } = new();
    public List<CategoryBreakdownPoint> CategoryBreakdownData { get; private set; } = new();
    public ObservableCollection<TopProductItem> TopProductsData { get; private set; } = new();

    #endregion

    public async Task LoadReportDataAsync()
    {
        try
        {
            var stats = _groceryDataService.GetDashboardStats();
            
            TotalProducts = (int)stats.GetValueOrDefault("totalItems", 0);
            PriceUpdates = (int)stats.GetValueOrDefault("priceRecords", 0);

            await LoadPriceTrendsData();
            await LoadStoreComparisonData();
            await LoadCategoryBreakdownData();
            await LoadTopProductsData();
            
            CalculateMetrics();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading report data: {ex.Message}");
        }
    }

    private async Task LoadPriceTrendsData()
    {
        try
        {
            var days = GetDaysFromTimePeriod(SelectedTimePeriod);
            var fromDate = DateTime.Now.AddDays(-days);
            
            var priceHistory = _groceryDataService.GetPriceHistory(from: fromDate);
            var trendData = new List<PriceTrendPoint>();

            // Group by date and calculate daily averages
            var dailyGroups = priceHistory
                .GroupBy(p => p.DateRecorded.Date)
                .OrderBy(g => g.Key);

            foreach (var dayGroup in dailyGroups)
            {
                var prices = dayGroup.Select(p => p.Price).ToList();
                if (prices.Any())
                {
                    trendData.Add(new PriceTrendPoint
                    {
                        Date = dayGroup.Key,
                        AvgPrice = prices.Average(),
                        MinPrice = prices.Min(),
                        MaxPrice = prices.Max()
                    });
                }
            }

            // If no real data or insufficient data, fall back to sample data
            if (trendData.Count < 3)
            {
                var random = new Random();
                var basePrice = 5.50m;
                trendData.Clear();
                
                for (int i = 0; i < days; i++)
                {
                    var date = DateTime.Now.AddDays(-days + i);
                    var variance = (decimal)(random.NextDouble() * 2 - 1);
                    
                    var avgPrice = basePrice + variance;
                    var minPrice = avgPrice - (decimal)(random.NextDouble() * 0.5);
                    var maxPrice = avgPrice + (decimal)(random.NextDouble() * 0.5);

                    trendData.Add(new PriceTrendPoint
                    {
                        Date = date,
                        AvgPrice = Math.Max(0.50m, avgPrice),
                        MinPrice = Math.Max(0.10m, minPrice),
                        MaxPrice = maxPrice + 1.0m
                    });
                }
            }

            PriceTrendsData = trendData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading price trends data: {ex.Message}");
            PriceTrendsData = new List<PriceTrendPoint>();
        }
    }

    private async Task LoadStoreComparisonData()
    {
        try
        {
            var storeStats = _groceryDataService.GetStoreComparisonStats();
            var storeData = new List<StoreComparisonPoint>();

            foreach (var (storeName, avgPrice, productCount) in storeStats)
            {
                if (SelectedStoreChain != "All Stores" && SelectedStoreChain != storeName)
                    continue;

                storeData.Add(new StoreComparisonPoint
                {
                    StoreName = storeName,
                    AvgPrice = avgPrice,
                    ProductCount = productCount
                });
            }

            // If no real data, fall back to sample data
            if (!storeData.Any())
            {
                var random = new Random();
                var stores = new[] { "Coles", "Woolworths", "IGA", "ALDI", "Foodworks" };
                
                foreach (var store in stores)
                {
                    if (SelectedStoreChain != "All Stores" && SelectedStoreChain != store)
                        continue;

                    storeData.Add(new StoreComparisonPoint
                    {
                        StoreName = store,
                        AvgPrice = (decimal)(4.0 + random.NextDouble() * 3.0),
                        ProductCount = random.Next(50, 200)
                    });
                }
            }

            StoreComparisonData = storeData;
            
            // Determine best store
            BestStore = storeData.OrderBy(s => s.AvgPrice).FirstOrDefault()?.StoreName;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading store comparison data: {ex.Message}");
            StoreComparisonData = new List<StoreComparisonPoint>();
        }
    }

    private async Task LoadCategoryBreakdownData()
    {
        try
        {
            var categoryStats = _groceryDataService.GetCategoryStats();
            var categoryData = new List<CategoryBreakdownPoint>();

            foreach (var (category, avgPrice, count) in categoryStats)
            {
                categoryData.Add(new CategoryBreakdownPoint
                {
                    Category = category,
                    TotalSpent = avgPrice * count, // Approximate total spending
                    ProductCount = count
                });
            }

            // If no real data, fall back to sample data
            if (!categoryData.Any())
            {
                var random = new Random();
                var categories = new[] 
                { 
                    "Fruits & Vegetables", "Meat & Seafood", "Dairy & Eggs", 
                    "Bakery", "Pantry", "Frozen", "Beverages", "Snacks" 
                };

                foreach (var category in categories)
                {
                    categoryData.Add(new CategoryBreakdownPoint
                    {
                        Category = category,
                        TotalSpent = (decimal)(random.NextDouble() * 150 + 50),
                        ProductCount = random.Next(10, 50)
                    });
                }
            }

            CategoryBreakdownData = categoryData;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading category breakdown data: {ex.Message}");
            CategoryBreakdownData = new List<CategoryBreakdownPoint>();
        }
    }

    private async Task LoadTopProductsData()
    {
        TopProductsData.Clear();
        var random = new Random();

        var products = new[]
        {
            "Banana - 1kg", "Milk - 2L", "Bread - White", "Eggs - Dozen",
            "Chicken Breast - 1kg", "Apples - 1kg", "Rice - 2kg", "Pasta - 500g",
            "Yogurt - 1kg", "Cheese - 500g", "Tomatoes - 1kg", "Potatoes - 2kg"
        };

        foreach (var product in products.Take(8))
        {
            var minPrice = (decimal)(1.0 + random.NextDouble() * 3.0);
            var maxPrice = minPrice + (decimal)(random.NextDouble() * 2.0);
            var avgPrice = (minPrice + maxPrice) / 2;
            var variance = ((maxPrice - minPrice) / avgPrice) * 100;

            var varianceColor = variance > 20 ? new SolidColorBrush(Microsoft.UI.Colors.Red) :
                               variance > 10 ? new SolidColorBrush(Microsoft.UI.Colors.Orange) :
                               new SolidColorBrush(Microsoft.UI.Colors.Green);

            TopProductsData.Add(new TopProductItem
            {
                ProductName = product,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                AvgPrice = avgPrice,
                Variance = variance,
                VarianceColor = varianceColor
            });
        }
    }

    private void CalculateMetrics()
    {
        if (PriceTrendsData.Count >= 2)
        {
            var firstPrice = PriceTrendsData.First().AvgPrice;
            var lastPrice = PriceTrendsData.Last().AvgPrice;
            AvgPriceChange = ((lastPrice - firstPrice) / firstPrice) * 100;
        }
    }

    private int GetDaysFromTimePeriod(string timePeriod)
    {
        return timePeriod switch
        {
            "Last 7 Days" => 7,
            "Last 30 Days" => 30,
            "Last 3 Months" => 90,
            "Last 6 Months" => 180,
            "Last Year" => 365,
            _ => 7
        };
    }
}

// Chart Data Models
public class PriceTrendPoint
{
    public DateTime Date { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
}

public class StoreComparisonPoint
{
    public string StoreName { get; set; } = string.Empty;
    public decimal AvgPrice { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryBreakdownPoint
{
    public string Category { get; set; } = string.Empty;
    public decimal TotalSpent { get; set; }
    public int ProductCount { get; set; }
}

public class TopProductItem
{
    public string ProductName { get; set; } = string.Empty;
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public decimal AvgPrice { get; set; }
    public decimal Variance { get; set; }
    public SolidColorBrush VarianceColor { get; set; } = new(Microsoft.UI.Colors.Black);
}