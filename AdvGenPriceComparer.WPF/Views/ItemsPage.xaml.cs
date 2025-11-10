using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class ItemsPage : Page
{
    public ItemsPage(ItemViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
