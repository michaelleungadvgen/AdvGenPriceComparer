using System;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;
using LiveChartsCore.SkiaSharpView.WPF;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for PriceForecastWindow.xaml
/// </summary>
public partial class PriceForecastWindow : Window
{
    public PriceForecastViewModel ViewModel { get; }
    private CartesianChart? _chart;

    public PriceForecastWindow(PriceForecastViewModel viewModel)
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
            Height = 300
        };
        
        // Bind chart properties
        _chart.SetBinding(CartesianChart.SeriesProperty, "ForecastSeries");
        _chart.SetBinding(CartesianChart.XAxesProperty, "ChartXAxes");
        _chart.SetBinding(CartesianChart.YAxesProperty, "ChartYAxes");
        
        ChartContainer.Child = _chart;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
