using System;
using System.Windows;
using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;
using LiveChartsCore.SkiaSharpView.WPF;

namespace AdvGenPriceComparer.WPF.Views;

public partial class PriceHistoryPage : Page
{
    public PriceHistoryViewModel ViewModel { get; }
    private CartesianChart? _chart;

    public PriceHistoryPage(PriceHistoryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        // Create chart programmatically to avoid XAML compilation issues
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Create chart
        _chart = new CartesianChart
        {
            Height = 350
        };
        
        // Bind chart properties
        _chart.SetBinding(CartesianChart.SeriesProperty, "PriceHistorySeries");
        _chart.SetBinding(CartesianChart.XAxesProperty, "ChartXAxes");
        _chart.SetBinding(CartesianChart.YAxesProperty, "ChartYAxes");
        
        ChartContainer.Child = _chart;
    }
}
