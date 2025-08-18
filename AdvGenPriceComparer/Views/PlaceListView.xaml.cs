using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using AdvGenPriceComparer.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Desktop.WinUI.Views
{
    public sealed partial class PlaceListView : Page
    {
        private readonly IDialogService _dialogService;
        private readonly IGroceryDataService _groceryDataService;
        private readonly INotificationService _notificationService;

        public PlaceListView()
        {
            this.InitializeComponent();
            _dialogService = App.Services.GetRequiredService<IDialogService>();
            _groceryDataService = App.Services.GetRequiredService<IGroceryDataService>();
            _notificationService = App.Services.GetRequiredService<INotificationService>();
            LoadStores();
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
                var placeViewModel = new PlaceViewModel();
                var result = await _dialogService.ShowAddPlaceDialogAsync(placeViewModel);
                
                if (result)
                {
                    var place = placeViewModel.CreatePlace();
                    var placeId = _groceryDataService.AddSupermarket(
                        place.Name,
                        place.Chain ?? string.Empty,
                        place.Suburb,
                        place.State ?? string.Empty,
                        place.Postcode ?? string.Empty
                    );

                    await _notificationService.ShowSuccessAsync("Store added successfully!");
                    LoadStores(); // Refresh the store list
                }
            }
            catch (System.Exception ex)
            {
                await _notificationService.ShowErrorAsync($"Error adding store: {ex.Message}");
            }
        }
    }
}