using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class AddPlaceView : UserControl
{
    public PlaceViewModel ViewModel => (PlaceViewModel)DataContext;

    public AddPlaceView()
    {
        this.InitializeComponent();
    }
}