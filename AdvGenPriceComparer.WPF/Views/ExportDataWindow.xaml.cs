using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class ExportDataWindow : Window
{
    public ExportDataViewModel ViewModel { get; }

    public ExportDataWindow(ExportDataViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.ExportCompleted += OnExportCompleted;
    }

    private void OnExportCompleted(object? sender, bool success)
    {
        if (success)
        {
            DialogResult = true;
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    protected override void OnClosed(System.EventArgs e)
    {
        ViewModel.ExportCompleted -= OnExportCompleted;
        base.OnClosed(e);
    }
}
