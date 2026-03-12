using System.Windows;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for BestPricesWindow.xaml
/// </summary>
public partial class BestPricesWindow : Window
{
    private readonly BestPricesViewModel _viewModel;

    public BestPricesWindow(BestPricesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to events
        _viewModel.RequestClose += (s, e) => Close();
        _viewModel.RequestViewDetails += OnRequestViewDetails;
    }

    private void OnRequestViewDetails(object? sender, BestPriceInfo info)
    {
        // Show detailed information about the selected deal
        var details = $"Item: {info.ItemName}\n\n" +
                      $"Current Best Price: ${info.BestPrice:F2} at {info.BestStoreName}\n" +
                      $"Historical Low: ${info.HistoricalLow:F2}\n" +
                      $"Historical High: ${info.HistoricalHigh:F2}\n" +
                      $"Average Price: ${info.AveragePrice:F2}\n\n" +
                      $"You Save: ${info.SavingsAmount:F2} ({info.SavingsPercent:F0}% off average)\n\n" +
                      $"Status: {(info.IsHistoricalLow ? "🔥 At Historical Low!" : info.IsBestDeal ? "⭐ Great Deal!" : "Good Price")}";

        MessageBox.Show(details, "Price Details", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
