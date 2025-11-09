using System;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;

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

    public event Action? OnStoreAdded;

    public void RefreshDashboard()
    {
        try
        {
            TotalItems = _dataService.GetAllItems().Count();
            TrackedStores = _dataService.GetAllPlaces().Count();
            PriceUpdates = _dataService.GetRecentPriceUpdates(1000).Count();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error refreshing dashboard: {ex.Message}");
        }
    }

    private void AddItem()
    {
        // Open Add Item dialog
        _dialogService.ShowInfo("Add Item functionality will be implemented in the Items view.");
    }

    private void AddPlace()
    {
        // Open Add Place dialog
        _dialogService.ShowInfo("Add Store functionality will be implemented in the Stores view.");
        OnStoreAdded?.Invoke();
    }

    public void Dispose()
    {
        // Cleanup if needed
    }
}
