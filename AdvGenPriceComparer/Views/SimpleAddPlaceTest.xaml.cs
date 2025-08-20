using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class SimpleAddPlaceTest : Page
{
    public SimpleAddPlaceTest()
    {
        this.InitializeComponent();
    }

    private async void TestBasicDialog_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Testing basic dialog...";
            
            var dialog = new ContentDialog
            {
                Title = "Basic Test Dialog",
                Content = "This is a basic test to verify ContentDialog works.",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot
            };
            
            var result = await dialog.ShowAsync();
            StatusText.Text = $"Basic dialog result: {result}";
        }
        catch (System.Exception ex)
        {
            StatusText.Text = $"Basic dialog error: {ex.Message}";
        }
    }

    private async void TestAddPlaceDirect_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Testing Add Place dialog (direct)...";
            
            var placeViewModel = new PlaceViewModel();
            var addPlaceView = new AddPlaceView
            {
                DataContext = placeViewModel
            };

            var dialog = new ContentDialog
            {
                Title = "Add Store/Supermarket (Direct)",
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
            StatusText.Text = $"Add Place dialog (direct) result: {result}";
            
            if (result == ContentDialogResult.Primary)
            {
                StatusText.Text += $" - Store Name: {placeViewModel.StoreName}";
            }
        }
        catch (System.Exception ex)
        {
            StatusText.Text = $"Add Place dialog (direct) error: {ex.Message}";
        }
    }

    private async void TestAddPlaceService_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Testing Add Place dialog (via service)...";
            
            var dialogService = App.Services.GetRequiredService<IDialogService>();
            var placeViewModel = new PlaceViewModel();
            
            var result = await dialogService.ShowAddPlaceDialogAsync(placeViewModel);
            StatusText.Text = $"Add Place dialog (service) result: {result}";
            
            if (result)
            {
                StatusText.Text += $" - Store Name: {placeViewModel.StoreName}";
            }
        }
        catch (System.Exception ex)
        {
            StatusText.Text = $"Add Place dialog (service) error: {ex.Message}";
        }
    }
}