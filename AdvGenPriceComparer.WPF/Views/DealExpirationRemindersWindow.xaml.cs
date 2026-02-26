using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for DealExpirationRemindersWindow.xaml
/// </summary>
public partial class DealExpirationRemindersWindow : Window
{
    public DealExpirationRemindersWindow(DealExpirationReminderViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
