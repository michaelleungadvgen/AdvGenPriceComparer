using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class AddItemWindow : Window
{
    public AddItemViewModel ViewModel { get; }

    public AddItemWindow(AddItemViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private void SaveItem_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SaveItem())
        {
            DialogResult = true;
            Close();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
