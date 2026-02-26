using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase, IDisposable
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private int _totalItems;
    private int _trackedStores;
    private int _priceUpdates;

    public MainWindowViewModel(
        IGroceryDataService dataService,
        IDialogService dialogService)
    {
        _dataService = dataService;
        _dialogService = dialogService;

        AddItemCommand = new RelayCommand(AddItem);
        AddPlaceCommand = new RelayCommand(AddPlace);
        ComparePricesCommand = new RelayCommand(ComparePrices);
        FavoritesCommand = new RelayCommand(ShowFavorites);
        GlobalSearchCommand = new RelayCommand(ShowGlobalSearch);
        ScanBarcodeCommand = new RelayCommand(ScanBarcode);
        PriceDropNotificationsCommand = new RelayCommand(ShowPriceDropNotifications);
        DealExpirationRemindersCommand = new RelayCommand(ShowDealExpirationReminders);
        WeeklySpecialsDigestCommand = new RelayCommand(ShowWeeklySpecialsDigest);

        RefreshDashboard();
    }

    public int TotalItems
    {
        get => _totalItems;
        set => SetProperty(ref _totalItems, value);
    }

    public int TrackedStores
    {
        get => _trackedStores;
        set => SetProperty(ref _trackedStores, value);
    }

    public int PriceUpdates
    {
        get => _priceUpdates;
        set => SetProperty(ref _priceUpdates, value);
    }

    public ICommand AddItemCommand { get; }
    public ICommand AddPlaceCommand { get; }
    public ICommand ComparePricesCommand { get; }
    public ICommand FavoritesCommand { get; }
    public ICommand GlobalSearchCommand { get; }
    public ICommand ScanBarcodeCommand { get; }
    public ICommand PriceDropNotificationsCommand { get; }
    public ICommand DealExpirationRemindersCommand { get; }
    public ICommand WeeklySpecialsDigestCommand { get; }

    public event Action? OnStoreAdded;

    // Chart properties
    public ISeries[] CategorySeries { get; private set; } = Array.Empty<ISeries>();
    public ISeries[] PriceTrendSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();

    public void RefreshDashboard()
    {
        try
        {
            TotalItems = _dataService.GetAllItems().Count();
            TrackedStores = _dataService.GetAllPlaces().Count();
            PriceUpdates = _dataService.GetRecentPriceUpdates(1000).Count();

            LoadChartData();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing dashboard: {ex.Message}");
        }
    }

    private void LoadChartData()
    {
        try
        {
            // Load Items by Category
            var categoryStats = _dataService.GetCategoryStats().ToList();

            if (categoryStats.Any())
            {
                CategorySeries = categoryStats.Select(stat => new PieSeries<int>
                {
                    Values = new[] { stat.count },
                    Name = stat.category,
                    DataLabelsPaint = new SolidColorPaint(SKColors.White),
                    DataLabelsSize = 12,
                    DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Middle
                }).ToArray();
            }
            else
            {
                // Empty state
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

            // Load Price Trends (last 30 days)
            var priceHistory = _dataService.GetPriceHistory(
                itemId: null,
                placeId: null,
                from: DateTime.Now.AddDays(-30),
                to: DateTime.Now).ToList();

            if (priceHistory.Any())
            {
                var groupedByDate = priceHistory
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

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = groupedByDate.Select(g => g.Date.ToString("MM/dd")).ToArray(),
                        LabelsRotation = 45
                    }
                };
            }
            else
            {
                // Empty state
                PriceTrendSeries = new ISeries[]
                {
                    new LineSeries<int>
                    {
                        Values = new[] { 0, 0, 0 },
                        Name = "No data",
                        Stroke = new SolidColorPaint(SKColors.LightGray) { StrokeThickness = 2 }
                    }
                };

                XAxes = new Axis[]
                {
                    new Axis
                    {
                        Labels = new[] { "Day 1", "Day 2", "Day 3" }
                    }
                };
            }

            OnPropertyChanged(nameof(CategorySeries));
            OnPropertyChanged(nameof(PriceTrendSeries));
            OnPropertyChanged(nameof(XAxes));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading chart data: {ex.Message}");
        }
    }

    private void AddItem()
    {
        var viewModel = new AddItemViewModel(_dataService, _dialogService);
        var window = new Views.AddItemWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            RefreshDashboard();
        }
    }

    private void AddPlace()
    {
        var viewModel = new AddStoreViewModel(_dataService, _dialogService);
        var window = new AddStoreWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            RefreshDashboard();
            OnStoreAdded?.Invoke();
        }
    }

    private void ComparePrices()
    {
        _dialogService.ShowComparePricesDialog();
    }

    private void ShowGlobalSearch()
    {
        var result = _dialogService.ShowGlobalSearchDialog();
        
        // If a result was selected, refresh dashboard
        if (result != null)
        {
            RefreshDashboard();
        }
    }

    private void ScanBarcode()
    {
        _dialogService.ShowBarcodeScannerDialog();
    }

    private void ShowPriceDropNotifications()
    {
        _dialogService.ShowPriceDropNotificationsDialog();
    }

    private void ShowDealExpirationReminders()
    {
        _dialogService.ShowDealExpirationRemindersDialog();
    }

    private void ShowWeeklySpecialsDigest()
    {
        _dialogService.ShowWeeklySpecialsDigestDialog();
    }

    private void ShowFavorites()
    {
        _dialogService.ShowFavoritesDialog();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
