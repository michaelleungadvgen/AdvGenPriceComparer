using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for WeeklySpecialsDigestWindow.xaml
/// </summary>
public partial class WeeklySpecialsDigestWindow : Window
{
    public WeeklySpecialsDigestWindow(WeeklySpecialsDigestViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
