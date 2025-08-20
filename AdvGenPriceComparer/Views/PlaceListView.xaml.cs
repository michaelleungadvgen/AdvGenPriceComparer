using System;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using AdvGenPriceComparer.Desktop.WinUI.Views;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class PlaceListView : Page
    {
        private IDialogService? _dialogService;
        private IGroceryDataService? _groceryDataService;
        private INotificationService? _notificationService;

        public PlaceListView()
        {
            this.InitializeComponent();
            InitializeServices();
            LoadStores();
        }

        private void InitializeServices()
        {
            try
            {
                _dialogService = App.Services.GetRequiredService<IDialogService>();
                _groceryDataService = App.Services.GetRequiredService<IGroceryDataService>();
                _notificationService = App.Services.GetRequiredService<INotificationService>();
            }
            catch (System.Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error initializing services: {ex.Message}");
            }
        }

        private void LoadStores()
        {
            // TODO: Load stores from database
            // For now, this is a placeholder that will be connected to the database service
        }

        private async void AddStore_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_dialogService == null)
                    InitializeServices();
                
                if (_dialogService == null)
                {
                    // Fallback: create dialog directly
                    await ShowAddPlaceDialogDirect();
                    return;
                }
                
                var placeViewModel = new PlaceViewModel();
                var result = await _dialogService.ShowAddPlaceDialogAsync(placeViewModel);
                
                if (result)
                {
                    var place = placeViewModel.CreatePlace();
                    var placeId = _groceryDataService?.AddSupermarket(
                        place.Name,
                        place.Chain ?? string.Empty,
                        place.Suburb,
                        place.State ?? string.Empty,
                        place.Postcode ?? string.Empty
                    );

                    if (_notificationService != null)
                        await _notificationService.ShowSuccessAsync("Store added successfully!");
                    LoadStores();
                }
            }
            catch (Exception ex)
            {
                if (_notificationService != null)
                    await _notificationService.ShowErrorAsync($"Error adding store: {ex.Message}");
            }
        }
        
        private async System.Threading.Tasks.Task ShowAddPlaceDialogDirect()
        {
            try
            {
                var placeViewModel = new PlaceViewModel();
                var addPlaceView = new AddPlaceView
                {
                    DataContext = placeViewModel
                };

                var dialog = new ContentDialog
                {
                    Title = "Add Store/Supermarket",
                    Content = addPlaceView,
                    PrimaryButtonText = "Add Store",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };

                // Bind validation
                placeViewModel.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(PlaceViewModel.IsValid))
                    {
                        dialog.IsPrimaryButtonEnabled = placeViewModel.IsValid;
                    }
                };
                dialog.IsPrimaryButtonEnabled = placeViewModel.IsValid;

                var result = await dialog.ShowAsync();
                
                if (result == ContentDialogResult.Primary)
                {
                    var place = placeViewModel.CreatePlace();
                    var placeId = _groceryDataService?.AddSupermarket(
                        place.Name,
                        place.Chain ?? string.Empty,
                        place.Suburb,
                        place.State ?? string.Empty,
                        place.Postcode ?? string.Empty
                    );

                    if (_notificationService != null)
                        await _notificationService.ShowSuccessAsync("Store added successfully!");
                    LoadStores();
                }
            }
            catch (Exception ex)
            {
                if (_notificationService != null)
                    await _notificationService.ShowErrorAsync($"Error: {ex.Message}");
            }
        }

        private async void TestDialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Test Dialog",
                    Content = "This is a test dialog to verify basic dialog functionality works.",
                    PrimaryButtonText = "OK",
                    CloseButtonText = "Cancel",
                    XamlRoot = this.XamlRoot
                };
                
                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                if (_notificationService != null)
                    await _notificationService.ShowErrorAsync($"Test dialog error: {ex.Message}");
            }
        }
    }
}