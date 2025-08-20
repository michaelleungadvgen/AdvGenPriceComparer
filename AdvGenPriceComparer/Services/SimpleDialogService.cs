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

public class SimpleDialogService : IDialogService
{
    private XamlRoot? _xamlRoot;

    public void Initialize(XamlRoot xamlRoot)
    {
        _xamlRoot = xamlRoot;
    }

    public async Task<bool> ShowAddItemDialogAsync(ItemViewModel itemViewModel)
    {
        if (_xamlRoot == null) return false;

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
            XamlRoot = _xamlRoot
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
        System.Diagnostics.Debug.WriteLine($"ShowAddPlaceDialogAsync called, _xamlRoot is null: {_xamlRoot == null}");
        
        if (_xamlRoot == null) 
        {
            System.Diagnostics.Debug.WriteLine("_xamlRoot is null, returning false");
            return false;
        }

        System.Diagnostics.Debug.WriteLine("Creating AddPlaceView");
        var addPlaceView = new AddPlaceView
        {
            DataContext = placeViewModel
        };

        System.Diagnostics.Debug.WriteLine("Creating ContentDialog");
        var dialog = new ContentDialog
        {
            Title = "Add Store/Supermarket",
            Content = addPlaceView,
            PrimaryButtonText = "Add Store",
            CloseButtonText = "Cancel",
            XamlRoot = _xamlRoot
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

        System.Diagnostics.Debug.WriteLine("Showing dialog");
        var result = await dialog.ShowAsync();
        System.Diagnostics.Debug.WriteLine($"Dialog closed with result: {result}");
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowComparePricesDialogAsync(IEnumerable<(Item item, decimal lowestPrice, Place place)> bestDeals)
    {
        if (_xamlRoot == null) return;

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
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task ShowAnalyticsDialogAsync(Dictionary<string, object> stats)
    {
        if (_xamlRoot == null) return;

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
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task ShowNetworkDialogAsync(NetworkInfo networkInfo)
    {
        if (_xamlRoot == null) return;

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
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }

    public async Task<bool> ShowConfirmationDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return false;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            XamlRoot = _xamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }

    public async Task ShowMessageDialogAsync(string title, string message)
    {
        if (_xamlRoot == null) return;

        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = _xamlRoot
        };

        await dialog.ShowAsync();
    }
}