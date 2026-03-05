using AdvGenPriceComparer.WPF.ViewModels;
using System.Windows;

namespace AdvGenPriceComparer.WPF.Views
{
    /// <summary>
    /// Interaction logic for IllusoryDiscountDetectionWindow.xaml
    /// </summary>
    public partial class IllusoryDiscountDetectionWindow : Window
    {
        private readonly IllusoryDiscountDetectionViewModel _viewModel;

        public IllusoryDiscountDetectionWindow(IllusoryDiscountDetectionViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
