using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class StoresPage : Page
{
    public StoreViewModel ViewModel { get; }

    public StoresPage(StoreViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
