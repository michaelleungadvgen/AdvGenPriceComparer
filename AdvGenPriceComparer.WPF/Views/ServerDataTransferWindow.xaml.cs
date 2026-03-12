using System;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for ServerDataTransferWindow.xaml
/// </summary>
public partial class ServerDataTransferWindow : Window
{
    private readonly ServerDataTransferViewModel _viewModel;

    public ServerDataTransferWindow(ServerDataTransferViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // Subscribe to close request
        viewModel.RequestClose += (s, e) => Close();

        // Handle password box separately since it doesn't support direct binding
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Set initial password if available
        if (!string.IsNullOrEmpty(_viewModel.ApiKey))
        {
            ApiKeyPasswordBox.Password = _viewModel.ApiKey;
        }

        // Subscribe to password changes
        ApiKeyPasswordBox.PasswordChanged += (s, args) =>
        {
            _viewModel.ApiKey = ApiKeyPasswordBox.Password;
        };
    }
}
