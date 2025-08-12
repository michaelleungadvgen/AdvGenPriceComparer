using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Core.Helpers;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Services;
using System.IO;
using System.Linq;
using System.Collections.ObjectModel;

namespace AdvGenPriceComparer.Desktop.WinUI;

public sealed partial class MainWindow : Window
{
    private readonly IGroceryDataService _groceryData;
    private readonly NetworkManager _networkManager;
    private readonly ServerConfigService _serverConfig;
    private readonly ObservableCollection<NetworkPeer> _connectedPeers = new();
    private readonly ObservableCollection<PriceShareMessage> _recentPriceUpdates = new();

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize database in AppData folder
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AdvGenPriceComparer");
        Directory.CreateDirectory(appDataPath);
        var dbPath = Path.Combine(appDataPath, "GroceryPrices.db");
        
        _groceryData = new GroceryDataService(dbPath);
        
        // Initialize P2P networking
        var serverConfigPath = Path.Combine(appDataPath, "servers.json");
        
        // Copy servers.json from project root if it doesn't exist
        if (!File.Exists(serverConfigPath))
        {
            var projectServerPath = Path.Combine(Directory.GetCurrentDirectory(), "servers.json");
            if (File.Exists(projectServerPath))
            {
                File.Copy(projectServerPath, serverConfigPath);
            }
        }
        
        _serverConfig = new ServerConfigService(serverConfigPath);
        _networkManager = new NetworkManager(_groceryData, _serverConfig);
        
        
        // Subscribe to network events
        _networkManager.PeerConnected += OnPeerConnected;
        _networkManager.PeerDisconnected += OnPeerDisconnected;
        _networkManager.PriceReceived += OnPriceReceived;
        _networkManager.ErrorOccurred += OnNetworkError;
        
        // Subscribe to window close event
        this.Closed += Window_Closed;
        
