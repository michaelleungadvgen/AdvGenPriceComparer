using System;
using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views
{
    /// <summary>
    /// Interaction logic for CloudSyncStatusWindow.xaml
    /// </summary>
    public partial class CloudSyncStatusWindow : Window
    {
        private readonly CloudSyncViewModel _viewModel;

        /// <summary>
        /// Creates a new instance of the CloudSyncStatusWindow.
        /// </summary>
        public CloudSyncStatusWindow(CloudSyncViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;

            // Set owner for proper dialog behavior
            Owner = System.Windows.Application.Current.MainWindow;

            // Handle window closing to cleanup
            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cleanup event subscriptions
            _viewModel.Cleanup();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Shows the cloud sync status window as a dialog.
        /// </summary>
        public static void ShowDialog(CloudSyncViewModel viewModel)
        {
            var window = new CloudSyncStatusWindow(viewModel);
            window.ShowDialog();
        }
    }
}
