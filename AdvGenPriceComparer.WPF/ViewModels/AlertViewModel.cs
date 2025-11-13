using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class AlertViewModel : ViewModelBase
{
    private readonly IAlertRepository _alertRepository;
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private ObservableCollection<AlertDisplayItem> _alerts;
    private AlertDisplayItem? _selectedAlert;
    private string _filterType = "All";
    private List<AlertDisplayItem> _allAlerts = new();

    public AlertViewModel(IAlertRepository alertRepository, IGroceryDataService dataService, IDialogService dialogService)
    {
        _alertRepository = alertRepository;
        _dataService = dataService;
        _dialogService = dialogService;
        _alerts = new ObservableCollection<AlertDisplayItem>();

        AddAlertCommand = new RelayCommand(AddAlert);
        ViewAlertCommand = new RelayCommand<AlertDisplayItem>(ViewAlert);
        MarkAsReadCommand = new RelayCommand<AlertDisplayItem>(MarkAsRead);
        DismissAlertCommand = new RelayCommand<AlertDisplayItem>(DismissAlert);
        MarkAllAsReadCommand = new RelayCommand(MarkAllAsRead);
        DismissAllCommand = new RelayCommand(DismissAll);
        RefreshCommand = new RelayCommand(LoadAlerts);

        LoadAlerts();
    }

    public ObservableCollection<AlertDisplayItem> Alerts
    {
        get => _alerts;
        set => SetProperty(ref _alerts, value);
    }

    public AlertDisplayItem? SelectedAlert
    {
        get => _selectedAlert;
        set
        {
            if (SetProperty(ref _selectedAlert, value))
            {
                CommandManager.InvalidateRequerySuggested();
            }
        }
    }

    public string FilterType
    {
        get => _filterType;
        set
        {
            if (SetProperty(ref _filterType, value))
            {
                FilterAlerts();
            }
        }
    }

    public string AlertCountText => $"{Alerts.Count} alerts";
    public string UnreadCountText => $"{_alertRepository.GetUnreadCount()} unread";

    public ICommand AddAlertCommand { get; }
    public ICommand ViewAlertCommand { get; }
    public ICommand MarkAsReadCommand { get; }
    public ICommand DismissAlertCommand { get; }
    public ICommand MarkAllAsReadCommand { get; }
    public ICommand DismissAllCommand { get; }
    public ICommand RefreshCommand { get; }

    private void LoadAlerts()
    {
        try
        {
            var alerts = _alertRepository.GetAll().ToList();
            _allAlerts.Clear();

            foreach (var alert in alerts)
            {
                var item = _dataService.GetItemById(alert.ItemId);
                var place = alert.PlaceId != null ? _dataService.GetPlaceById(alert.PlaceId) : null;

                var displayItem = new AlertDisplayItem
                {
                    Alert = alert,
                    ItemName = item?.Name ?? "Unknown Item",
                    PlaceName = place?.Name ?? "All Stores",
                    DisplayMessage = alert.Message ?? alert.GenerateMessage(
                        item?.Name ?? "Unknown Item",
                        place?.Name)
                };

                _allAlerts.Add(displayItem);
            }

            FilterAlerts();
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to load alerts: {ex.Message}");
        }
    }

    private void FilterAlerts()
    {
        Alerts.Clear();

        var filtered = _allAlerts.AsEnumerable();

        filtered = FilterType switch
        {
            "Unread" => filtered.Where(a => !a.Alert.IsRead && !a.Alert.IsDismissed),
            "Active" => filtered.Where(a => a.Alert.IsActive && !a.Alert.IsDismissed),
            "Dismissed" => filtered.Where(a => a.Alert.IsDismissed),
            _ => filtered.Where(a => !a.Alert.IsDismissed)
        };

        foreach (var alert in filtered.OrderByDescending(a => a.Alert.LastTriggered ?? a.Alert.DateCreated))
        {
            Alerts.Add(alert);
        }

        OnPropertyChanged(nameof(AlertCountText));
        OnPropertyChanged(nameof(UnreadCountText));
    }

    private void AddAlert()
    {
        _dialogService.ShowInfo("Add Alert dialog will be implemented.");
    }

    private void ViewAlert(AlertDisplayItem? alertItem)
    {
        if (alertItem == null) return;

        var message = $"Item: {alertItem.ItemName}\n" +
                     $"Store: {alertItem.PlaceName}\n" +
                     $"Type: {alertItem.Alert.Type}\n" +
                     $"Message: {alertItem.DisplayMessage}\n" +
                     $"Created: {alertItem.Alert.DateCreated:g}\n" +
                     $"Last Triggered: {(alertItem.Alert.LastTriggered?.ToString("g") ?? "Never")}";

        _dialogService.ShowInfo(message, "Alert Details");

        if (!alertItem.Alert.IsRead)
        {
            MarkAsRead(alertItem);
        }
    }

    private void MarkAsRead(AlertDisplayItem? alertItem)
    {
        if (alertItem == null) return;

        try
        {
            _alertRepository.MarkAsRead(alertItem.Alert.Id);
            alertItem.Alert.IsRead = true;
            OnPropertyChanged(nameof(UnreadCountText));
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to mark alert as read: {ex.Message}");
        }
    }

    private void DismissAlert(AlertDisplayItem? alertItem)
    {
        if (alertItem == null) return;

        try
        {
            _alertRepository.Dismiss(alertItem.Alert.Id);
            _allAlerts.Remove(alertItem);
            FilterAlerts();
            _dialogService.ShowSuccess("Alert dismissed.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to dismiss alert: {ex.Message}");
        }
    }

    private void MarkAllAsRead()
    {
        try
        {
            _alertRepository.MarkAllAsRead();
            foreach (var alert in _allAlerts)
            {
                alert.Alert.IsRead = true;
            }
            OnPropertyChanged(nameof(UnreadCountText));
            _dialogService.ShowSuccess("All alerts marked as read.");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Failed to mark all as read: {ex.Message}");
        }
    }

    private void DismissAll()
    {
        var result = _dialogService.ShowConfirmation(
            "Are you sure you want to dismiss all read alerts?",
            "Confirm Dismiss All");

        if (result)
        {
            try
            {
                _alertRepository.DismissAllRead();
                _allAlerts.RemoveAll(a => a.Alert.IsRead);
                FilterAlerts();
                _dialogService.ShowSuccess("All read alerts dismissed.");
            }
            catch (Exception ex)
            {
                _dialogService.ShowError($"Failed to dismiss all alerts: {ex.Message}");
            }
        }
    }

    public void CheckForAlerts(string itemId, decimal oldPrice, decimal newPrice)
    {
        var alerts = _alertRepository.GetAlertsByItem(itemId);
        foreach (var alert in alerts)
        {
            if (alert.ShouldTrigger(oldPrice, newPrice))
            {
                alert.Trigger(newPrice, oldPrice);

                var item = _dataService.GetItemById(itemId);
                var place = alert.PlaceId != null ? _dataService.GetPlaceById(alert.PlaceId) : null;
                alert.Message = alert.GenerateMessage(item?.Name ?? "Unknown Item", place?.Name);

                _alertRepository.Update(alert);
            }
        }

        LoadAlerts();
    }
}

public class AlertDisplayItem
{
    public required AlertLogicEntity Alert { get; set; }
    public required string ItemName { get; set; }
    public required string PlaceName { get; set; }
    public required string DisplayMessage { get; set; }
}
