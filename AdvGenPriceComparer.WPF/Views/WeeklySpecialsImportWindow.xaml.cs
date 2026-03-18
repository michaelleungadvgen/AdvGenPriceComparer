using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for WeeklySpecialsImportWindow.xaml
/// </summary>
public partial class WeeklySpecialsImportWindow : Window
{
    public WeeklySpecialsImportWindow(WeeklySpecialsImportViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        
        // Subscribe to close request
        viewModel.RequestClose += (s, e) => Close();
    }
}
