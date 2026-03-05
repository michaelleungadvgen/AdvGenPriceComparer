using System;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for ImportFromUrlWindow.xaml
/// </summary>
public partial class ImportFromUrlWindow : Window
{
    public ImportFromUrlWindow(ImportFromUrlViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.RequestClose += (s, e) => Close();
    }
}
