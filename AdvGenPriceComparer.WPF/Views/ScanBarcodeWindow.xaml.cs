using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;
using Wpf.Ui.Controls;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for ScanBarcodeWindow.xaml
/// </summary>
public partial class ScanBarcodeWindow : FluentWindow
{
    public ScanBarcodeWindow(ScanBarcodeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
