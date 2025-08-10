using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Core.Services;
using AdvGenPriceComparer.Desktop.WinUI.Services;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

/// <summary>
/// Legacy version that can be used with the existing MainWindow without dependency injection
/// </summary>
public class MainWindowViewModelLegacy : BaseViewModel
{
    private readonly IGroceryDataService _groceryDataService;
    private readonly NetworkManager _networkManager;
    private readonly ServerConfigService _serverConfig;
    private readonly Window _mainWindow;

    private int _totalItems;
    private int _trackedStores;
    private int _priceUpdates;
    private int _networkUsers;

    public MainWindowViewModelLegacy(Window mainWindow)
    {
        _mainWindow = mainWindow;

        // Initialize services (same as original MainWindow.xaml.cs)
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer");
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "GroceryPrices.db");

        _groceryDataService = new GroceryDataService(dbPath);

        var serverConfigPath = Path.Combine(appDataPath, "servers.json");
        if (!File.Exists(serverConfigPath))
        {
            var projectServerPath = Path.Combine(Directory.GetCurrentDirectory(), "servers.json");
            if (File.Exists(projectServerPath))
            {
                File.Copy(projectServerPath, serverConfigPath);
            }
        }

        _serverConfig = new ServerConfigService(serverConfigPath);
        _networkManager = new NetworkManager(_groceryDataService, _serverConfig);

        // Commands
        AddItemCommand = new RelayCommand(async () => await ShowAddItemDialogAsync());
        AddPlaceCommand = new RelayCommand(async () => await ShowAddPlaceDialogAsync());
        ComparePricesCommand = new RelayCommand(async () => await ShowComparePricesDialogAsync());
        ViewAnalyticsCommand = new RelayCommand(async () => await ShowAnalyticsDialogAsync());

        LoadDashboardData();
    }

    #region Properties

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

    public int NetworkUsers
    {
        get => _networkUsers;
        set => SetProperty(ref _networkUsers, value);
    }

    #endregion

    #region Commands

    public RelayCommand AddItemCommand { get; }
    public RelayCommand AddPlaceCommand { get; }
    public RelayCommand ComparePricesCommand { get; }
    public RelayCommand ViewAnalyticsCommand { get; }

    #endregion

    #region Methods

    private void LoadDashboardData()
    {
        try
        {
            var stats = _groceryDataService.GetDashboardStats();

            TotalItems = (int)stats["totalItems"];
            TrackedStores = (int)stats["trackedStores"];
            PriceUpdates = (int)stats["priceRecords"];
            NetworkUsers = 0; // Will be updated when network is implemented
        }
        catch (Exception ex)
        {
            // Log error
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
    }

    private async Task ShowAddItemDialogAsync()
    {
        var itemViewModel = new ItemViewModel();
        var dialogService = new DialogService(_mainWindow);
        var notificationService = new NotificationService(_mainWindow);

        try
        {
            var result = await dialogService.ShowAddItemDialogAsync(itemViewModel);

            if (result)
            {
                var item = itemViewModel.CreateItem();
                var itemId = _groceryDataService.AddGroceryItem(
                    item.Name,
                    item.Brand ?? string.Empty,
                    item.Category ?? "Other",
                    item.Barcode ?? string.Empty,
                    item.PackageSize ?? string.Empty
                );

                await notificationService.ShowSuccessAsync("Item added successfully!");
                LoadDashboardData();
            }
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"Error adding item: {ex.Message}");
        }
    }

    private async Task ShowAddPlaceDialogAsync()
    {
        var placeViewModel = new PlaceViewModel();
        var dialogService = new DialogService(_mainWindow);
        var notificationService = new NotificationService(_mainWindow);

        try
        {
            var result = await dialogService.ShowAddPlaceDialogAsync(placeViewModel);

            if (result)
            {
                var place = placeViewModel.CreatePlace();
                var placeId = _groceryDataService.AddSupermarket(
                    place.Name,
                    place.Chain ?? string.Empty,
                    place.Suburb,
                    place.State ?? string.Empty,
                    place.Postcode ?? string.Empty
                );

                await notificationService.ShowSuccessAsync("Store added successfully!");
                LoadDashboardData();
            }
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"Error adding store: {ex.Message}");
        }
    }

    private async Task ShowComparePricesDialogAsync()
    {
        var dialogService = new DialogService(_mainWindow);
        var notificationService = new NotificationService(_mainWindow);

        try
        {
            var bestDeals = _groceryDataService.FindBestDeals().Take(10).ToList();
            await dialogService.ShowComparePricesDialogAsync(bestDeals);
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"Error loading price comparisons: {ex.Message}");
        }
    }

    private async Task ShowAnalyticsDialogAsync()
    {
        var dialogService = new DialogService(_mainWindow);
        var notificationService = new NotificationService(_mainWindow);

        try
        {
            var stats = _groceryDataService.GetDashboardStats();
            await dialogService.ShowAnalyticsDialogAsync(stats);
        }
        catch (Exception ex)
        {
            await notificationService.ShowErrorAsync($"Error loading analytics: {ex.Message}");
        }
    }

    #endregion

    public void Dispose()
    {
        _networkManager?.Dispose();
    }
}