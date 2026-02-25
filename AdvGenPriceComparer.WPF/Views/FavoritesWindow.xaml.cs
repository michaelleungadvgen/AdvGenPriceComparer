using AdvGenPriceComparer.WPF.ViewModels;
using System;
using System.Windows;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for FavoritesWindow.xaml
/// </summary>
public partial class FavoritesWindow : Window
{
    private readonly FavoritesViewModel _viewModel;

    public FavoritesWindow(FavoritesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        _viewModel.Dispose();
    }
}
