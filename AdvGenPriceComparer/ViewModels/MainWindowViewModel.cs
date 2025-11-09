using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Desktop.WinUI.Services;

namespace AdvGenPriceComparer.Desktop.WinUI.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly IGroceryDataService _groceryDataService;
    private readonly NetworkManager _networkManager;
    private readonly IDialogService _dialogService;
    private readonly INotificationService _notificationService;

    private int _totalItems;
    private int _trackedStores;
    private int _priceUpdates;
    private int _networkUsers;
    private bool _isServerRunning;
    private string _nodeId = string.Empty;
    private string _searchText = string.Empty;

    public MainWindowViewModel(
        IGroceryDataService groceryDataService,
        NetworkManager networkManager,
        IDialogService dialogService,
        INotificationService notificationService)
    {
        _groceryDataService = groceryDataService;
        _networkManager = networkManager;
        _dialogService = dialogService;
        _notificationService = notificationService;

        // Initialize commands
        AddItemCommand = new RelayCommand(async () => await ShowAddItemDialogAsync());
        AddPlaceCommand = new RelayCommand(async () => await ShowAddPlaceDialogAsync());
        ComparePricesCommand = new RelayCommand(async () => await ShowComparePricesDialogAsync());
        ViewAnalyticsCommand = new RelayCommand(async () => await ShowAnalyticsDialogAsync());
        ViewNetworkCommand = new RelayCommand(async () => await ShowNetworkDialogAsync());
        SearchCommand = new RelayCommand(async () => await PerformSearchAsync(), () => !string.IsNullOrWhiteSpace(SearchText));

        // Initialize collections
        ConnectedPeers = new ObservableCollection<NetworkPeer>();
        RecentPriceUpdates = new ObservableCollection<PriceShareMessage>();

        // Subscribe to network events
        _networkManager.PeerConnected += OnPeerConnected;
        _networkManager.PeerDisconnected += OnPeerDisconnected;
        _networkManager.PriceReceived += OnPriceReceived;

        // Load initial data
        _ = LoadDashboardDataAsync();
        _ = InitializeNetworkingAsync();
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

    public bool IsServerRunning
    {
        get => _isServerRunning;
        set => SetProperty(ref _isServerRunning, value);
    }

    public string NodeId
    {
        get => _nodeId;
        set => SetProperty(ref _nodeId, value);
    }

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value, () =>
        {
            ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
        });
    }

    public ObservableCollection<NetworkPeer> ConnectedPeers { get; }
    public ObservableCollection<PriceShareMessage> RecentPriceUpdates { get; }

    // Events
    public event Action? OnStoreAdded;

    #endregion

    #region Commands

    public ICommand AddItemCommand { get; }
    public ICommand AddPlaceCommand { get; }
    public ICommand ComparePricesCommand { get; }
    public ICommand ViewAnalyticsCommand { get; }
    public ICommand ViewNetworkCommand { get; }
    public ICommand SearchCommand { get; }

    #endregion

    #region Methods

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var stats = _groceryDataService.GetDashboardStats();

            TotalItems = (int)stats["totalItems"];
            TrackedStores = (int)stats["trackedStores"];
            PriceUpdates = (int)stats["priceRecords"];

            // Network users is the count of connected peers
            NetworkUsers = ConnectedPeers.Count;
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error loading dashboard data: {ex.Message}");
        }
    }

    /// <summary>
    /// Public method to refresh dashboard statistics
    /// </summary>
    public void RefreshDashboard()
    {
        _ = LoadDashboardDataAsync();
    }

    private async Task InitializeNetworkingAsync()
    {
        try
        {
            // Start local server
            var serverStarted = await _networkManager.StartServer(8081);
            IsServerRunning = serverStarted;
            NodeId = _networkManager.NodeId;

            if (serverStarted)
            {
                // Connect to available servers
                await _networkManager.DiscoverAndConnectToServers("NSW");
            }
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error initializing networking: {ex.Message}");
        }
    }

    private async Task ShowAddItemDialogAsync()
    {
        try
        {
            var itemViewModel = new ItemViewModel();
            var result = await _dialogService.ShowAddItemDialogAsync(itemViewModel);
            
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

                await _notificationService.ShowSuccessAsync("Item added successfully!");
                await LoadDashboardDataAsync();
            }
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error adding item: {ex.Message}");
        }
    }

    private async Task ShowAddPlaceDialogAsync()
    {
        try
        {
            var placeViewModel = new PlaceViewModel();
            var result = await _dialogService.ShowAddPlaceDialogAsync(placeViewModel);
            
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

                await _notificationService.ShowSuccessAsync("Store added successfully!");
                await LoadDashboardDataAsync();
                
                // Navigate to the Store View after adding a place
                OnStoreAdded?.Invoke();
            }
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error adding store: {ex.Message}");
        }
    }

    private async Task ShowComparePricesDialogAsync()
    {
        try
        {
            var bestDeals = _groceryDataService.FindBestDeals().Take(10).ToList();
            await _dialogService.ShowComparePricesDialogAsync(bestDeals);
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error loading price comparisons: {ex.Message}");
        }
    }

    private async Task ShowAnalyticsDialogAsync()
    {
        try
        {
            var stats = _groceryDataService.GetDashboardStats();
            await _dialogService.ShowAnalyticsDialogAsync(stats);
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error loading analytics: {ex.Message}");
        }
    }

    private async Task ShowNetworkDialogAsync()
    {
        try
        {
            var networkInfo = new NetworkInfo
            {
                IsServerRunning = IsServerRunning,
                NodeId = NodeId,
                ConnectedPeers = ConnectedPeers.ToList(),
                RecentUpdates = RecentPriceUpdates.ToList()
            };
            
            await _dialogService.ShowNetworkDialogAsync(networkInfo);
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error showing network dialog: {ex.Message}");
        }
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
            return;

        try
        {
            // TODO: Implement search functionality
            await _notificationService.ShowInfoAsync($"Searching for: {SearchText}");
        }
        catch (Exception ex)
        {
            await _notificationService.ShowErrorAsync($"Error performing search: {ex.Message}");
        }
    }

    #endregion

    #region Event Handlers

    private void OnPeerConnected(object? sender, NetworkPeer peer)
    {
        ConnectedPeers.Add(peer);
        NetworkUsers = ConnectedPeers.Count;
    }

    private void OnPeerDisconnected(object? sender, NetworkPeer peer)
    {
        var existingPeer = ConnectedPeers.FirstOrDefault(p => p.Id == peer.Id);
        if (existingPeer != null)
        {
            ConnectedPeers.Remove(existingPeer);
            NetworkUsers = ConnectedPeers.Count;
        }
    }

    private void OnPriceReceived(object? sender, PriceShareMessage priceMessage)
    {
        RecentPriceUpdates.Insert(0, priceMessage);
        
        // Keep only last 10 updates
        while (RecentPriceUpdates.Count > 10)
        {
            RecentPriceUpdates.RemoveAt(RecentPriceUpdates.Count - 1);
        }
        
        _ = LoadDashboardDataAsync(); // Refresh stats
    }

    #endregion

    public void Dispose()
    {
        _networkManager?.Dispose();
    }
}

public class NetworkInfo
{
    public bool IsServerRunning { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public List<NetworkPeer> ConnectedPeers { get; set; } = new();
    public List<PriceShareMessage> RecentUpdates { get; set; } = new();
}