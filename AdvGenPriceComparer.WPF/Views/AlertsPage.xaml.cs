using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class AlertsPage : Page
{
    public AlertsPage(AlertViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
