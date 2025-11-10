using System;
using System.Linq;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Desktop.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        this.Closed += OnClosed;
        
        // Subscribe to store added event
        ViewModel.OnStoreAdded += () => ContentFrame.Navigate(typeof(Views.PlaceListView));
        
        // Initialize services with XamlRoot when content is loaded
        this.Activated += (s, e) => InitializeServices();
    }
    
    private void InitializeServices()
    {
        try
        {
            if (this.Content?.XamlRoot != null)
            {
                var dialogService = App.Services.GetRequiredService<IDialogService>() as SimpleDialogService;
                dialogService?.Initialize(this.Content.XamlRoot);
                
                var notificationService = App.Services.GetRequiredService<INotificationService>() as SimpleNotificationService;
                notificationService?.Initialize(this.Content.XamlRoot);
                
                System.Diagnostics.Debug.WriteLine("Services initialized with XamlRoot");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("XamlRoot not available for service initialization");
            }
        }
        catch (System.Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error initializing services: {ex.Message}");
        }
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        ViewModel.Dispose();
    }

    private void ItemsNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.ItemListView));
    }

    private void StoresNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.PlaceListView));
    }

    private void CategoriesNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.CategoryListView));
    }

    private void ReportsNav_Click(object sender, RoutedEventArgs e)
    {
        ContentFrame.Navigate(typeof(Views.ReportView));
    }

    private async void GenerateDemoData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var demoDataService = App.Services.GetRequiredService<DemoDataService>();
            demoDataService.GenerateDemoData();
            
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            await notificationService.ShowSuccessAsync("Demo data generated successfully! Check the Reports page to see charts with data.");
        }
        catch (Exception ex)
        {
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            await notificationService.ShowErrorAsync($"Error generating demo data: {ex.Message}");
        }
    }

    private void AddItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AddItemCommand.CanExecute(null))
            ViewModel.AddItemCommand.Execute(null);
    }

    private void AddPlace_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.AddPlaceCommand.CanExecute(null))
            ViewModel.AddPlaceCommand.Execute(null);
    }

    private async void ImportJsonData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Create file picker
            var filePicker = new Windows.Storage.Pickers.FileOpenPicker
            {
                ViewMode = Windows.Storage.Pickers.PickerViewMode.List,
                SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary
            };
            filePicker.FileTypeFilter.Add(".json");

            // Initialize the file picker with the window handle
            var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(filePicker, hwnd);

            // Show file picker
            var file = await filePicker.PickSingleFileAsync();

            if (file != null)
            {
                var notificationService = App.Services.GetRequiredService<INotificationService>();
                var importService = App.Services.GetRequiredService<AdvGenPriceComparer.Data.LiteDB.Services.JsonImportService>();

                // Show loading notification
                await notificationService.ShowInfoAsync($"Importing data from {file.Name}...");

                // Import the file
                var result = importService.ImportFromFile(file.Path);

                if (result.Success)
                {
                    await notificationService.ShowSuccessAsync(result.Message ?? "Data imported successfully!");

                    // Refresh dashboard stats
                    ViewModel.RefreshDashboard();

                    // Refresh the items view if currently showing
                    if (ContentFrame.Content is Views.ItemListView)
                    {
                        ContentFrame.Navigate(typeof(Views.ItemListView));
                    }
                }
                else
                {
                    await notificationService.ShowErrorAsync($"Import failed: {result.ErrorMessage}");

                    // Show detailed errors if any
                    if (result.Errors.Any())
                    {
                        var errorDetails = string.Join("\n", result.Errors.Take(5));
                        System.Diagnostics.Debug.WriteLine($"Import errors:\n{errorDetails}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            await notificationService.ShowErrorAsync($"Error during import: {ex.Message}");
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private async void About_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            await notificationService.ShowInfoAsync(
                "AdvGen Price Comparer v1.0\n\n" +
                "An AI-powered grocery price tracking and comparison tool.\n\n" +
                "Features:\n" +
                "• Import price data from JSON files\n" +
                "• Track prices across multiple stores\n" +
                "• Compare prices and find best deals\n" +
                "• Generate analytics and reports\n\n" +
                "© 2025 AdvGen. All rights reserved."
            );
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error showing about dialog: {ex.Message}");
        }
    }
}
