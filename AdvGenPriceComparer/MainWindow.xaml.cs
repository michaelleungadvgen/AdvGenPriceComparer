using AdvGenPriceComparer.Desktop.WinUI.ViewModels;
using Microsoft.UI.Xaml;

namespace AdvGenPriceComparer.Desktop.WinUI;

public sealed partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        this.Closed += OnClosed;
    }

    private void OnClosed(object sender, WindowEventArgs e)
    {
        ViewModel.Dispose();
    }
}
