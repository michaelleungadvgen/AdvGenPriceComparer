using AdvGenPriceComparer.WPF.ViewModels;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui.Controls;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for PriceDropNotificationsWindow.xaml
/// </summary>
public partial class PriceDropNotificationsWindow : FluentWindow
{
    private readonly PriceDropNotificationViewModel _viewModel;

    public PriceDropNotificationsWindow(PriceDropNotificationViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // Subscribe to close request
        _viewModel.RequestClose += OnRequestClose;

        // Add Escape key handler
        PreviewKeyDown += OnPreviewKeyDown;
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        Close();
    }

    private void OnPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        // Unsubscribe to prevent memory leaks
        _viewModel.RequestClose -= OnRequestClose;
        PreviewKeyDown -= OnPreviewKeyDown;
        base.OnClosed(e);
    }
}
