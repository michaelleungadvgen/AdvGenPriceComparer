using System.Windows.Controls;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views
{
    public partial class CategoryPage : Page
    {
        public CategoryPage(CategoryViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
