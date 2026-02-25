using AdvGenPriceComparer.WPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for PriceDropNotificationsWindow.xaml
/// </summary>
public partial class PriceDropNotificationsWindow : FluentWindow
{
    public PriceDropNotificationsWindow(PriceDropNotificationViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
