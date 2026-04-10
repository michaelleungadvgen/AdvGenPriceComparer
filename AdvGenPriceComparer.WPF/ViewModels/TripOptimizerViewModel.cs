using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using AdvGenFlow;
using AdvGenPriceComparer.Application.Queries;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for the trip optimizer window
/// </summary>
public class TripOptimizerViewModel : ViewModelBase
{
    private readonly ITripOptimizerService _tripOptimizerService;
    private readonly IMediator _mediator;
    private readonly IShoppingListService _shoppingListService;
    private readonly ILoggerService _logger;
    private readonly IDialogService _dialogService;

    // Collections
    public ObservableCollection<ShoppingList> AvailableShoppingLists { get; set; } = new();
    public ObservableCollection<Place> AvailableStores { get; set; } = new();
    public ObservableCollection<StoreStopViewModel> StoreStops { get; set; } = new();
    public ObservableCollection<ShoppingListItemViewModel> UnavailableItems { get; set; } = new();

    // Selected items
    private ShoppingList? _selectedShoppingList;
    public ShoppingList? SelectedShoppingList
    {
        get => _selectedShoppingList;
        set
        {
            if (SetProperty(ref _selectedShoppingList, value))
            {
                OnPropertyChanged(nameof(CanOptimize));
                OnPropertyChanged(nameof(SelectedListItemsCount));
                OnPropertyChanged(nameof(SelectedListTotal));
            }
        }
    }

    // Optimization options
    private OptimizationStrategy _selectedStrategy = OptimizationStrategy.Balanced;
    public OptimizationStrategy SelectedStrategy
    {
        get => _selectedStrategy;
        set
        {
            if (SetProperty(ref _selectedStrategy, value))
            {
                OnPropertyChanged(nameof(StrategyDescription));
            }
        }
    }

    private int _maxStores = 5;
    public int MaxStores
    {
        get => _maxStores;
        set => SetProperty(ref _maxStores, value);
    }

    private double _maxDistanceKm = 50;
    public double MaxDistanceKm
    {
        get => _maxDistanceKm;
        set => SetProperty(ref _maxDistanceKm, value);
    }

    private decimal _minSavingsThreshold = 5.00m;
    public decimal MinSavingsThreshold
    {
        get => _minSavingsThreshold;
        set => SetProperty(ref _minSavingsThreshold, value);
    }

    private bool _prioritizeOneStop = true;
    public bool PrioritizeOneStop
    {
        get => _prioritizeOneStop;
        set => SetProperty(ref _prioritizeOneStop, value);
    }

    // Results
    private bool _hasResults;
    public bool HasResults
    {
        get => _hasResults;
        set => SetProperty(ref _hasResults, value);
    }

    private bool _isOptimizing;
    public bool IsOptimizing
    {
        get => _isOptimizing;
        set
        {
            if (SetProperty(ref _isOptimizing, value))
            {
                OnPropertyChanged(nameof(CanOptimize));
            }
        }
    }

    private string _statusMessage = "";
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private double _totalDistanceKm;
    public double TotalDistanceKm
    {
        get => _totalDistanceKm;
        set => SetProperty(ref _totalDistanceKm, value);
    }

    private double _totalTravelTime;
    public double TotalTravelTime
    {
        get => _totalTravelTime;
        set => SetProperty(ref _totalTravelTime, value);
    }

    private decimal _totalCost;
    public decimal TotalCost
    {
        get => _totalCost;
        set => SetProperty(ref _totalCost, value);
    }

    private decimal _potentialSavings;
    public decimal PotentialSavings
    {
        get => _potentialSavings;
        set => SetProperty(ref _potentialSavings, value);
    }

    private int _numberOfStores;
    public int NumberOfStores
    {
        get => _numberOfStores;
        set => SetProperty(ref _numberOfStores, value);
    }

    private int _totalItems;
    public int TotalItems
    {
        get => _totalItems;
        set => SetProperty(ref _totalItems, value);
    }

    private int _unavailableItemsCount;
    public int UnavailableItemsCount
    {
        get => _unavailableItemsCount;
        set => SetProperty(ref _unavailableItemsCount, value);
    }

    // Computed properties
    public bool CanOptimize => SelectedShoppingList != null && !IsOptimizing;

    public int SelectedListItemsCount => SelectedShoppingList?.Items.Count(i => !i.IsChecked) ?? 0;

    public decimal? SelectedListTotal => SelectedShoppingList?.EstimatedTotal;

    public string StrategyDescription => SelectedStrategy switch
    {
        OptimizationStrategy.Cost => "Prioritize lowest prices, may visit more stores",
        OptimizationStrategy.Distance => "Minimize driving, shop at fewer stores",
        OptimizationStrategy.Balanced => "Balance cost savings with travel time",
        _ => ""
    };

    public string FormattedTotalTime
    {
        get
        {
            var hours = (int)(TotalTravelTime / 60);
            var minutes = (int)(TotalTravelTime % 60);
            if (hours > 0)
                return $"{hours}h {minutes}m";
            return $"{minutes}m";
        }
    }

