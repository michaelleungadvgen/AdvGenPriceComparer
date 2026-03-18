using System.Windows.Input;
using AdvGenPriceComparer.WPF.ViewModels;
using Wpf.Ui.Controls;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for PriceAlertWindow.xaml
/// </summary>
public partial class PriceAlertWindow : FluentWindow
{
    private readonly PriceAlertViewModel _viewModel;

    public PriceAlertWindow(PriceAlertViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel ?? throw new System.ArgumentNullException(nameof(viewModel));
        DataContext = _viewModel;

        // Handle the RequestClose event from ViewModel
        _viewModel.RequestClose += (s, e) => Close();

        // Handle ESC key to close
        PreviewKeyDown += (s, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }
}
