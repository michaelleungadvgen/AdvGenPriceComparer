using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class PriceHistoryViewModel : ViewModelBase
{
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IDialogService _dialogService;
    private ObservableCollection<PriceRecordViewModel> _priceRecords;
    private ObservableCollection<Item> _items;
    private ObservableCollection<Place> _places;
    private PriceRecordViewModel? _selectedPriceRecord;
    private Item? _selectedItem;
    private Item? _selectedChartItem;
    private Place? _selectedPlace;
    private Place? _selectedComparisonStore;
    private DateTime? _fromDate;
    private DateTime? _toDate;
    private bool _showOnlySales;
    private bool _showChartSection = false;
    private string _searchText = string.Empty;
    private List<PriceRecordViewModel> _allPriceRecords = new();
    
    // Chart properties
    private ISeries[] _priceHistorySeries = Array.Empty<ISeries>();
    private Axis[] _chartXAxes = Array.Empty<Axis>();
    private Axis[] _chartYAxes = Array.Empty<Axis>();

    public PriceHistoryViewModel(
        IPriceRecordRepository priceRecordRepository,
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IDialogService dialogService)
    {
        _priceRecordRepository = priceRecordRepository;
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _dialogService = dialogService;
        _priceRecords = new ObservableCollection<PriceRecordViewModel>();
        _items = new ObservableCollection<Item>();
        _places = new ObservableCollection<Place>();

        AddPriceRecordCommand = new RelayCommand(AddPriceRecord);
        EditPriceRecordCommand = new RelayCommand<PriceRecordViewModel>(EditPriceRecord);
        DeletePriceRecordCommand = new RelayCommand<PriceRecordViewModel>(DeletePriceRecord);
        RefreshCommand = new RelayCommand(LoadPriceRecords);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        ToggleChartViewCommand = new RelayCommand(ToggleChartView);
        ViewItemChartCommand = new RelayCommand<Item>(ViewItemChart);

        LoadData();
    }

    public ObservableCollection<PriceRecordViewModel> PriceRecords
    {
        get => _priceRecords;
        set => SetProperty(ref _priceRecords, value);
    }

    public ObservableCollection<Item> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ObservableCollection<Place> Places
    {
        get => _places;
        set => SetProperty(ref _places, value);
    }

    public PriceRecordViewModel? SelectedPriceRecord
    {
        get => _selectedPriceRecord;
        set => SetProperty(ref _selectedPriceRecord, value);
    }

    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (SetProperty(ref _selectedItem, value))
            {
                FilterPriceRecords();
                // Auto-show chart when item is selected
                if (value != null)
                {
                    SelectedChartItem = value;
                    UpdateChart();
                }
            }
        }
    }

    public Item? SelectedChartItem
    {
        get => _selectedChartItem;
        set
        {
            if (SetProperty(ref _selectedChartItem, value))
            {
                UpdateChart();
            }
        }
    }

    public Place? SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            if (SetProperty(ref _selectedPlace, value))
            {
                FilterPriceRecords();
            }
        }
    }

    public Place? SelectedComparisonStore
    {
        get => _selectedComparisonStore;
        set
        {
            if (SetProperty(ref _selectedComparisonStore, value))
            {
                UpdateChart();
            }
        }
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set
        {
            if (SetProperty(ref _fromDate, value))
            {
                FilterPriceRecords();
            }
        }
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set
        {
            if (SetProperty(ref _toDate, value))
            {
                FilterPriceRecords();
            }
        }
    }

    public bool ShowOnlySales
    {
        get => _showOnlySales;
        set
        {
            if (SetProperty(ref _showOnlySales, value))
            {
                FilterPriceRecords();
            }
        }
    }

    public bool ShowChartSection
    {
        get => _showChartSection;
        set => SetProperty(ref _showChartSection, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                FilterPriceRecords();
            }
        }
    }

    // Chart Properties
    public ISeries[] PriceHistorySeries
    {
        get => _priceHistorySeries;
        set => SetProperty(ref _priceHistorySeries, value);
    }

    public Axis[] ChartXAxes
    {
        get => _chartXAxes;
        set => SetProperty(ref _chartXAxes, value);
    }

    public Axis[] ChartYAxes
    {
        get => _chartYAxes;
        set => SetProperty(ref _chartYAxes, value);
    }

    // Chart Statistics
    public string ChartTitle => SelectedChartItem != null 
        ? $"Price History: {SelectedChartItem.Name}" 
        : "Price History";
    
    public string ChartSubtitle => SelectedChartItem != null 
        ? $"Track price changes over time for {SelectedChartItem.Brand} {SelectedChartItem.Name}" 
        : "Select an item to view price history";

    public string CurrentPriceText => GetCurrentPrice()?.ToString("$0.00") ?? "N/A";
    public string LowestPriceText => GetLowestPrice()?.ToString("$0.00") ?? "N/A";
    public string HighestPriceText => GetHighestPrice()?.ToString("$0.00") ?? "N/A";
    public string AveragePriceText => GetAveragePrice()?.ToString("$0.00") ?? "N/A";

    public ObservableCollection<Place> AvailableStores => _places;

    public string RecordCountText => $"{PriceRecords.Count} price records";

    public ICommand AddPriceRecordCommand { get; }
    public ICommand EditPriceRecordCommand { get; }
    public ICommand DeletePriceRecordCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearFiltersCommand { get; }
    public ICommand ToggleChartViewCommand { get; }
    public ICommand ViewItemChartCommand { get; }

    private void LoadData()
    {
        LoadItems();
        LoadPlaces();
        LoadPriceRecords();
    }

    private void LoadItems()
    {
        try
        {
            var items = _itemRepository.GetAll().OrderBy(i => i.Name);
            Items.Clear();
            foreach (var item in items)
            {
                Items.Add(item);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load items: {ex.Message}");
        }
    }

    private void LoadPlaces()
    {
        try
        {
            var places = _placeRepository.GetAll().OrderBy(p => p.Name);
            Places.Clear();
            foreach (var place in places)
            {
                Places.Add(place);
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load places: {ex.Message}");
        }
    }

    private void LoadPriceRecords()
    {
        try
        {
            var records = _priceRecordRepository.GetAll();
            var viewModels = records.Select(r => new PriceRecordViewModel(r, _itemRepository, _placeRepository)).ToList();
            _allPriceRecords = viewModels;
            FilterPriceRecords();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load price records: {ex.Message}");
        }
    }

    private void FilterPriceRecords()
    {
        var filtered = _allPriceRecords.AsEnumerable();

        // Filter by item
        if (SelectedItem != null)
        {
            filtered = filtered.Where(r => r.ItemId == SelectedItem.Id);
        }

        // Filter by place
        if (SelectedPlace != null)
        {
            filtered = filtered.Where(r => r.PlaceId == SelectedPlace.Id);
        }

        // Filter by date range
        if (FromDate.HasValue)
        {
            filtered = filtered.Where(r => r.DateRecorded >= FromDate.Value);
        }
        if (ToDate.HasValue)
        {
            filtered = filtered.Where(r => r.DateRecorded <= ToDate.Value);
        }

        // Filter by sales only
        if (ShowOnlySales)
        {
            filtered = filtered.Where(r => r.IsOnSale);
        }

        // Filter by search text
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var search = SearchText.ToLower();
            filtered = filtered.Where(r => 
                (r.ItemName?.ToLower().Contains(search) ?? false) ||
                (r.PlaceName?.ToLower().Contains(search) ?? false) ||
                (r.SaleDescription?.ToLower().Contains(search) ?? false));
        }

        PriceRecords.Clear();
        foreach (var record in filtered.OrderByDescending(r => r.DateRecorded))
        {
            PriceRecords.Add(record);
        }

        OnPropertyChanged(nameof(RecordCountText));
    }

    private void ClearFilters()
    {
        SelectedItem = null;
        SelectedPlace = null;
        FromDate = null;
        ToDate = null;
        ShowOnlySales = false;
        SearchText = string.Empty;
        ShowChartSection = false;
        FilterPriceRecords();
    }

    private void ToggleChartView()
    {
        ShowChartSection = !ShowChartSection;
        if (ShowChartSection && SelectedChartItem != null)
        {
            UpdateChart();
        }
    }

    private void ViewItemChart(Item? item)
    {
        if (item == null) return;
        
        SelectedChartItem = item;
        ShowChartSection = true;
        UpdateChart();
    }

    private void UpdateChart()
    {
        if (SelectedChartItem == null) return;

        try
        {
            // Get price history for the selected item
            var history = _priceRecordRepository.GetPriceHistory(SelectedChartItem.Id, FromDate, ToDate)
                .OrderBy(r => r.DateRecorded)
                .ToList();

            if (!history.Any())
            {
                PriceHistorySeries = Array.Empty<ISeries>();
                return;
            }

            // Group by store
            var stores = history.Select(r => r.PlaceId).Distinct().ToList();
            var series = new List<ISeries>();
            var colors = new[] { 
                new SKColor(37, 99, 235),   // Blue
                new SKColor(220, 38, 38),   // Red
                new SKColor(22, 163, 74),   // Green
                new SKColor(234, 179, 8),   // Yellow
                new SKColor(147, 51, 234),  // Purple
                new SKColor(236, 72, 153)   // Pink
            };

            int colorIndex = 0;
            foreach (var storeId in stores)
            {
                var store = _placeRepository.GetById(storeId);
                var storeHistory = history.Where(r => r.PlaceId == storeId).ToList();
                
                var lineSeries = new LineSeries<decimal>
                {
                    Name = store?.Name ?? $"Store {storeId[..8]}",
                    Values = storeHistory.Select(r => r.Price).ToArray(),
                    Stroke = new SolidColorPaint(colors[colorIndex % colors.Length]) { StrokeThickness = 3 },
                    Fill = null,
                    GeometrySize = 8,
                    GeometryStroke = new SolidColorPaint(colors[colorIndex % colors.Length]) { StrokeThickness = 2 },
                    LineSmoothness = 0.5
                };
                
                series.Add(lineSeries);
                colorIndex++;
            }

            // Add comparison store if selected
            if (SelectedComparisonStore != null && 
                !stores.Contains(SelectedComparisonStore.Id) && 
                SelectedComparisonStore.Id != SelectedChartItem.Id)
            {
                var comparisonHistory = _priceRecordRepository.GetPriceHistory(SelectedChartItem.Id, FromDate, ToDate)
                    .Where(r => r.PlaceId == SelectedComparisonStore.Id)
                    .OrderBy(r => r.DateRecorded)
                    .ToList();

                if (comparisonHistory.Any())
                {
                    var comparisonSeries = new LineSeries<decimal>
                    {
                        Name = $"{SelectedComparisonStore.Name} (Compare)",
                        Values = comparisonHistory.Select(r => r.Price).ToArray(),
                        Stroke = new SolidColorPaint(new SKColor(100, 100, 100)) { StrokeThickness = 2 },
                        Fill = null,
                        GeometrySize = 6,
                        LineSmoothness = 0.5
                    };
                    series.Add(comparisonSeries);
                }
            }

            PriceHistorySeries = series.ToArray();

            // Get all unique dates for X-axis
            var allDates = history.Select(r => r.DateRecorded).Distinct().OrderBy(d => d).ToList();
            
            ChartXAxes = new[]
            {
                new Axis
                {
                    Name = "Date",
                    Labels = allDates.Select(d => d.ToString("dd/MM")).ToArray(),
                    LabelsRotation = 45,
                    TextSize = 12
                }
            };

            ChartYAxes = new[]
            {
                new Axis
                {
                    Name = "Price ($)",
                    TextSize = 12,
                    Labeler = value => $"${value:F2}"
                }
            };

            // Update statistics
            OnPropertyChanged(nameof(ChartTitle));
            OnPropertyChanged(nameof(ChartSubtitle));
            OnPropertyChanged(nameof(CurrentPriceText));
            OnPropertyChanged(nameof(LowestPriceText));
            OnPropertyChanged(nameof(HighestPriceText));
            OnPropertyChanged(nameof(AveragePriceText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to update chart: {ex.Message}");
        }
    }

    private decimal? GetCurrentPrice()
    {
        if (SelectedChartItem == null) return null;
        var latest = _priceRecordRepository.GetLatestPrice(SelectedChartItem.Id, null);
        return latest?.Price;
    }

    private decimal? GetLowestPrice()
    {
        if (SelectedChartItem == null) return null;
        return _priceRecordRepository.GetLowestPrice(SelectedChartItem.Id);
    }

    private decimal? GetHighestPrice()
    {
        if (SelectedChartItem == null) return null;
        return _priceRecordRepository.GetHighestPrice(SelectedChartItem.Id);
    }

    private decimal? GetAveragePrice()
    {
        if (SelectedChartItem == null) return null;
        return _priceRecordRepository.GetAveragePrice(SelectedChartItem.Id);
    }

    private void AddPriceRecord()
    {
        var viewModel = new AddPriceRecordViewModel(_itemRepository, _placeRepository, _priceRecordRepository);
        var window = new AddPriceRecordWindow(viewModel);
        
        if (window.ShowDialog() == true)
        {
            LoadPriceRecords();
            _dialogService.ShowSuccess("Price record added successfully.");
        }
    }

    private void EditPriceRecord(PriceRecordViewModel? priceRecord)
    {
        if (priceRecord == null) return;

        var viewModel = new AddPriceRecordViewModel(_itemRepository, _placeRepository, _priceRecordRepository, priceRecord.PriceRecord);
        var window = new AddPriceRecordWindow(viewModel);
        
        if (window.ShowDialog() == true)
        {
            LoadPriceRecords();
            _dialogService.ShowSuccess("Price record updated successfully.");
        }
    }

    private void DeletePriceRecord(PriceRecordViewModel? priceRecord)
    {
        if (priceRecord == null) return;

        var result = _dialogService.ShowConfirmation(
            "Delete Price Record",
            $"Are you sure you want to delete this price record for '{priceRecord.ItemName}' at '{priceRecord.PlaceName}'?");

        if (result)
        {
            try
            {
                _priceRecordRepository.Delete(priceRecord.Id);
                LoadPriceRecords();
                _dialogService.ShowSuccess("Price record deleted successfully.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to delete price record: {ex.Message}");
            }
        }
    }
}

public class PriceRecordViewModel : ViewModelBase
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;

    public PriceRecordViewModel(PriceRecord priceRecord, IItemRepository itemRepository, IPlaceRepository placeRepository)
    {
        PriceRecord = priceRecord;
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        
        // Load related data
        var item = itemRepository.GetById(priceRecord.ItemId);
        var place = placeRepository.GetById(priceRecord.PlaceId);
        
        ItemName = item?.Name ?? "Unknown Item";
        PlaceName = place?.Name ?? "Unknown Store";
    }

    public PriceRecord PriceRecord { get; }
    
    public string Id => PriceRecord.Id;
    public string ItemId => PriceRecord.ItemId;
    public string PlaceId => PriceRecord.PlaceId;
    public decimal Price => PriceRecord.Price;
    public decimal? OriginalPrice => PriceRecord.OriginalPrice;
    public bool IsOnSale => PriceRecord.IsOnSale;
    public string? SaleDescription => PriceRecord.SaleDescription;
    public DateTime DateRecorded => PriceRecord.DateRecorded;
    public DateTime? ValidTo => PriceRecord.ValidTo;
    public string? Source => PriceRecord.Source;

    public string ItemName { get; }
    public string PlaceName { get; }
    
    public string PriceDisplay => OriginalPrice.HasValue && OriginalPrice > Price
        ? $"${Price:F2} (was ${OriginalPrice:F2})"
        : $"${Price:F2}";
    
    public string StatusDisplay => IsOnSale ? "On Sale" : "Regular Price";
}
