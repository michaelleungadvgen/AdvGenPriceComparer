using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.Views;

/// <summary>
/// Interaction logic for ShoppingListWindow.xaml
/// </summary>
public partial class ShoppingListWindow : Window
{
    public ShoppingListWindow(ShoppingListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void ListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is ShoppingList list)
        {
            if (DataContext is ShoppingListViewModel vm)
            {
                vm.SelectListCommand.Execute(list);
            }
        }
    }

    private void NewItemName_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is ShoppingListViewModel vm && vm.AddItemCommand.CanExecute(null))
            {
                vm.AddItemCommand.Execute(null);
            }
        }
    }
}
