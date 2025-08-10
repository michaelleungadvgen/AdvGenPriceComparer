using Microsoft.UI.Xaml.Controls;
using AdvGenPriceComparer.Desktop.WinUI.ViewModels;

namespace AdvGenPriceComparer.Desktop.WinUI.Views;

public sealed partial class AddItemView : UserControl
{
    public ItemViewModel ViewModel => (ItemViewModel)DataContext;

    public AddItemView()
    {
        this.InitializeComponent();
    }
}