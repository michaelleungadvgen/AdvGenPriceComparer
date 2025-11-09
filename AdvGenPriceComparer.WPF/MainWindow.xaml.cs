using System;
using System.Linq;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Services;
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
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        ViewModel.Dispose();
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Items view
        MessageBox.Show("Items view - Coming soon!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void StoresNav_Click(object sender, RoutedEventArgs e)
    {
        // TODO: Navigate to Stores view
        MessageBox.Show("Stores view - Coming soon!", "Navigation", MessageBoxButton.OK, MessageBoxImage.Information);
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