using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.Desktop.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowAddItemDialogAsync();
    }

    private void SharePrices_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowSharePriceDialogAsync();
    }

    private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to analytics page
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowAddItemDialogAsync();
    }

    private void SharingNav_Click(object sender, RoutedEventArgs e)
    {
        _ = ShowSharePriceDialogAsync();
    }

    private async Task ShowAddItemDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Add Grocery Item",
            Content = "Add a new grocery item to track prices across supermarkets (Coles, Woolworths, IGA, etc.)",
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync().AsTask();
    }

    private async Task ShowSharePriceDialogAsync()
    {
        var dialog = new ContentDialog
        {
            Title = "Compare Grocery Prices",
            Content = "Compare prices for grocery items across different supermarkets to find the best deals.",
            CloseButtonText = "OK",
            XamlRoot = this.Content.XamlRoot
        };

        await dialog.ShowAsync().AsTask();
    }
}