    // Commands
    public ICommand OptimizeCommand { get; }
    public ICommand RefreshListsCommand { get; }
    public ICommand ExportResultsCommand { get; }
    public ICommand PrintResultsCommand { get; }

    public event EventHandler? RequestClose;

    public TripOptimizerViewModel(
        ITripOptimizerService tripOptimizerService,
        IMediator mediator,
        IShoppingListService shoppingListService,
        ILoggerService logger,
        IDialogService dialogService)
    {
        _tripOptimizerService = tripOptimizerService;
        _mediator = mediator;
        _shoppingListService = shoppingListService;
        _logger = logger;
        _dialogService = dialogService;

        OptimizeCommand = new RelayCommand(async () => await OptimizeAsync(), () => CanOptimize);
        RefreshListsCommand = new RelayCommand(async () => await LoadDataAsync());
        ExportResultsCommand = new RelayCommand(async () => await ExportResultsAsync(), () => HasResults);
        PrintResultsCommand = new RelayCommand(() => PrintResults(), () => HasResults);

        _ = LoadDataAsync();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            StatusMessage = "Loading shopping lists and stores...";

            // Load shopping lists
            var lists = _shoppingListService.GetAllShoppingLists();
            AvailableShoppingLists.Clear();
            foreach (var list in lists.Where(l => l.IsActive))
            {
                AvailableShoppingLists.Add(list);
            }

            // Load stores
            var stores = _mediator.Send(new GetAllPlacesQuery()).GetAwaiter().GetResult();
            AvailableStores.Clear();
            foreach (var store in stores.Where(s => s.IsActive))
            {
                AvailableStores.Add(store);
            }

            StatusMessage = $"Loaded {AvailableShoppingLists.Count} shopping lists and {AvailableStores.Count} stores";
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading data for trip optimizer", ex);
            StatusMessage = "Error loading data. Please try again.";
        }
    }

    private async Task OptimizeAsync()
    {
        if (SelectedShoppingList == null)
            return;

        try
        {
            IsOptimizing = true;
            HasResults = false;
            StatusMessage = "Optimizing your shopping trip...";

            // Clear previous results
            StoreStops.Clear();
            UnavailableItems.Clear();

            // Create optimization options
            var options = new TripOptimizationOptions
            {
                MaxStores = MaxStores,
                MaxTravelDistanceKm = MaxDistanceKm,
                Strategy = SelectedStrategy,
                MinSavingsThreshold = MinSavingsThreshold,
                PrioritizeOneStopShopping = PrioritizeOneStop
            };

            // Run optimization
            var result = await _tripOptimizerService.OptimizeTripAsync(SelectedShoppingList, options);

            if (!result.Success)
            {
                StatusMessage = result.Message;
                _dialogService.ShowError(result.Message, "Optimization Failed");
                return;
            }

            // Update results
            TotalDistanceKm = result.TotalDistanceKm;
            TotalTravelTime = result.TotalTravelTimeMinutes;
            TotalCost = result.TotalCost;
            PotentialSavings = result.PotentialSavings;
            NumberOfStores = result.NumberOfStores;
            TotalItems = result.TotalItems;
            UnavailableItemsCount = result.UnavailableItems.Count;

            // Populate store stops
            foreach (var stop in result.StoreStops)
            {
                StoreStops.Add(new StoreStopViewModel(stop));
            }

            // Populate unavailable items
            foreach (var item in result.UnavailableItems)
            {
                UnavailableItems.Add(new ShoppingListItemViewModel(item));
            }

            HasResults = true;
            StatusMessage = $"Trip optimized! Visit {NumberOfStores} stores, save up to ${PotentialSavings:F2}";

            OnPropertyChanged(nameof(FormattedTotalTime));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error during trip optimization", ex);
            StatusMessage = "Optimization failed. Please try again.";
            _dialogService.ShowError($"Optimization failed: {ex.Message}", "Error");
        }
        finally
        {
            IsOptimizing = false;
        }
    }

    private async Task ExportResultsAsync()
    {
        if (!HasResults)
            return;

        try
        {
            // Generate markdown report
            var report = GenerateMarkdownReport();

            // Copy to clipboard
            System.Windows.Clipboard.SetText(report);

            _dialogService.ShowInfo("Trip plan copied to clipboard!", "Export Complete");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error exporting trip results", ex);
            _dialogService.ShowError("Failed to export results", "Error");
        }
    }

    private void PrintResults()
    {
        // In a real implementation, this would open a print dialog
        // For now, just show a message
        _dialogService.ShowInfo("Print functionality would open the system print dialog.", "Print");
    }

    private string GenerateMarkdownReport()
    {
        var sb = new System.Text.StringBuilder();

        sb.AppendLine($"# Shopping Trip Plan: {SelectedShoppingList?.Name}");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
        sb.AppendLine();

        sb.AppendLine("## Trip Summary");
        sb.AppendLine($"- **Number of Stores:** {NumberOfStores}");
        sb.AppendLine($"- **Total Items:** {TotalItems}");
        sb.AppendLine($"- **Total Distance:** {TotalDistanceKm:F1} km");
        sb.AppendLine($"- **Estimated Time:** {FormattedTotalTime}");
        sb.AppendLine($"- **Total Cost:** ${TotalCost:F2}");
        if (PotentialSavings > 0)
            sb.AppendLine($"- **Potential Savings:** ${PotentialSavings:F2}");
        sb.AppendLine();

        sb.AppendLine("## Store Stops");
        sb.AppendLine();

        int stopNum = 1;
        foreach (var stop in StoreStops)
        {
            sb.AppendLine($"### Stop {stopNum++}: {stop.StoreName}");
            sb.AppendLine($"**Address:** {stop.Address}");
            if (stop.TravelTimeFromPrevious > 0)
                sb.AppendLine($"**Drive from previous:** {stop.TravelTimeFromPrevious:F0} min ({stop.DistanceFromPreviousKm:F1} km)");
            sb.AppendLine();

            sb.AppendLine("| Item | Brand | Qty | Price | Notes |");
            sb.AppendLine("|------|-------|-----|-------|-------|");

            foreach (var item in stop.Items)
            {
                var notes = item.IsOnSale ? "🎉 On Sale" : "";
                if (item.Savings.HasValue && item.Savings.Value > 0)
                    notes += $" Save ${item.Savings.Value:F2}";

                sb.AppendLine($"| {item.Name} | {item.Brand ?? "-"} | {item.QuantityDisplay} | ${item.FinalPrice:F2} | {notes} |");
            }

            sb.AppendLine();
            sb.AppendLine($"**Subtotal:** ${stop.Subtotal:F2}");
            sb.AppendLine();
        }

        if (UnavailableItemsCount > 0)
        {
            sb.AppendLine("## Unavailable Items");
            sb.AppendLine("The following items could not be found at any store:");
            sb.AppendLine();

            foreach (var item in UnavailableItems)
            {
                sb.AppendLine($"- {item.DisplayName}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("---");
        sb.AppendLine("*Generated by AdvGen Price Comparer*");

        return sb.ToString();
    }
}

/// <summary>
/// ViewModel for a store stop
/// </summary>
public class StoreStopViewModel : ViewModelBase
{
    public Place Store { get; }
    public ObservableCollection<TripItemViewModel> Items { get; } = new();

    public string StoreName => Store.Name;
    public string Chain => Store.Chain ?? "Independent";
    public string Address => $"{Store.Address}, {Store.Suburb} {Store.State} {Store.Postcode}".Trim(',', ' ');
    public int StopNumber { get; set; }
    public double TravelTimeFromPrevious { get; set; }
    public double DistanceFromPreviousKm { get; set; }
    public decimal Subtotal { get; set; }
    public int ItemCount => Items.Count;

    public string TravelInfo => TravelTimeFromPrevious > 0
        ? $"🚗 {TravelTimeFromPrevious:F0} min ({DistanceFromPreviousKm:F1} km)"
        : "📍 Starting point";

    public StoreStopViewModel(StoreStop stop)
    {
        Store = stop.Store;
        StopNumber = stop.StopNumber;
        TravelTimeFromPrevious = stop.TravelTimeFromPrevious;
        DistanceFromPreviousKm = stop.DistanceFromPreviousKm;
        Subtotal = stop.Subtotal;

        foreach (var item in stop.Items)
        {
            Items.Add(new TripItemViewModel(item));
        }
    }
}

/// <summary>
/// ViewModel for a trip item
/// </summary>
public class TripItemViewModel : ViewModelBase
{
    private readonly TripItem _item;

    public string Name => _item.ShoppingListItem.Name;
    public string? Brand => _item.ShoppingListItem.Brand;
    public string DisplayName => _item.ShoppingListItem.DisplayName;
    public string? QuantityDisplay => _item.ShoppingListItem.QuantityDisplay;
    public decimal FinalPrice => _item.FinalPrice;
    public decimal? Savings => _item.Savings;
    public bool IsOnSale => _item.IsOnSale;
    public string? Category => _item.ShoppingListItem.Category;

    public string PriceDisplay => _item.ShoppingListItem.Quantity.HasValue && _item.ShoppingListItem.Quantity.Value > 1
        ? $"${_item.FinalPrice:F2} each = ${_item.FinalPrice * _item.ShoppingListItem.Quantity.Value:F2}"
        : $"${_item.FinalPrice:F2}";

    public string? SavingsDisplay => Savings.HasValue && Savings.Value > 0
        ? $"Save ${Savings.Value:F2}"
        : null;

    public bool HasSavings => Savings.HasValue && Savings.Value > 0;

    public TripItemViewModel(TripItem item)
    {
        _item = item;
    }
}

/// <summary>
/// ViewModel for shopping list item (simplified)
/// </summary>
public class ShoppingListItemViewModel : ViewModelBase
{
    private readonly ShoppingListItem _item;

    public string Name => _item.Name;
    public string? Brand => _item.Brand;
    public string DisplayName => _item.DisplayName;
    public string? QuantityDisplay => _item.QuantityDisplay;
    public string? Category => _item.Category;

    public ShoppingListItemViewModel(ShoppingListItem item)
    {
        _item = item;
    }
}
