using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.ML.Services;
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
    private readonly IMediator _mediator;
    private readonly IDialogService _dialogService;
    private readonly CategoryPredictionService? _categoryPredictionService;
    private int _totalItems;
    private int _trackedStores;
    private int _priceUpdates;

    public MainWindowViewModel(
        IGroceryDataService dataService,
        IMediator mediator,
        IDialogService dialogService,
        CategoryPredictionService? categoryPredictionService = null)
    {
        _dataService = dataService;
        _mediator = mediator;
        _dialogService = dialogService;
        _categoryPredictionService = categoryPredictionService;

        AddItemCommand = new RelayCommand(AddItem);
        AddPlaceCommand = new RelayCommand(AddPlace);
        ComparePricesCommand = new RelayCommand(ComparePrices);
        FavoritesCommand = new RelayCommand(ShowFavorites);
        GlobalSearchCommand = new RelayCommand(ShowGlobalSearch);
        ScanBarcodeCommand = new RelayCommand(ScanBarcode);
        PriceDropNotificationsCommand = new RelayCommand(ShowPriceDropNotifications);
        PriceAlertsCommand = new RelayCommand(ShowPriceAlerts);
        DealExpirationRemindersCommand = new RelayCommand(ShowDealExpirationReminders);
        WeeklySpecialsDigestCommand = new RelayCommand(ShowWeeklySpecialsDigest);
        ShoppingListsCommand = new RelayCommand(ShowShoppingLists);
        SettingsCommand = new RelayCommand(ShowSettings);
        MLModelManagementCommand = new RelayCommand(ShowMLModelManagement);
        PriceForecastCommand = new RelayCommand(ShowPriceForecast);
        IllusoryDiscountDetectionCommand = new RelayCommand(ShowIllusoryDiscountDetection);
        ChatCommand = new RelayCommand(ShowChat);
        BestPricesCommand = new RelayCommand(ShowBestPrices);

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
    public ICommand PriceAlertsCommand { get; }
    public ICommand DealExpirationRemindersCommand { get; }
    public ICommand WeeklySpecialsDigestCommand { get; }
    public ICommand ShoppingListsCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand MLModelManagementCommand { get; }
    public ICommand PriceForecastCommand { get; }
    public ICommand IllusoryDiscountDetectionCommand { get; }
    public ICommand ChatCommand { get; }
    public ICommand BestPricesCommand { get; }

    public event Action? OnStoreAdded;

    // Chart properties
    public ISeries[] CategorySeries { get; private set; } = Array.Empty<ISeries>();
    public ISeries[] PriceTrendSeries { get; private set; } = Array.Empty<ISeries>();
    public Axis[] XAxes { get; private set; } = Array.Empty<Axis>();

    public void RefreshDashboard()
    {
        try
        {
            TotalItems = _mediator.Send(new GetAllItemsQuery()).GetAwaiter().GetResult().Count();
            TrackedStores = _mediator.Send(new GetAllPlacesQuery()).GetAwaiter().GetResult().Count();
            PriceUpdates = _mediator.Send(new GetRecentPriceUpdatesQuery(1000)).GetAwaiter().GetResult().Count();

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
            var categoryStats = _mediator.Send(new GetCategoryStatsQuery()).GetAwaiter().GetResult().ToList();

            if (categoryStats.Any())
            {
                CategorySeries = categoryStats.Select(stat => new PieSeries<int>
                {
                    Values = new[] { stat.ItemCount },
                    Name = stat.Category,
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
            var priceHistory = _mediator.Send(new GetPriceHistoryQuery(
                ItemId: null,
                PlaceId: null,
                From: DateTime.Now.AddDays(-30),
                To: DateTime.Now)).GetAwaiter().GetResult().ToList();

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
        var viewModel = new AddItemViewModel(_mediator, _dialogService, _categoryPredictionService);
        var window = new Views.AddItemWindow(viewModel);

        if (window.ShowDialog() == true)
        {
            RefreshDashboard();
        }
    }

    private void AddPlace()
    {
        var viewModel = new AddStoreViewModel(_mediator, _dialogService);
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

    private void ShowPriceAlerts()
    {
        _dialogService.ShowPriceAlertsDialog();
    }

    private void ShowDealExpirationReminders()
    {
        _dialogService.ShowDealExpirationRemindersDialog();
    }

    private void ShowWeeklySpecialsDigest()
    {
        _dialogService.ShowWeeklySpecialsDigestDialog();
    }

    private void ShowShoppingLists()
    {
        _dialogService.ShowShoppingListsDialog();
    }

    private void ShowSettings()
    {
        _dialogService.ShowSettingsDialog();
    }

    private void ShowMLModelManagement()
    {
        _dialogService.ShowMLModelManagementDialog();
    }

    private void ShowPriceForecast()
    {
        _dialogService.ShowPriceForecastDialog();
    }

    private void ShowIllusoryDiscountDetection()
    {
        _dialogService.ShowIllusoryDiscountDetectionDialog();
    }

    private void ShowChat()
    {
        _dialogService.ShowChatDialog();
    }

    private void ShowFavorites()
    {
        _dialogService.ShowFavoritesDialog();
    }

    private void ShowBestPrices()
    {
        _dialogService.ShowBestPricesDialog();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
