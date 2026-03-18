using System;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for StaticPeerConfigWindow.xaml
/// </summary>
public partial class StaticPeerConfigWindow : Window
{
    private readonly StaticPeerConfigViewModel _viewModel;

    public StaticPeerConfigWindow(StaticPeerConfigViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to the RequestClose event
        _viewModel.RequestClose += (sender, args) =>
        {
            DialogResult = true;
            Close();
        };

        Owner = System.Windows.Application.Current.MainWindow;
    }
}
