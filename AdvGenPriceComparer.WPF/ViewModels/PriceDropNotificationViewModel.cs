using System.Collections.ObjectModel;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for managing price drop notifications
/// </summary>
public class PriceDropNotificationViewModel : ViewModelBase
{
    private readonly IPriceDropNotificationService _notificationService;
    private readonly IGroceryDataService _groceryData;

    public ObservableCollection<AlertLogicEntity> Notifications { get; set; } = new();
    public ObservableCollection<Item> WatchedItems { get; set; } = new();

    private AlertLogicEntity? _selectedNotification;
    public AlertLogicEntity? SelectedNotification
    {
        get => _selectedNotification;
        set
        {
            _selectedNotification = value;
            OnPropertyChanged();
        }
    }

    private int _unreadCount;
    public int UnreadCount
    {
        get => _unreadCount;
        set
        {
            _unreadCount = value;
            OnPropertyChanged();
        }
    }

    private bool _isMonitoring;
    public bool IsMonitoring
    {
        get => _isMonitoring;
        set
        {
            _isMonitoring = value;
            OnPropertyChanged();
        }
    }

    private string _newAlertItemId = string.Empty;
    public string NewAlertItemId
    {
        get => _newAlertItemId;
        set
        {
            _newAlertItemId = value;
            OnPropertyChanged();
        }
    }

    private decimal? _newAlertThresholdPercentage;
    public decimal? NewAlertThresholdPercentage
    {
        get => _newAlertThresholdPercentage;
        set
        {
            _newAlertThresholdPercentage = value;
            OnPropertyChanged();
        }
    }

    private decimal? _newAlertThresholdPrice;
    public decimal? NewAlertThresholdPrice
    {
        get => _newAlertThresholdPrice;
        set
        {
            _newAlertThresholdPrice = value;
            OnPropertyChanged();
        }
    }

    private string _newAlertName = string.Empty;
    public string NewAlertName
    {
        get => _newAlertName;
        set
        {
            _newAlertName = value;
            OnPropertyChanged();
        }
    }

    public ICommand RefreshCommand { get; }
    public ICommand MarkAsReadCommand { get; }
    public ICommand DismissCommand { get; }
    public ICommand CreateAlertCommand { get; }
    public ICommand ToggleMonitoringCommand { get; }
    public ICommand ClearAllCommand { get; }

    public PriceDropNotificationViewModel(
        IPriceDropNotificationService notificationService,
        IGroceryDataService groceryData)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        _groceryData = groceryData ?? throw new ArgumentNullException(nameof(groceryData));

        RefreshCommand = new RelayCommand(LoadNotifications);
        MarkAsReadCommand = new RelayCommand<AlertLogicEntity>(async (alert) => await MarkAsReadAsync(alert));
        DismissCommand = new RelayCommand<AlertLogicEntity>(async (alert) => await DismissAsync(alert));
        CreateAlertCommand = new RelayCommand(async () => await CreateAlertAsync(), CanCreateAlert);
        ToggleMonitoringCommand = new RelayCommand(ToggleMonitoring);
        ClearAllCommand = new RelayCommand(async () => await ClearAllAsync());

        // Subscribe to price drop events
        _notificationService.PriceDropDetected += OnPriceDropDetected;

        LoadNotifications();
        LoadWatchedItems();
        IsMonitoring = _notificationService.IsMonitoring;
    }

    private void LoadNotifications()
    {
        Notifications.Clear();
        var notifications = _notificationService.GetTriggeredNotifications();
        foreach (var notification in notifications)
        {
            Notifications.Add(notification);
        }
        UnreadCount = _notificationService.GetUnreadNotificationCount();
    }

    private void LoadWatchedItems()
    {
        WatchedItems.Clear();
        var activeAlerts = _groceryData.Alerts.GetActiveAlerts()
            .Where(a => a.Type == AlertType.PriceDecrease || a.Type == AlertType.PriceChange)
            .ToList();

        foreach (var alert in activeAlerts)
        {
            var item = _groceryData.Items.GetById(alert.ItemId);
            if (item != null && !WatchedItems.Any(i => i.Id == item.Id))
            {
                WatchedItems.Add(item);
            }
        }
    }

    private async Task MarkAsReadAsync(AlertLogicEntity? alert)
    {
        if (alert == null) return;

        await _notificationService.MarkAsReadAsync(alert.Id);
        alert.MarkAsRead();
        LoadNotifications();
    }

    private async Task DismissAsync(AlertLogicEntity? alert)
    {
        if (alert == null) return;

        await _notificationService.DismissNotificationAsync(alert.Id);
        Notifications.Remove(alert);
        UnreadCount = _notificationService.GetUnreadNotificationCount();
    }

    private async Task CreateAlertAsync()
    {
        if (string.IsNullOrEmpty(NewAlertItemId)) return;

        await _notificationService.CreatePriceDropAlertAsync(
            NewAlertItemId,
            NewAlertThresholdPercentage,
            NewAlertThresholdPrice,
            string.IsNullOrEmpty(NewAlertName) ? null : NewAlertName);

        // Reset form
        NewAlertItemId = string.Empty;
        NewAlertThresholdPercentage = null;
        NewAlertThresholdPrice = null;
        NewAlertName = string.Empty;

        LoadWatchedItems();
        LoadNotifications();
    }

    private bool CanCreateAlert()
    {
        return !string.IsNullOrEmpty(NewAlertItemId);
    }

    private void ToggleMonitoring()
    {
        if (_notificationService.IsMonitoring)
        {
            _notificationService.StopMonitoring();
        }
        else
        {
            _notificationService.StartMonitoring();
        }
        IsMonitoring = _notificationService.IsMonitoring;
    }

    private async Task ClearAllAsync()
    {
        var notificationsToClear = Notifications.ToList();
        foreach (var notification in notificationsToClear)
        {
            await _notificationService.DismissNotificationAsync(notification.Id);
        }
        LoadNotifications();
    }

    private void OnPriceDropDetected(object? sender, PriceDropEventArgs e)
    {
        // Refresh notifications when a price drop is detected
        System.Windows.Application.Current.Dispatcher.Invoke(LoadNotifications);
    }
}
