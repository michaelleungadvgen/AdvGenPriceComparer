using AdvGenPriceComparer.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace AdvGenPriceComparer.WPF.Views
{
    /// <summary>
    /// Interaction logic for ReportsPage.xaml
    /// </summary>
    public partial class ReportsPage : Page
    {
        public ReportsPage(ReportsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Charts are loaded by bindings in MainWindow style
            // This page focuses on statistics and best deals display
        }
    }
}
