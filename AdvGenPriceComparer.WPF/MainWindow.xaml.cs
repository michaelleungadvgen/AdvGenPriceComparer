using System;
using System.IO;
using System.Linq;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;
using AdvGenPriceComparer.Data.LiteDB.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Wpf.Ui.Controls;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace AdvGenPriceComparer.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
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
        try
        {
            var dataService = ((App)Application.Current).Services.GetRequiredService<IGroceryDataService>();
            var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();

            // Cast to GroceryDataService since CategoryViewModel needs it
            if (dataService is not GroceryDataService groceryDataService)
            {
                throw new InvalidOperationException("IGroceryDataService is not of type GroceryDataService");
            }

            var viewModel = new CategoryViewModel(groceryDataService, logger);
            var page = new CategoryPage(viewModel);

            // Hide dashboard and show frame
            DashboardContent.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Navigate(page);
            UpdateNavigation("Categories");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error navigating to Categories: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PriceHistoryNav_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var priceHistoryViewModel = ((App)Application.Current).Services.GetRequiredService<PriceHistoryViewModel>();
            var page = new PriceHistoryPage(priceHistoryViewModel);

            DashboardContent.Visibility = Visibility.Collapsed;
            ContentFrame.Visibility = Visibility.Visible;
            ContentFrame.Navigate(page);
            UpdateNavigation("PriceHistory");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error navigating to Price History: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ReportsNav_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Reports view
        MessageBox.Show("Reports view - Coming soon!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void UpdateNavigation(string activePage)
    {
        // Get theme colors from resources
        var accentLight = (System.Windows.Media.SolidColorBrush)Application.Current.FindResource("SystemAccentBrushLight3");
        var accentDark = (System.Windows.Media.SolidColorBrush)Application.Current.FindResource("SystemAccentBrushDark3");
        var defaultColor = new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#555"));

        // Reset all buttons
        DashboardNav.Background = System.Windows.Media.Brushes.Transparent;
        DashboardNav.Foreground = defaultColor;
        DashboardNav.FontWeight = FontWeights.Normal;

        ItemsNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        ItemsNavBtn.Foreground = defaultColor;

        StoresNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        StoresNavBtn.Foreground = defaultColor;

        CategoriesNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        CategoriesNavBtn.Foreground = defaultColor;

        PriceHistoryNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        PriceHistoryNavBtn.Foreground = defaultColor;

        ReportsNavBtn.Background = System.Windows.Media.Brushes.Transparent;
        ReportsNavBtn.Foreground = defaultColor;

        // Highlight active button with light blue theme
        switch (activePage)
        {
            case "Dashboard":
                DashboardNav.Background = accentLight;
                DashboardNav.Foreground = accentDark;
                DashboardNav.FontWeight = FontWeights.Medium;
                break;
            case "Items":
                ItemsNavBtn.Background = accentLight;
                ItemsNavBtn.Foreground = accentDark;
                break;
            case "Stores":
                StoresNavBtn.Background = accentLight;
                StoresNavBtn.Foreground = accentDark;
                break;
            case "Categories":
                CategoriesNavBtn.Background = accentLight;
                CategoriesNavBtn.Foreground = accentDark;
                break;
            case "PriceHistory":
                PriceHistoryNavBtn.Background = accentLight;
                PriceHistoryNavBtn.Foreground = accentDark;
                break;
            case "Reports":
                ReportsNavBtn.Background = accentLight;
                ReportsNavBtn.Foreground = accentDark;
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
        var logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        logger.LogInfo("ImportJsonData_Click called");

        try
        {
            logger.LogInfo("Getting services from DI container");
            var dataService = ((App)Application.Current).Services.GetRequiredService<IGroceryDataService>();
            var dialogService = ((App)Application.Current).Services.GetRequiredService<IDialogService>();
            var jsonImportService = ((App)Application.Current).Services.GetRequiredService<JsonImportService>();
            logger.LogInfo("Services retrieved successfully");

            logger.LogInfo("Creating ImportDataViewModel");
            var viewModel = new ImportDataViewModel(dataService, dialogService, jsonImportService);
            logger.LogInfo("ImportDataViewModel created");

            logger.LogInfo("Creating ImportDataWindow");
            var window = new ImportDataWindow(viewModel) { Owner = this };
            logger.LogInfo("ImportDataWindow created, showing dialog");

            if (window.ShowDialog() == true)
            {
                logger.LogInfo("Import dialog closed with success result");
                // Refresh dashboard stats
                ViewModel.RefreshDashboard();
            }
            else
            {
                logger.LogInfo("Import dialog closed without success result");
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Error in ImportJsonData_Click", ex);
            MessageBox.Show($"Error opening import dialog: {ex.Message}\n\nCheck logs at: {logger.GetLogFilePath()}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ExportJsonData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var viewModel = ((App)Application.Current).Services.GetRequiredService<ExportDataViewModel>();
            var window = new ExportDataWindow(viewModel) { Owner = this };

            window.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening export dialog: {ex.Message}",
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

    private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double-click to maximize/restore
            MaximizeButton_Click(sender, e);
        }
        else
        {
            // Single-click to drag
            this.DragMove();
        }
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        if (this.WindowState == WindowState.Maximized)
        {
            this.WindowState = WindowState.Normal;
            MaximizeButton.Content = "□";
        }
        else
        {
            this.WindowState = WindowState.Maximized;
            MaximizeButton.Content = "❐";
        }
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}