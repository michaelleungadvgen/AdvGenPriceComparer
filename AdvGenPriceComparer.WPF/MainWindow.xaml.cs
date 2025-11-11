using System;
using System.IO;
using System.Linq;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace AdvGenPriceComparer.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        this.Closed += OnClosed;
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Create charts programmatically to avoid XAML compilation issues
        var categoryChart = new PieChart();
        categoryChart.SetBinding(PieChart.SeriesProperty, nameof(ViewModel.CategorySeries));
        CategoryChartContainer.Child = categoryChart;

        var trendChart = new CartesianChart();
        trendChart.SetBinding(CartesianChart.SeriesProperty, nameof(ViewModel.PriceTrendSeries));
        trendChart.SetBinding(CartesianChart.XAxesProperty, nameof(ViewModel.XAxes));
        TrendChartContainer.Child = trendChart;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        ViewModel.Dispose();
    }

    private void DashboardNav_Click(object sender, RoutedEventArgs e)
    {
        // Show dashboard and hide frame
        DashboardContent.Visibility = Visibility.Visible;
        ContentFrame.Visibility = Visibility.Collapsed;
        UpdateNavigation("Dashboard");
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dataService = ((App)Application.Current).Services.GetRequiredService<IGroceryDataService>();
            var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
            var viewModel = new ItemViewModel(dataService, dialogService);
            var page = new ItemsPage(viewModel);

            // Hide dashboard and show frame
            DashboardContent.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Navigate(page);
            UpdateNavigation("Items");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error navigating to Items: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void StoresNav_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dataService = ((App)Application.Current).Services.GetRequiredService<IGroceryDataService>();
            var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
            var viewModel = new StoreViewModel(dataService, dialogService);
            var page = new StoresPage(viewModel);

            // Hide dashboard and show frame
            DashboardContent.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Navigate(page);
            UpdateNavigation("Stores");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error navigating to Stores: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CategoriesNav_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Categories view
        MessageBox.Show("Categories view - Coming soon!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void ReportsNav_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Reports view
        MessageBox.Show("Reports view - Coming soon!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void UpdateNavigation(string activePage)
    {
        // Reset all buttons
        DashboardNav.Background = System.Windows.Media.Brushes.Transparent;
        DashboardNav.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));
        DashboardNav.FontWeight = FontWeights.Normal;

        ItemsNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        ItemsNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));

        StoresNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        StoresNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));

        CategoriesNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        CategoriesNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));

        ReportsNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        ReportsNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));

        // Highlight active button
        switch (activePage)
        {
            case "Dashboard":
                DashboardNav.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dbeafe"));
                DashboardNav.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                DashboardNav.FontWeight = FontWeights.Medium;
                break;
            case "Items":
                ItemsNavBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dbeafe"));
                ItemsNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                break;
            case "Stores":
                StoresNavBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dbeafe"));
                StoresNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                break;
            case "Categories":
                CategoriesNavBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dbeafe"));
                CategoriesNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                break;
            case "Reports":
                ReportsNavBtn.Background = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#dbeafe"));
                ReportsNavBtn.Foreground = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#3b82f6"));
                break;
        }
    }

    private void GenerateDemoData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var demoDataService = ((App)Application.Current).Services.GetRequiredService<DemoDataService>();
            demoDataService.GenerateDemoData();

            MessageBox.Show("Demo data generated successfully! Check the Reports page to see charts with data.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Refresh dashboard
            ViewModel.RefreshDashboard();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating demo data: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ImportJsonData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var dataService = ((App)Application.Current).Services.GetRequiredService<IGroceryDataService>();
            var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();

            // Get database path
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer");
            var dbPath = Path.Combine(appDataPath, "GroceryPrices.db");

            var viewModel = new ImportDataViewModel(dataService, dialogService, dbPath);
            var window = new ImportDataWindow(viewModel) { Owner = this };

            if (window.ShowDialog() == true)
            {
                // Refresh dashboard stats
                ViewModel.RefreshDashboard();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening import dialog: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OldImportJsonData_Click_Backup(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create file dialog
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Title = "Import JSON Data",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var importService = ((App)Application.Current).Services
                    .GetRequiredService<AdvGenPriceComparer.Data.LiteDB.Services.JsonImportService>();

                // Import the file
                var result = importService.ImportFromFile(openFileDialog.FileName);

                if (result.Success)
                {
                    MessageBox.Show(result.Message ?? "Data imported successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Refresh dashboard stats
                    ViewModel.RefreshDashboard();
                }
                else
                {
                    var errorMessage = $"Import failed: {result.ErrorMessage}";

                    // Show detailed errors if any
                    if (result.Errors.Any())
                    {
                        var errorDetails = string.Join("\n", result.Errors.Take(5));
                        errorMessage += $"\n\nFirst 5 errors:\n{errorDetails}";
                    }

                    MessageBox.Show(errorMessage, "Import Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during import: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show(
            "AdvGen Price Comparer v1.0\n\n" +
            "An AI-powered grocery price tracking and comparison tool.\n\n" +
            "Features:\n" +
            "• Import price data from JSON files\n" +
            "• Track prices across multiple stores\n" +
            "• Compare prices and find best deals\n" +
            "• Generate analytics and reports\n\n" +
            "© 2025 AdvGen. All rights reserved.",
            "About AdvGen Price Comparer",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}