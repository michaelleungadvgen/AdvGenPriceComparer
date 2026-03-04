using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for MLModelManagementWindow.xaml
/// Window for managing ML.NET category prediction models
/// </summary>
public partial class MLModelManagementWindow : Window
{
    public MLModelManagementWindow(MLModelManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
