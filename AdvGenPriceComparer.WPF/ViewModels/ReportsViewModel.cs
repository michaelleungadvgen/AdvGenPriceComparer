using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdvGenPriceComparer.WPF.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly IPriceRecordRepository _priceRecordRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPlaceRepository _placeRepository;

        private int _totalItems;
        private int _totalStores;
        private int _activeDeals;
        private string _averageSavings = "$0.00";
        private List<BestDealInfo> _bestDeals = new();
        private ISeries[] _categorySeries = Array.Empty<ISeries>();
        private ISeries[] _priceTrendSeries = Array.Empty<ISeries>();

        public ReportsViewModel(
            IPriceRecordRepository priceRecordRepository,
            IItemRepository itemRepository,
            IPlaceRepository placeRepository)
        {
            _priceRecordRepository = priceRecordRepository;
            _itemRepository = itemRepository;
            _placeRepository = placeRepository;

            LoadData();
        }

        public int TotalItems
        {
            get => _totalItems;
            set => SetProperty(ref _totalItems, value);
        }

        public int TotalStores
        {
            get => _totalStores;
            set => SetProperty(ref _totalStores, value);
        }

        public int ActiveDeals
        {
            get => _activeDeals;
            set => SetProperty(ref _activeDeals, value);
        }

        public string AverageSavings
        {
            get => _averageSavings;
            set => SetProperty(ref _averageSavings, value);
        }

        public List<BestDealInfo> BestDeals
        {
            get => _bestDeals;
            set => SetProperty(ref _bestDeals, value);
        }

        public ISeries[] CategorySeries
        {
            get => _categorySeries;
            set => SetProperty(ref _categorySeries, value);
        }

        public ISeries[] PriceTrendSeries
        {
            get => _priceTrendSeries;
            set => SetProperty(ref _priceTrendSeries, value);
        }

        private void LoadData()
        {
            try
            {
                LoadStatistics();
                LoadCategoryDistribution();
                LoadPriceTrends();
                LoadBestDeals();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading reports data: {ex.Message}");
            }
        }

        private void LoadStatistics()
        {
            var items = _itemRepository.GetAll();
            var places = _placeRepository.GetAll();
            var priceRecords = _priceRecordRepository.GetAll();

            TotalItems = items.Count();
            TotalStores = places.Count();
            
            // Count deals (records in the last 30 days that are on sale)
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            var recentRecords = priceRecords.Where(pr => pr.DateRecorded >= thirtyDaysAgo).ToList();
            
            ActiveDeals = recentRecords.Count(pr => pr.IsOnSale);

            // Calculate average savings (rough estimate based on items with multiple prices)
            var itemsWithMultiplePrices = priceRecords
                .GroupBy(pr => pr.ItemId)
                .Where(g => g.Count() > 1)
                .ToList();

            if (itemsWithMultiplePrices.Any())
            {
                var totalSavings = 0m;
                var itemCount = 0;

                foreach (var group in itemsWithMultiplePrices)
                {
                    var prices = group.Select(pr => pr.Price).OrderBy(p => p).ToList();
                    if (prices.Count >= 2)
                    {
                        var savings = prices.Last() - prices.First();
                        totalSavings += savings;
                        itemCount++;
                    }
                }

                var avg = itemCount > 0 ? totalSavings / itemCount : 0;
                AverageSavings = $"${avg:F2}";
            }
        }

        private void LoadCategoryDistribution()
        {
            try
            {
                var items = _itemRepository.GetAll();
                var categoryCounts = items
                    .Where(i => !string.IsNullOrEmpty(i.Category))
                    .GroupBy(i => i.Category)
                    .Select(g => new { Category = g.Key ?? "Uncategorized", Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .Take(8)
                    .ToList();

                if (categoryCounts.Any())
                {
                    var colors = new[]
                    {
                        SKColor.Parse("#2196F3"),
                        SKColor.Parse("#4CAF50"),
                        SKColor.Parse("#FF9800"),
                        SKColor.Parse("#E91E63"),
                        SKColor.Parse("#9C27B0"),
                        SKColor.Parse("#00BCD4"),
                        SKColor.Parse("#FF5722"),
                        SKColor.Parse("#607D8B")
                    };

                    CategorySeries = categoryCounts.Select((stat, index) => new PieSeries<int>
                    {
                        Values = new[] { stat.Count },
                        Name = stat.Category,
                        Fill = new SolidColorPaint(colors[index % colors.Length]),
                        DataLabelsPaint = new SolidColorPaint(SKColors.White),
                        DataLabelsSize = 12,
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                    }).ToArray();
                }
                else
                {
                    CategorySeries = new ISeries[]
                    {
                        new PieSeries<int>
                        {
                            Values = new[] { 1 },
                            Name = "No data",
                            Fill = new SolidColorPaint(SKColors.LightGray)
                        }
                    };
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading category distribution: {ex.Message}");
                CategorySeries = Array.Empty<ISeries>();
            }
        }

        private void LoadPriceTrends()
        {
            try
            {
                var priceRecords = _priceRecordRepository.GetAll()
                    .Where(pr => pr.DateRecorded >= DateTime.Now.AddDays(-30))
                    .ToList();

                if (priceRecords.Any())
                {
                    var groupedByDate = priceRecords
                        .GroupBy(p => p.DateRecorded.Date)
                        .OrderBy(g => g.Key)
                        .Select(g => new { Date = g.Key, Count = g.Count() })
                        .ToList();

                    PriceTrendSeries = new ISeries[]
                    {
                        new LineSeries<int>
                        {
                            Values = groupedByDate.Select(g => g.Count).ToArray(),
                            Name = "Price Updates",
                            Fill = null,
                            GeometrySize = 8,
                            Stroke = new SolidColorPaint(SKColor.Parse("#0078d4")) { StrokeThickness = 3 }
                        }
                    };
                }
                else
                {
                    PriceTrendSeries = Array.Empty<ISeries>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading price trends: {ex.Message}");
                PriceTrendSeries = Array.Empty<ISeries>();
            }
        }

        private void LoadBestDeals()
        {
            try
            {
                var priceRecords = _priceRecordRepository.GetAll()
                    .Where(pr => pr.DateRecorded >= DateTime.Now.AddDays(-7))
                    .ToList();

                var items = _itemRepository.GetAll().ToDictionary(i => i.Id, i => i);
                var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);

                var deals = priceRecords
                    .Where(pr => pr.IsOnSale && pr.OriginalPrice.HasValue && pr.OriginalPrice > pr.Price)
                    .Select(pr =>
                    {
                        items.TryGetValue(pr.ItemId, out var item);
                        places.TryGetValue(pr.PlaceId, out var place);

                        var savings = pr.OriginalPrice.Value - pr.Price;
                        var discountPercent = pr.OriginalPrice.Value > 0 
                            ? (savings / pr.OriginalPrice.Value * 100) 
                            : 0;

                        return new BestDealInfo
                        {
                            ItemName = item?.Name ?? "Unknown Item",
                            StoreName = place?.Name ?? "Unknown Store",
                            Category = item?.Category ?? "Uncategorized",
                            Price = pr.Price,
                            OriginalPrice = pr.OriginalPrice,
                            PriceDisplay = $"${pr.Price:F2}",
                            SavingsDisplay = $"Save ${savings:F2}",
                            DiscountPercentDisplay = $"{discountPercent:F0}% off"
                        };
                    })
                    .OrderBy(d => d.Price)
                    .Take(10)
                    .ToList();

                BestDeals = deals;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading best deals: {ex.Message}");
                BestDeals = new List<BestDealInfo>();
            }
        }
    }

    public class BestDealInfo
    {
        public string ItemName { get; set; } = "";
        public string StoreName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string PriceDisplay { get; set; } = "";
        public string SavingsDisplay { get; set; } = "";
        public string DiscountPercentDisplay { get; set; } = "";
    }
}
