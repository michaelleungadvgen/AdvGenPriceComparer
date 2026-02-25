using System.Windows;
using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class PriceHistoryPage : Page
{
    public PriceHistoryViewModel ViewModel { get; }

    public PriceHistoryPage(PriceHistoryViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }
}
