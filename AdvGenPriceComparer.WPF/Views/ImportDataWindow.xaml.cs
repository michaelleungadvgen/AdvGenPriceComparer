using System;
using System.Windows;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;

namespace AdvGenPriceComparer.WPF.Views;

public partial class ImportDataWindow : Window
{
    public ImportDataViewModel ViewModel { get; }
    private readonly ILoggerService _logger;

    public ImportDataWindow(ImportDataViewModel viewModel)
    {
        _logger = ((App)Application.Current).Services.GetRequiredService<ILoggerService>();
        _logger.LogInfo("ImportDataWindow constructor called");

        try
        {
            _logger.LogInfo("Calling InitializeComponent");
            InitializeComponent();
            _logger.LogInfo("InitializeComponent completed");

            ViewModel = viewModel;
            DataContext = ViewModel;
            _logger.LogInfo("DataContext set to ViewModel");

            _logger.LogInfo("Loading stores");
            ViewModel.LoadStores();
            _logger.LogInfo($"Stores loaded: {ViewModel.Stores.Count} stores found");
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in ImportDataWindow constructor", ex);
            throw;
        }
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

    private void NextToStep3_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.GoToStep3();
    }

    private void BackToStep2_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CurrentStep = 2;
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

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _logger.LogInfo("Window_Loaded event fired - window is now visible");
        _logger.LogInfo($"Window ActualHeight: {ActualHeight}, ActualWidth: {ActualWidth}");
        _logger.LogInfo($"Window IsVisible: {IsVisible}, IsLoaded: {IsLoaded}");
        _logger.LogInfo($"Window Left: {Left}, Top: {Top}");
    }

    private void SameAsRecordDate_Click(object sender, RoutedEventArgs e)
    {
        // Set the expiry date to match the catalogue date
        ViewModel.ExpiryDate = ViewModel.CatalogueDate;
    }
}
