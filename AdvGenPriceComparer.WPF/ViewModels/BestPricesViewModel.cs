using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for the Best Prices window
/// </summary>
public class BestPricesViewModel : ViewModelBase
{
    private readonly IBestPriceService _bestPriceService;
    private readonly ILoggerService _logger;
    private BestPriceInfo? _selectedDeal;
    private int _selectedTabIndex;

    public ObservableCollection<BestPriceInfo> BestDeals { get; } = new();
    public ObservableCollection<BestPriceInfo> HistoricalLows { get; } = new();
    public ObservableCollection<BestPriceInfo> BestSavings { get; } = new();
    public ObservableCollection<PriceComparisonResult> PriceComparisons { get; } = new();

    public BestPriceInfo? SelectedDeal
    {
        get => _selectedDeal;
        set
        {
            _selectedDeal = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanViewDetails));
        }
    }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            _selectedTabIndex = value;
            OnPropertyChanged();
        }
    }

    public bool CanViewDetails => SelectedDeal != null;

    public ICommand RefreshCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler? RequestClose;
    public event EventHandler<BestPriceInfo>? RequestViewDetails;

    public BestPricesViewModel(IBestPriceService bestPriceService, ILoggerService logger)
    {
        _bestPriceService = bestPriceService;
        _logger = logger;

        RefreshCommand = new RelayCommand(LoadData);
        ViewDetailsCommand = new RelayCommand(ViewDetails, () => CanViewDetails);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));

        LoadData();
    }

    private void LoadData()
    {
        try
        {
            _logger.LogInfo("Loading best prices data...");

            // Load best deals
            BestDeals.Clear();
            foreach (var deal in _bestPriceService.GetAllBestDeals(30))
            {
                BestDeals.Add(deal);
            }

            // Load historical lows
            HistoricalLows.Clear();
            foreach (var deal in BestDeals.Where(d => d.IsHistoricalLow).Take(20))
            {
                HistoricalLows.Add(deal);
            }

            // Load best savings (>20% off)
            BestSavings.Clear();
            foreach (var deal in _bestPriceService.GetBestSavings(20))
            {
                BestSavings.Add(deal);
            }

            _logger.LogInfo($"Loaded {BestDeals.Count} best deals, {HistoricalLows.Count} historical lows, {BestSavings.Count} best savings");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error loading best prices data", ex);
        }
    }

    private void ViewDetails()
    {
        if (SelectedDeal != null)
        {
            RequestViewDetails?.Invoke(this, SelectedDeal);
        }
    }

    /// <summary>
    /// Get highlight color based on level
    /// </summary>
    public static string GetHighlightColor(PriceHighlightLevel level)
    {
        return level switch
        {
            PriceHighlightLevel.BestPrice => "#FFD700",  // Gold
            PriceHighlightLevel.GreatDeal => "#4CAF50",  // Green
            PriceHighlightLevel.GoodDeal => "#2196F3",   // Blue
            _ => "Transparent"
        };
    }

    /// <summary>
    /// Get highlight text based on level
    /// </summary>
    public static string GetHighlightText(PriceHighlightLevel level)
    {
        return level switch
        {
            PriceHighlightLevel.BestPrice => "🔥 BEST PRICE",
            PriceHighlightLevel.GreatDeal => "⭐ GREAT DEAL",
            PriceHighlightLevel.GoodDeal => "✓ GOOD DEAL",
            _ => string.Empty
        };
    }
}
