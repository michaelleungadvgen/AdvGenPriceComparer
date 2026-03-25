using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class PriceComparisonViewModel
{
    private readonly IMediator _mediator;

    public PriceComparisonViewModel(IMediator mediator, string category = null)
    {
        _mediator = mediator;
        LoadComparisonData(category);
    }

    public List<StoreComparisonItem> StoreComparisons { get; private set; } = new();

    public string Title => "Price Comparison - Store vs Store";

    public int TotalItems => StoreComparisons.Sum(s => s.ItemCount);

    private void LoadComparisonData(string category)
    {
        var storeStats = _mediator.Send(new GetStoreComparisonStatsQuery()).GetAwaiter().GetResult().ToList();

        if (storeStats.Any())
        {
            StoreComparisons = storeStats.Select(stat => new StoreComparisonItem
            {
                StoreName = stat.StoreName,
                AveragePrice = stat.AveragePrice,
                ItemCount = stat.ProductCount,
                IsLowest = stat.AveragePrice == storeStats.Min(s => s.AveragePrice)
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
