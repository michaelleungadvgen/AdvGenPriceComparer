using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for managing user-defined price alerts
/// </summary>
public class PriceAlertViewModel : ViewModelBase
{
    private readonly IPriceAlertService _priceAlertService;
    private readonly IGroceryDataService _groceryData;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;

    public ObservableCollection<PriceAlertDisplayItem> Alerts { get; set; } = new();
    public ObservableCollection<Item> AllItems { get; set; } = new();
    public ObservableCollection<Place> AllPlaces { get; set; } = new();

    private PriceAlertDisplayItem? _selectedAlert;
    public PriceAlertDisplayItem? SelectedAlert
    {
        get => _selectedAlert;
        set
        {
            _selectedAlert = value;
            OnPropertyChanged();
        }
    }

    // New alert form properties
    private Item? _selectedItem;
    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            UpdateSuggestedTargetPrice();
        }
    }

    private Place? _selectedPlace;
    public Place? SelectedPlace
    {
        get => _selectedPlace;
        set
        {
            _selectedPlace = value;
            OnPropertyChanged();
        }
    }

    private decimal _targetPrice;
    public decimal TargetPrice
    {
        get => _targetPrice;
        set
        {
            _targetPrice = value;
            OnPropertyChanged();
        }
    }

    private PriceAlertCondition _selectedCondition = PriceAlertCondition.BelowOrEqual;
    public PriceAlertCondition SelectedCondition
    {
        get => _selectedCondition;
        set
        {
            _selectedCondition = value;
            OnPropertyChanged();
        }
    }

    private string _alertName = string.Empty;
    public string AlertName
    {
        get => _alertName;
        set
        {
            _alertName = value;
            OnPropertyChanged();
        }
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set
        {
            _notes = value;
            OnPropertyChanged();
        }
    }

    private bool _enableNotification = true;
    public bool EnableNotification
    {
        get => _enableNotification;
        set
        {
            _enableNotification = value;
            OnPropertyChanged();
        }
    }

    private int? _expiryDays;
    public int? ExpiryDays
    {
        get => _expiryDays;
        set
        {
            _expiryDays = value;
            OnPropertyChanged();
        }
    }

    // Stats properties
    private int _activeAlertCount;
    public int ActiveAlertCount
    {
        get => _activeAlertCount;
        set
        {
            _activeAlertCount = value;
            OnPropertyChanged();
        }
    }

    private int _triggeredAlertCount;
    public int TriggeredAlertCount
    {
        get => _triggeredAlertCount;
        set
        {
            _triggeredAlertCount = value;
            OnPropertyChanged();
        }
    }

    // Filter
    private string _filterStatus = "All";
    public string FilterStatus
    {
        get => _filterStatus;
        set
        {
            _filterStatus = value;
            OnPropertyChanged();
            LoadAlerts();
        }
    }

    public List<string> FilterOptions => new() { "All", "Active", "Triggered", "Expired", "Disabled" };
    public List<PriceAlertCondition> ConditionOptions => Enum.GetValues<PriceAlertCondition>().ToList();

    // Commands
    public ICommand RefreshCommand { get; }
    public ICommand CreateAlertCommand { get; }
    public ICommand DeleteAlertCommand { get; }
    public ICommand ReactivateAlertCommand { get; }
    public ICommand DisableAlertCommand { get; }
    public ICommand AcknowledgeAlertCommand { get; }
    public ICommand ClearFormCommand { get; }
    public ICommand CloseCommand { get; }

    public event EventHandler? RequestClose;

    public PriceAlertViewModel(
        IPriceAlertService priceAlertService,
        IGroceryDataService groceryData,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _priceAlertService = priceAlertService ?? throw new ArgumentNullException(nameof(priceAlertService));
        _groceryData = groceryData ?? throw new ArgumentNullException(nameof(groceryData));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        RefreshCommand = new RelayCommand(LoadAlerts);
        CreateAlertCommand = new RelayCommand(async () => await CreateAlertAsync(), CanCreateAlert);
        DeleteAlertCommand = new RelayCommand<PriceAlertDisplayItem>(async (alert) => await DeleteAlertAsync(alert));
        ReactivateAlertCommand = new RelayCommand<PriceAlertDisplayItem>(async (alert) => await ReactivateAlertAsync(alert));
        DisableAlertCommand = new RelayCommand<PriceAlertDisplayItem>(async (alert) => await DisableAlertAsync(alert));
        AcknowledgeAlertCommand = new RelayCommand<PriceAlertDisplayItem>(async (alert) => await AcknowledgeAlertAsync(alert));
        ClearFormCommand = new RelayCommand(ClearForm);
        CloseCommand = new RelayCommand(() => RequestClose?.Invoke(this, EventArgs.Empty));

        // Subscribe to alert triggered events
        _priceAlertService.AlertTriggered += OnAlertTriggered;

        LoadAlerts();
        LoadItems();
        LoadPlaces();
        UpdateStats();
    }

    private void LoadAlerts()
    {
        Alerts.Clear();

        var alerts = _priceAlertService.GetAllAlertsAsync().Result;

        // Apply filter
        alerts = FilterStatus switch
        {
            "Active" => alerts.Where(a => a.Status == PriceAlertStatus.Active),
            "Triggered" => alerts.Where(a => a.Status == PriceAlertStatus.Triggered),
            "Expired" => alerts.Where(a => a.Status == PriceAlertStatus.Expired),
            "Disabled" => alerts.Where(a => a.Status == PriceAlertStatus.Disabled),
            _ => alerts
        };

        foreach (var alert in alerts.OrderByDescending(a => a.DateCreated))
        {
            var displayItem = new PriceAlertDisplayItem
            {
                Alert = alert,
                StatusText = GetStatusText(alert.Status),
                StatusColor = GetStatusColor(alert.Status),
                ConditionText = GetConditionText(alert.Condition),
                DisplayMessage = alert.GetDisplayMessage()
            };
            Alerts.Add(displayItem);
        }
    }

    private void LoadItems()
    {
        AllItems.Clear();
        var items = _groceryData.Items.GetAll();
        foreach (var item in items.OrderBy(i => i.Name))
        {
            AllItems.Add(item);
        }
    }

    private void LoadPlaces()
    {
        AllPlaces.Clear();
        AllPlaces.Add(new Place { Id = string.Empty, Name = "All Stores" });
        var places = _groceryData.Places.GetAll();
        foreach (var place in places.OrderBy(p => p.Name))
        {
            AllPlaces.Add(place);
        }
    }

    private async void UpdateStats()
    {
        ActiveAlertCount = await _priceAlertService.GetActiveAlertCountAsync();
        TriggeredAlertCount = await _priceAlertService.GetTriggeredAlertCountAsync();
    }

    private async void UpdateSuggestedTargetPrice()
    {
        if (SelectedItem == null) return;

        // Get current price for the item
        var prices = _groceryData.PriceRecords.GetByItem(SelectedItem.Id);
        var latestPrice = prices.OrderByDescending(p => p.DateRecorded).FirstOrDefault();

        if (latestPrice != null)
        {
            // Suggest a target price 10% below current price
            TargetPrice = Math.Round(latestPrice.Price * 0.9m, 2);
        }
    }

    private async Task CreateAlertAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            DateTime? expiryDate = ExpiryDays.HasValue 
                ? DateTime.UtcNow.AddDays(ExpiryDays.Value) 
                : null;

            var placeId = SelectedPlace?.Id;
            if (placeId == string.Empty) placeId = null;

            var alert = await _priceAlertService.CreateAlertAsync(
                SelectedItem.Id,
                TargetPrice,
                SelectedCondition,
                placeId,
                string.IsNullOrEmpty(AlertName) ? null : AlertName,
                expiryDate);

            // Store additional properties
            alert.Notes = string.IsNullOrEmpty(Notes) ? null : Notes;
            alert.EnableNotification = EnableNotification;
            await _priceAlertService.UpdateAlertAsync(alert);

            _dialogService.ShowSuccess("Price alert created successfully!");
            ClearForm();
            LoadAlerts();
            UpdateStats();
        }
        catch (Exception ex)
        {
            _logger.LogError("Error creating price alert", ex);
            _dialogService.ShowError($"Failed to create alert: {ex.Message}");
        }
    }

    private bool CanCreateAlert()
    {
        return SelectedItem != null && TargetPrice > 0;
    }

    private async Task DeleteAlertAsync(PriceAlertDisplayItem? displayItem)
    {
        if (displayItem == null) return;

        var result = _dialogService.ShowConfirmation(
            "Are you sure you want to delete this price alert?",
            "Confirm Delete");

        if (result)
        {
            try
            {
                await _priceAlertService.DeleteAlertAsync(displayItem.Alert.Id);
                Alerts.Remove(displayItem);
                UpdateStats();
                _dialogService.ShowSuccess("Price alert deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting alert {displayItem.Alert.Id}", ex);
                _dialogService.ShowError($"Failed to delete alert: {ex.Message}");
            }
        }
    }

    private async Task ReactivateAlertAsync(PriceAlertDisplayItem? displayItem)
    {
        if (displayItem == null) return;

        try
        {
            await _priceAlertService.ReactivateAlertAsync(displayItem.Alert.Id);
            LoadAlerts();
            UpdateStats();
            _dialogService.ShowSuccess("Price alert reactivated.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error reactivating alert {displayItem.Alert.Id}", ex);
            _dialogService.ShowError($"Failed to reactivate alert: {ex.Message}");
        }
    }

    private async Task DisableAlertAsync(PriceAlertDisplayItem? displayItem)
    {
        if (displayItem == null) return;

        try
        {
            await _priceAlertService.DisableAlertAsync(displayItem.Alert.Id);
            LoadAlerts();
            UpdateStats();
            _dialogService.ShowSuccess("Price alert disabled.");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error disabling alert {displayItem.Alert.Id}", ex);
            _dialogService.ShowError($"Failed to disable alert: {ex.Message}");
        }
    }

    private async Task AcknowledgeAlertAsync(PriceAlertDisplayItem? displayItem)
    {
        if (displayItem == null) return;

        try
        {
            await _priceAlertService.AcknowledgeAlertAsync(displayItem.Alert.Id);
            LoadAlerts();
            UpdateStats();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error acknowledging alert {displayItem.Alert.Id}", ex);
            _dialogService.ShowError($"Failed to acknowledge alert: {ex.Message}");
        }
    }

    private void ClearForm()
    {
        SelectedItem = null;
        SelectedPlace = null;
        TargetPrice = 0;
        SelectedCondition = PriceAlertCondition.BelowOrEqual;
        AlertName = string.Empty;
        Notes = string.Empty;
        EnableNotification = true;
        ExpiryDays = null;
    }

    private void OnAlertTriggered(object? sender, PriceAlertTriggeredEventArgs e)
    {
        // Refresh alerts when triggered
        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            LoadAlerts();
            UpdateStats();
        });
    }

    private static string GetStatusText(PriceAlertStatus status)
    {
        return status switch
        {
            PriceAlertStatus.Active => "Active",
            PriceAlertStatus.Triggered => "Triggered!",
            PriceAlertStatus.Expired => "Expired",
            PriceAlertStatus.Disabled => "Disabled",
            _ => status.ToString()
        };
    }

    private static string GetStatusColor(PriceAlertStatus status)
    {
        return status switch
        {
            PriceAlertStatus.Active => "#4CAF50",      // Green
            PriceAlertStatus.Triggered => "#FF5722",   // Orange/Red
            PriceAlertStatus.Expired => "#9E9E9E",     // Gray
            PriceAlertStatus.Disabled => "#757575",    // Dark Gray
            _ => "#000000"
        };
    }

    private static string GetConditionText(PriceAlertCondition condition)
    {
        return condition switch
        {
            PriceAlertCondition.BelowOrEqual => "≤",
            PriceAlertCondition.Below => "<",
            PriceAlertCondition.Equal => "=",
            PriceAlertCondition.Above => ">",
            PriceAlertCondition.AboveOrEqual => "≥",
            _ => condition.ToString()
        };
    }
}

/// <summary>
/// Display item for price alerts with UI-specific properties
/// </summary>
public class PriceAlertDisplayItem
{
    public required PriceAlert Alert { get; set; }
    public required string StatusText { get; set; }
    public required string StatusColor { get; set; }
    public required string ConditionText { get; set; }
    public required string DisplayMessage { get; set; }
}
