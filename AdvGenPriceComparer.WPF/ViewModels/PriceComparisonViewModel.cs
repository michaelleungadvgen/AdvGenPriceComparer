using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class PriceComparisonViewModel
{
    private readonly IGroceryDataService _dataService;

    public PriceComparisonViewModel(IGroceryDataService dataService, string category = null)
    {
        _dataService = dataService;
        LoadComparisonData(category);
    }

    public List<StoreComparisonItem> StoreComparisons { get; private set; } = new();

    public string Title => "Price Comparison - Store vs Store";

    public int TotalItems => StoreComparisons.Sum(s => s.ItemCount);

    private void LoadComparisonData(string category)
    {
        var storeStats = _dataService.GetStoreComparisonStats().ToList();

        if (storeStats.Any())
        {
            StoreComparisons = storeStats.Select(stat => new StoreComparisonItem
            {
                StoreName = stat.storeName,
                AveragePrice = stat.avgPrice,
                ItemCount = stat.productCount,
                IsLowest = stat.avgPrice == storeStats.Min(s => s.avgPrice)
            }).OrderBy(s => s.AveragePrice).ToList();
        }
    }
}

public class StoreComparisonItem
{
    public string StoreName { get; set; } = string.Empty;
    public decimal AveragePrice { get; set; }
    public int ItemCount { get; set; }
    public bool IsLowest { get; set; }
    public string AveragePriceDisplay => $"${AveragePrice:F2}";
}
