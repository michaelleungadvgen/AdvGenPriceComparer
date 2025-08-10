using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Desktop.WinUI.Views;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public class DialogService : IDialogService
{
    private readonly Window _mainWindow;

    public DialogService(Window mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task<bool> ShowAddItemDialogAsync(ItemViewModel itemViewModel)
    {
        var addItemView = new AddItemView
        {
            DataContext = itemViewModel
        };

        var dialog = new ContentDialog
        {
            Title = "Add Grocery Item",
            Content = addItemView,
            PrimaryButtonText = "Add Item",
            CloseButtonText = "Cancel",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        // Bind the primary button enabled state to the view model's IsValid property
        itemViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ItemViewModel.IsValid))
            {
                dialog.IsPrimaryButtonEnabled = itemViewModel.IsValid;
            }
        };

        // Set initial state
        dialog.IsPrimaryButtonEnabled = itemViewModel.IsValid;

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task<bool> ShowAddPlaceDialogAsync(PlaceViewModel placeViewModel)
    {
        var addPlaceView = new AddPlaceView
        {
            DataContext = placeViewModel
        };

        var dialog = new ContentDialog
        {
            Title = "Add Store/Supermarket",
            Content = addPlaceView,
            PrimaryButtonText = "Add Store",
            CloseButtonText = "Cancel",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        // Bind the primary button enabled state to the view model's IsValid property
        placeViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(PlaceViewModel.IsValid))
            {
                dialog.IsPrimaryButtonEnabled = placeViewModel.IsValid;
            }
        };

        // Set initial state
        dialog.IsPrimaryButtonEnabled = placeViewModel.IsValid;

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowComparePricesDialogAsync(IEnumerable<(Item item, decimal lowestPrice, Place place)> bestDeals)
    {
        var content = new StackPanel { Spacing = 12 };
        
        content.Children.Add(new TextBlock 
        { 
            Text = "Compare prices across different supermarkets:",
            TextWrapping = TextWrapping.Wrap
        });

        var dealsList = bestDeals.ToList();
        
        if (dealsList.Any())
        {
            content.Children.Add(new TextBlock 
            { 
                Text = "Best Deals:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 12, 0, 6)
            });

            foreach (var deal in dealsList.Take(10))
            {
                content.Children.Add(new TextBlock 
                { 
                    Text = $"• {deal.item.Name} - ${deal.lowestPrice:F2} at {deal.place.Name}",
                    Margin = new Thickness(12, 0, 0, 4)
                });
            }
        }
        else
        {
            content.Children.Add(new TextBlock 
            { 
                Text = "No price data available yet. Add some grocery items and record their prices!"
            });
        }

        var scrollViewer = new ScrollViewer
        {
            Content = content,
            MaxHeight = 400
        };

        var dialog = new ContentDialog
        {
            Title = "Compare Grocery Prices",
            Content = scrollViewer,
            CloseButtonText = "Close",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task ShowAnalyticsDialogAsync(Dictionary<string, object> stats)
    {
        var content = new StackPanel { Spacing = 12 };
        
        content.Children.Add(new TextBlock 
        { 
            Text = "Grocery Price Analytics", 
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold 
        });
        
        content.Children.Add(new TextBlock 
        { 
            Text = $"Total Items Tracked: {stats.GetValueOrDefault("totalItems", 0)}" 
        });
        
        content.Children.Add(new TextBlock 
        { 
            Text = $"Supermarkets: {stats.GetValueOrDefault("trackedStores", 0)}" 
        });
        
        content.Children.Add(new TextBlock 
        { 
            Text = $"Price Records: {stats.GetValueOrDefault("priceRecords", 0)}" 
        });
        
        content.Children.Add(new TextBlock 
        { 
            Text = $"Recent Updates: {stats.GetValueOrDefault("recentUpdates", 0)}" 
        });

        var dialog = new ContentDialog
        {
            Title = "Analytics Dashboard",
            Content = content,
            CloseButtonText = "Close",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task ShowNetworkDialogAsync(NetworkInfo networkInfo)
    {
        var content = new StackPanel { Spacing = 16 };
        
        content.Children.Add(new TextBlock 
        { 
            Text = "P2P Grocery Price Sharing Network",
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            FontSize = 16
        });

        // Network stats
        var statsPanel = new StackPanel { Spacing = 8 };
        statsPanel.Children.Add(new TextBlock 
        { 
            Text = $"Server Running: {(networkInfo.IsServerRunning ? "✅ Yes (Port 8081)" : "❌ No")}" 
        });
        statsPanel.Children.Add(new TextBlock 
        { 
            Text = $"Connected Peers: {networkInfo.ConnectedPeers.Count}" 
        });
        statsPanel.Children.Add(new TextBlock 
        { 
            Text = $"Node ID: {networkInfo.NodeId}" 
        });
        
        content.Children.Add(statsPanel);

        // Connected peers
        if (networkInfo.ConnectedPeers.Any())
        {
            content.Children.Add(new TextBlock 
            { 
                Text = "Connected Peers:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            });

            foreach (var peer in networkInfo.ConnectedPeers)
            {
                content.Children.Add(new TextBlock 
                { 
                    Text = $"• {peer.Host}:{peer.Port} ({peer.Region}) - {(peer.IsConnected ? "Connected" : "Disconnected")}",
                    Margin = new Thickness(12, 0, 0, 4)
                });
            }
        }

        // Recent price updates
        if (networkInfo.RecentUpdates.Any())
        {
            content.Children.Add(new TextBlock 
            { 
                Text = "Recent Price Updates from Network:",
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Margin = new Thickness(0, 8, 0, 4)
            });

            foreach (var update in networkInfo.RecentUpdates.Take(5))
            {
                content.Children.Add(new TextBlock 
                { 
                    Text = $"• {update.ItemName} - ${update.Price:F2} at {update.StoreName}",
                    Margin = new Thickness(12, 0, 0, 4),
                    Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Green)
                });
            }
        }

        var scrollViewer = new ScrollViewer
        {
            Content = content,
            MaxHeight = 400,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        var dialog = new ContentDialog
        {
            Title = "P2P Network Status",
            Content = scrollViewer,
            CloseButtonText = "Close",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowMessageDialogAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _mainWindow.Content.XamlRoot
        };

        await dialog.ShowAsync();
    }
}