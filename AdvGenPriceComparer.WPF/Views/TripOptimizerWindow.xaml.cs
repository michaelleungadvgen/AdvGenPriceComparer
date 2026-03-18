using AdvGenPriceComparer.WPF.ViewModels;
using System;
using System.Windows;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for TripOptimizerWindow.xaml
/// </summary>
public partial class TripOptimizerWindow : Window
{
    private readonly TripOptimizerViewModel _viewModel;

    public TripOptimizerWindow(TripOptimizerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Subscribe to close request
        _viewModel.RequestClose += (s, e) => Close();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        Owner = System.Windows.Application.Current.MainWindow;
    }
}