        LoadDashboardData();
        _ = InitializeNetworking();
    }


    private void LoadDashboardData()
    {
        try
        {
            var stats = _groceryData.GetDashboardStats();
            
            // Update UI with real data - these would be bound to UI elements in a full MVVM implementation
            // For now, this demonstrates how to use the database services
            var totalItems = (int)stats["totalItems"];
            var trackedStores = (int)stats["trackedStores"];
            var priceRecords = (int)stats["priceRecords"];
            
            // TODO: Update UI elements with these values
        }
        catch (Exception ex)
        {
            // Log error - in production, use proper logging
            System.Diagnostics.Debug.WriteLine($"Error loading dashboard data: {ex.Message}");
        }
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowAddItemDialogAsync();
    }

    private void AddPlace_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowAddPlaceDialogAsync();
    }

    private void SharePrices_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowComparePricesDialogAsync();
    }

    private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowAnalyticsDialogAsync();
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.ItemListView));
    }

    private void StoresNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.PlaceListView));
    }

    private void SharingNav_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowComparePricesDialogAsync();
    }

    private void NetworkNav_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowNetworkStatusDialogAsync();
    }

    private async Task ShowAddItemDialogAsync()
    {
        var addItemControl = new Controls.AddItemControl();
        
        var dialog = new ContentDialog
        {
            Title = "Add Grocery Item",
            Content = addItemControl,
            PrimaryButtonText = "Add Item",
            CloseButtonText = "Cancel",
            XamlRoot = this.Content.XamlRoot
        };

        // Update primary button enabled state based on validation
        addItemControl.ValidationChanged += (s, isValid) =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        };

        // Initial validation check
        dialog.IsPrimaryButtonEnabled = addItemControl.IsValid();

        var result = await dialog.ShowAsync().AsTask();
        
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var item = addItemControl.CreateItemFromForm();
                
                // Add item to database using the comprehensive grocery data service
                var itemId = _groceryData.AddGroceryItem(
                    item.Name,
                    item.Brand ?? "",
                    item.Category ?? "Other",
                    item.Barcode ?? "",
                    item.PackageSize ?? ""
                );
                
                await ShowSuccessMessageAsync("Item added successfully!");
                LoadDashboardData(); // Refresh dashboard
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Error adding item: {ex.Message}");
            }
        }
    }

    private async Task ShowAddPlaceDialogAsync()
    {
        var addPlaceControl = new Controls.AddPlaceControl();
        
        var dialog = new ContentDialog
        {
            Title = "Add Store/Supermarket",
            Content = addPlaceControl,
            PrimaryButtonText = "Add Store",
            CloseButtonText = "Cancel",
            XamlRoot = this.Content.XamlRoot
        };

        // Update primary button enabled state based on validation
        addPlaceControl.ValidationChanged += (s, isValid) =>
        {
            dialog.IsPrimaryButtonEnabled = isValid;
        };

        // Initial validation check
        dialog.IsPrimaryButtonEnabled = addPlaceControl.IsValid();

        var result = await dialog.ShowAsync().AsTask();
        
        if (result == ContentDialogResult.Primary)
        {
            try
            {
                var place = addPlaceControl.CreatePlaceFromForm();
                
                // Add place to database using the grocery data service
                var placeId = _groceryData.AddSupermarket(
                    place.Name,
                    place.Chain ?? "",
                    place.Suburb,
                    place.State ?? "",
                    place.Postcode ?? ""
                );
                
                await ShowSuccessMessageAsync("Store added successfully!");
                LoadDashboardData(); // Refresh dashboard
            }
            catch (Exception ex)
            {
                await ShowErrorMessageAsync($"Error adding store: {ex.Message}");
            }
        }
    }

    private StackPanel CreateAddItemContent()
    {
        var panel = new StackPanel { Spacing = 12 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Add a new grocery item to track prices across supermarkets:",
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 12)
        });
        
        panel.Children.Add(new TextBox 
        { 
            PlaceholderText = "Item name (e.g., Bread White 680g)",
            Header = "Product Name"
        });
        
        panel.Children.Add(new TextBox 
        { 
            PlaceholderText = "Brand (e.g., Tip Top, Helga's)",
            Header = "Brand"
        });
        
        panel.Children.Add(new ComboBox 
        { 
            Header = "Category",
            PlaceholderText = "Select category",
            Items = { "Bakery", "Dairy", "Meat", "Produce", "Pantry", "Frozen", "Other" }
        });
        
        return panel;
    }

    private async Task ShowComparePricesDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Compare Grocery Prices",
            Content = CreateComparePricesContent(),
            CloseButtonText = "Close",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync().AsTask();
    }

    private StackPanel CreateComparePricesContent()
    {
        var panel = new StackPanel { Spacing = 12 };
        
        panel.Children.Add(new TextBlock 
        { 
            Text = "Compare prices across different supermarkets:",
            TextWrapping = TextWrapping.Wrap
        });

        try
        {
            var bestDeals = _groceryData.FindBestDeals().Take(5);
            
            if (bestDeals.Any())
            {
                panel.Children.Add(new TextBlock 
                { 
                    Text = "Best Deals:",
                    FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                    Margin = new Thickness(0, 12, 0, 6)
                });

                foreach (var deal in bestDeals)
                {
                    panel.Children.Add(new TextBlock 
                    { 
                        Text = $"• {deal.item.Name} - ${deal.lowestPrice:F2} at {deal.place.Name}",
                        Margin = new Thickness(12, 0, 0, 4)
                    });
                }
            }
            else
            {
                panel.Children.Add(new TextBlock 
                { 
                    Text = "No price data available yet. Add some grocery items and record their prices!"
                });
            }
        }
        catch (Exception ex)
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = $"Error loading price comparisons: {ex.Message}",
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Red)
            });
        }

        return panel;
    }

    private async Task ShowAnalyticsDialogAsync()
    {
        var stats = _groceryData.GetDashboardStats();
        
        var content = new StackPanel { Spacing = 12 };
        content.Children.Add(new TextBlock { Text = "Grocery Price Analytics", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        content.Children.Add(new TextBlock { Text = $"Total Items Tracked: {stats["totalItems"]}" });
        content.Children.Add(new TextBlock { Text = $"Supermarkets: {stats["trackedStores"]}" });
        content.Children.Add(new TextBlock { Text = $"Price Records: {stats["priceRecords"]}" });
        content.Children.Add(new TextBlock { Text = $"Recent Updates: {stats["recentUpdates"]}" });

        var dialog = new ContentDialog
        {
            Title = "Analytics Dashboard",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync().AsTask();
    }

    private async Task ShowNetworkStatusDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "P2P Network Status",
            Content = CreateNetworkStatusContent(),
            PrimaryButtonText = "Connect to Servers",
            SecondaryButtonText = "Share Test Price",
            CloseButtonText = "Close",
            XamlRoot = this.Content.XamlRoot
        };

        var result = await dialog.ShowAsync().AsTask();
        
        if (result == ContentDialogResult.Primary)
        {
            await _networkManager.DiscoverAndConnectToServers();
            await ShowSuccessMessageAsync("Attempting to connect to available servers...");
        }
        else if (result == ContentDialogResult.Secondary)
        {
            await ShareTestPrice();
        }
    }

    private ScrollViewer CreateNetworkStatusContent()
    {
        var panel = new StackPanel { Spacing = 16 };
        
        // Server status
        panel.Children.Add(new TextBlock 
        { 
            Text = "P2P Grocery Price Sharing Network",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            FontSize = 16
        });

        // Network stats
        var statsPanel = new StackPanel { Spacing = 8 };
        statsPanel.Children.Add(new TextBlock { Text = $"Server Running: {(_networkManager.IsServerRunning ? "✅ Yes (Port 8081)" : "❌ No")}" });
        statsPanel.Children.Add(new TextBlock { Text = $"Connected Peers: {_connectedPeers.Count}" });
        statsPanel.Children.Add(new TextBlock { Text = $"Node ID: {_networkManager.NodeId}" });
        
        panel.Children.Add(statsPanel);

        // Connected peers
        if (_connectedPeers.Any())
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = "Connected Peers:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            });

            foreach (var peer in _connectedPeers)
            {
                var peerInfo = new TextBlock 
                { 
                    Text = $"• {peer.Host}:{peer.Port} ({peer.Region}) - {(peer.IsConnected ? "Connected" : "Disconnected")}",
                    Margin = new Thickness(12, 0, 0, 4)
                };
                panel.Children.Add(peerInfo);
            }
        }

        // Recent price updates from network
        if (_recentPriceUpdates.Any())
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = "Recent Price Updates from Network:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            });

            foreach (var update in _recentPriceUpdates.Take(5))
            {
                var updateInfo = new TextBlock 
                { 
                    Text = $"• {update.ItemName} - ${update.Price:F2} at {update.StoreName}",
                    Margin = new Thickness(12, 0, 0, 4),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                };
                panel.Children.Add(updateInfo);
            }
        }

        // Available servers
        var servers = _serverConfig.GetActiveServers();
        if (servers.Any())
        {
            panel.Children.Add(new TextBlock 
            { 
                Text = "Available Servers:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            });

            foreach (var server in servers)
            {
                var serverInfo = new TextBlock 
                { 
                    Text = $"• {server.Name} ({server.Region}) - {server.Host}:{server.Port}",
                    Margin = new Thickness(12, 0, 0, 4)
                };
                panel.Children.Add(serverInfo);
            }
        }

        return new ScrollViewer 
        { 
            Content = panel,
            MaxHeight = 400,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };
    }

    private async Task ShareTestPrice()
    {
        try
        {
            // Create a test item and place if they don't exist
            var testItemId = _groceryData.AddGroceryItem("Test Bread White 680g", "Test Brand", "Bakery", "1234567890", "680g");
            var testPlaceId = _groceryData.AddSupermarket("Test Coles", "Coles", "Sydney", "NSW", "2000");
            
            // Share a test price
            await _networkManager.SharePrice(testItemId, testPlaceId, 3.50m, true, 4.00m, "Weekly Special");
            
            await ShowSuccessMessageAsync("Test price shared with network!");
        }
        catch (Exception ex)
        {
            await ShowErrorMessageAsync($"Error sharing test price: {ex.Message}");
        }
    }

    private async Task ShowSuccessMessageAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Success",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };
        await dialog.ShowAsync().AsTask();
    }

    private async Task ShowErrorMessageAsync(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };
        await dialog.ShowAsync().AsTask();
    }

    #region P2P Networking

    private async Task InitializeNetworking()
    {
        try
        {
            // Start local server
            var serverStarted = await _networkManager.StartServer(8081);
            if (serverStarted)
            {
                System.Diagnostics.Debug.WriteLine("P2P server started on port 8081");
            }

            // Connect to available servers in the user's region (defaulting to NSW/Australia)
            await _networkManager.DiscoverAndConnectToServers("NSW");
            
            UpdateNetworkStatus();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing networking: {ex.Message}");
        }
    }

    private void OnPeerConnected(object sender, NetworkPeer peer)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _connectedPeers.Add(peer);
            UpdateNetworkStatus();
        });
    }

    private void OnPeerDisconnected(object sender, NetworkPeer peer)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            var existingPeer = _connectedPeers.FirstOrDefault(p => p.Id == peer.Id);
            if (existingPeer != null)
            {
                _connectedPeers.Remove(existingPeer);
            }
            UpdateNetworkStatus();
        });
    }

    private void OnPriceReceived(object sender, PriceShareMessage priceMessage)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            _recentPriceUpdates.Insert(0, priceMessage);
            
            // Keep only last 10 updates
            while (_recentPriceUpdates.Count > 10)
            {
                _recentPriceUpdates.RemoveAt(_recentPriceUpdates.Count - 1);
            }
            
            LoadDashboardData(); // Refresh stats
        });
    }

    private void OnNetworkError(object sender, string error)
    {
        DispatcherQueue.TryEnqueue(() =>
        {
            System.Diagnostics.Debug.WriteLine($"Network error: {error}");
        });
    }

    private void UpdateNetworkStatus()
    {
        // TODO: Update UI elements showing network status
        // This would update the "Network Users" stat card and any network status indicators
    }

    public async Task ShareCurrentPrice(string itemId, string placeId, decimal price, bool isOnSale = false, decimal? originalPrice = null, string saleDescription = null)
    {
        try
        {
            await _networkManager.SharePrice(itemId, placeId, price, isOnSale, originalPrice, saleDescription);
            await ShowSuccessMessageAsync("Price shared with network!");
        }
        catch (Exception ex)
        {
            await ShowErrorMessageAsync($"Error sharing price: {ex.Message}");
        }
    }

    private void Window_Closed(object sender, WindowEventArgs args)
    {
        _networkManager?.Dispose();
    }

    #endregion
}
