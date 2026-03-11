using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels
{
    public class ReportsViewModel : ViewModelBase
    {
        private readonly IPriceRecordRepository _priceRecordRepository;
        private readonly IItemRepository _itemRepository;
        private readonly IPlaceRepository _placeRepository;
        private readonly IReportGenerationService _reportService;
        private readonly IDialogService _dialogService;
        private readonly ILoggerService _logger;

        private int _totalItems;
        private int _totalStores;
        private int _activeDeals;
        private string _averageSavings = "$0.00";
        private List<BestDealInfo> _bestDeals = new();
        private ISeries[] _categorySeries = Array.Empty<ISeries>();
        private ISeries[] _priceTrendSeries = Array.Empty<ISeries>();
        private bool _isGeneratingReport;
        private string _reportStatusMessage = "";

        // Report generation options
        private int _selectedReportTypeIndex;
        private DateTime _reportStartDate = DateTime.Now.AddDays(-7);
        private DateTime _reportEndDate = DateTime.Now;
        private int _maxReportItems = 50;

        public ReportsViewModel(
            IPriceRecordRepository priceRecordRepository,
            IItemRepository itemRepository,
            IPlaceRepository placeRepository,
            IReportGenerationService reportService,
            IDialogService dialogService,
            ILoggerService logger)
        {
            _priceRecordRepository = priceRecordRepository;
            _itemRepository = itemRepository;
            _placeRepository = placeRepository;
            _reportService = reportService;
            _dialogService = dialogService;
            _logger = logger;

            // Commands
            GenerateReportCommand = new RelayCommand(async () => await GenerateReportAsync(), () => !IsGeneratingReport);
            ExportMarkdownCommand = new RelayCommand(async () => await ExportReportAsync(ReportFormat.Markdown), () => !IsGeneratingReport);
            ExportJsonCommand = new RelayCommand(async () => await ExportReportAsync(ReportFormat.Json), () => !IsGeneratingReport);
            ExportCsvCommand = new RelayCommand(async () => await ExportReportAsync(ReportFormat.Csv), () => !IsGeneratingReport);
            CopyReportCommand = new RelayCommand(async () => await CopyReportToClipboardAsync(), () => !IsGeneratingReport);

            LoadData();
        }

        #region Properties

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

        public bool IsGeneratingReport
        {
            get => _isGeneratingReport;
            set
            {
                SetProperty(ref _isGeneratingReport, value);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ReportStatusMessage
        {
            get => _reportStatusMessage;
            set => SetProperty(ref _reportStatusMessage, value);
        }

        // Report Options
        public int SelectedReportTypeIndex
        {
            get => _selectedReportTypeIndex;
            set => SetProperty(ref _selectedReportTypeIndex, value);
        }

        public DateTime ReportStartDate
        {
            get => _reportStartDate;
            set => SetProperty(ref _reportStartDate, value);
        }

        public DateTime ReportEndDate
        {
            get => _reportEndDate;
            set => SetProperty(ref _reportEndDate, value);
        }

        public int MaxReportItems
        {
            get => _maxReportItems;
            set => SetProperty(ref _maxReportItems, value);
        }

        public string[] ReportTypes => new[]
        {
            "Best Deals",
            "Price Trends",
            "Store Comparison",
            "Category Analysis"
        };

        #endregion

        #region Commands

        public ICommand GenerateReportCommand { get; }
        public ICommand ExportMarkdownCommand { get; }
        public ICommand ExportJsonCommand { get; }
        public ICommand ExportCsvCommand { get; }
        public ICommand CopyReportCommand { get; }

        #endregion

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
                _logger.LogError("Error loading reports data", ex);
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
                _logger.LogError("Error loading category distribution", ex);
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
                _logger.LogError("Error loading price trends", ex);
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
                _logger.LogError("Error loading best deals", ex);
                System.Diagnostics.Debug.WriteLine($"Error loading best deals: {ex.Message}");
                BestDeals = new List<BestDealInfo>();
            }
        }

        private async Task GenerateReportAsync()
        {
            IsGeneratingReport = true;
            ReportStatusMessage = "Generating report...";

            try
            {
                var options = new ReportOptions
                {
                    StartDate = ReportStartDate,
                    EndDate = ReportEndDate,
                    MaxItems = MaxReportItems,
                    ReportTitle = $"{ReportTypes[SelectedReportTypeIndex]} Report"
                };

                _logger.LogInfo($"Generating {ReportTypes[SelectedReportTypeIndex]} report");

                _lastGeneratedReport = SelectedReportTypeIndex switch
                {
                    0 => await _reportService.GenerateBestDealsReportAsync(options),
                    1 => await _reportService.GeneratePriceTrendsReportAsync(options),
                    2 => await _reportService.GenerateStoreComparisonReportAsync(options),
                    3 => await _reportService.GenerateCategoryAnalysisReportAsync(options),
                    _ => await _reportService.GenerateBestDealsReportAsync(options)
                };

                ReportStatusMessage = $"Report generated: {_lastGeneratedReport.Statistics.TotalItems} items found";
                _logger.LogInfo($"Report generated with {_lastGeneratedReport.Statistics.TotalItems} items");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error generating report", ex);
                ReportStatusMessage = $"Error generating report: {ex.Message}";
                _dialogService?.ShowError($"Failed to generate report: {ex.Message}", "Report Error");
            }
            finally
            {
                IsGeneratingReport = false;
            }
        }

        private ReportResult? _lastGeneratedReport;

        private async Task ExportReportAsync(ReportFormat format)
        {
            if (_lastGeneratedReport == null)
            {
                // Generate report first if not done
                await GenerateReportAsync();
                if (_lastGeneratedReport == null) return;
            }

            IsGeneratingReport = true;
            ReportStatusMessage = $"Exporting to {format}...";

            try
            {
                var extension = format switch
                {
                    ReportFormat.Markdown => "md",
                    ReportFormat.Json => "json",
                    ReportFormat.Csv => "csv",
                    _ => "txt"
                };

                var defaultFileName = $"{ReportTypes[SelectedReportTypeIndex].Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.{extension}";
                
                // Use a default path in Documents folder
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var outputPath = Path.Combine(documentsPath, "AdvGenPriceComparer", "Reports");
                Directory.CreateDirectory(outputPath);
                var filePath = Path.Combine(outputPath, defaultFileName);

                string savedPath = format switch
                {
                    ReportFormat.Markdown => await _reportService.ExportToMarkdownAsync(_lastGeneratedReport, filePath),
                    ReportFormat.Json => await _reportService.ExportToJsonAsync(_lastGeneratedReport, filePath),
                    ReportFormat.Csv => await _reportService.ExportToCsvAsync(_lastGeneratedReport, filePath),
                    _ => throw new ArgumentOutOfRangeException(nameof(format))
                };

                ReportStatusMessage = $"Report saved to: {savedPath}";
                _logger.LogInfo($"Report exported to {savedPath}");
                
                _dialogService?.ShowSuccess($"Report saved to:\n{savedPath}", "Export Complete");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error exporting report to {format}", ex);
                ReportStatusMessage = $"Error exporting: {ex.Message}";
                _dialogService?.ShowError($"Failed to export report: {ex.Message}", "Export Error");
            }
            finally
            {
                IsGeneratingReport = false;
            }
        }

        private async Task CopyReportToClipboardAsync()
        {
            if (_lastGeneratedReport == null)
            {
                await GenerateReportAsync();
                if (_lastGeneratedReport == null) return;
            }

            try
            {
                var preview = await _reportService.GetReportPreviewAsync(_lastGeneratedReport, ReportFormat.Markdown);
                
                // Copy to clipboard
                System.Windows.Clipboard.SetText(preview);
                
                ReportStatusMessage = "Report copied to clipboard";
                _logger.LogInfo("Report copied to clipboard");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error copying report to clipboard", ex);
                ReportStatusMessage = $"Error copying: {ex.Message}";
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
