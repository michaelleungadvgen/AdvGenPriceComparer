using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Desktop.WinUI.Services;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class TestAddPlaceView : Page
{
    private readonly IDialogService _dialogService;

    public TestAddPlaceView()
    {
        this.InitializeComponent();
        _dialogService = App.Services.GetRequiredService<IDialogService>();
    }

    private async void TestAddPlace_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            StatusText.Text = "Opening Add Place dialog...";
            
            var placeViewModel = new PlaceViewModel();
            var result = await _dialogService.ShowAddPlaceDialogAsync(placeViewModel);
            
            if (result)
            {
                StatusText.Text = $"Success! Store added: {placeViewModel.StoreName}";
            }
            else
            {
                StatusText.Text = "Dialog was cancelled.";
            }
        }
        catch (System.Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }
}