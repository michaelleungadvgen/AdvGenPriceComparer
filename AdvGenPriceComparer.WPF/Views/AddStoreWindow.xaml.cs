using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;

namespace AdvGenPriceComparer.WPF.Views;

public partial class AddStoreWindow : Window
{
    public AddStoreViewModel ViewModel { get; }

    public AddStoreWindow(AddStoreViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;
    }

    private void AddStore_Click(object sender, RoutedEventArgs e)
    {
        if (ViewModel.SaveStore())
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
