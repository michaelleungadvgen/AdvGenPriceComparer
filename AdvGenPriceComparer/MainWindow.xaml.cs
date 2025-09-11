using System;
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
            var importService = App.Services.GetRequiredService<JsonImportService>();
            var file = await importService.SelectJsonFileAsync(this);
            
            if (file != null)
            {
                var result = await importService.ImportJsonDataAsync(file);
                var notificationService = App.Services.GetRequiredService<INotificationService>();
                
                if (result.Success)
                {
                    await notificationService.ShowSuccessAsync(result.Message ?? "Data imported successfully!");
                    
                    // Refresh the items view if currently showing
                    if (ContentFrame.Content is Views.ItemListView)
                    {
                        ContentFrame.Navigate(typeof(Views.ItemListView));
                    }
                }
                else
                {
                    await notificationService.ShowErrorAsync($"Import failed: {result.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            var notificationService = App.Services.GetRequiredService<INotificationService>();
            await notificationService.ShowErrorAsync($"Error during import: {ex.Message}");
        }
    }
}
