using System.Windows;
using AdvGenPriceComparer.WPF.ViewModels;
using Microsoft.Win32;

namespace AdvGenPriceComparer.WPF.Views;

public partial class ImportDataWindow : Window
{
    public ImportDataViewModel ViewModel { get; }

    public ImportDataWindow(ImportDataViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = ViewModel;

        ViewModel.LoadStores();
    }

    private void BrowseFiles_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
            Title = "Select JSON Files",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            ViewModel.SetSelectedFiles(openFileDialog.FileNames);
        }
    }

    private void NewStore_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.OpenNewStoreDialog(this);
    }

    private void Next_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoToStep2();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoToStep1();
    }

    private async void Import_Click(object sender, RoutedEventArgs e)
    {
        await ViewModel.ImportData();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
