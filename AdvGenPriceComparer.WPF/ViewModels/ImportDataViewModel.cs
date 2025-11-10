using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class ImportDataViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private readonly string _dbPath;

    private int _currentStep = 1;
    private string[] _selectedFilePaths = Array.Empty<string>();
    private Place? _selectedStore;
    private DateTime _catalogueDate = DateTime.Today;
    private string _importStatus = "Ready to import...";
    private bool _isImporting = false;
    private bool _importCompleted = false;

    public ImportDataViewModel(IGroceryDataService dataService, IDialogService dialogService, string dbPath)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _dbPath = dbPath;

        Stores = new ObservableCollection<Place>();
        SelectedFiles = new ObservableCollection<string>();
    }

    public ObservableCollection<Place> Stores { get; }
    public ObservableCollection<string> SelectedFiles { get; }

    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(Step1Visibility));
                OnPropertyChanged(nameof(Step2Visibility));
            }
        }
    }

    public Visibility Step1Visibility => CurrentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step2Visibility => CurrentStep == 2 ? Visibility.Visible : Visibility.Collapsed;

    public string SelectedFilesText => _selectedFilePaths.Length > 0
        ? $"{_selectedFilePaths.Length} file(s) selected"
        : "No files selected";

    public int FileCount => _selectedFilePaths.Length;

    public Place? SelectedStore
    {
        get => _selectedStore;
        set => SetProperty(ref _selectedStore, value);
    }

    public DateTime CatalogueDate
    {
        get => _catalogueDate;
        set => SetProperty(ref _catalogueDate, value);
    }

    public string ImportStatus
    {
        get => _importStatus;
        set => SetProperty(ref _importStatus, value);
    }

    public bool IsImporting
    {
        get => _isImporting;
        set
        {
            if (SetProperty(ref _isImporting, value))
            {
                OnPropertyChanged(nameof(ProgressVisibility));
                OnPropertyChanged(nameof(CanImport));
                OnPropertyChanged(nameof(CanGoBack));
            }
        }
    }

    public Visibility ProgressVisibility => IsImporting ? Visibility.Visible : Visibility.Collapsed;

    public bool CanImport => !IsImporting && !_importCompleted;
    public bool CanGoBack => !IsImporting;

    public string ImportButtonText => _importCompleted ? "Close" : "Import";

    public void LoadStores()
    {
        Stores.Clear();
        var stores = _dataService.GetAllPlaces().OrderBy(p => p.Name);
        foreach (var store in stores)
        {
            Stores.Add(store);
        }
    }

    public void SetSelectedFiles(string[] filePaths)
    {
        _selectedFilePaths = filePaths;
        SelectedFiles.Clear();

        foreach (var path in filePaths)
        {
            SelectedFiles.Add(Path.GetFileName(path));
        }

        OnPropertyChanged(nameof(SelectedFilesText));
        OnPropertyChanged(nameof(FileCount));
    }

    public void OpenNewStoreDialog(Window owner)
    {
        var viewModel = new AddStoreViewModel(_dataService, _dialogService);
        var window = new AddStoreWindow(viewModel) { Owner = owner };

        if (window.ShowDialog() == true)
        {
            LoadStores();
            // Select the newly added store
            SelectedStore = Stores.FirstOrDefault(s => s.Name == viewModel.StoreName);
        }
    }

    public void GoToStep2()
    {
        // Validate Step 1
        if (_selectedFilePaths.Length == 0)
        {
            _dialogService.ShowWarning("Please select at least one JSON file.");
            return;
        }

        if (SelectedStore == null)
        {
            _dialogService.ShowWarning("Please select a store.");
            return;
        }

        CurrentStep = 2;
        ImportStatus = "Ready to import...\n\nClick 'Import' to begin.";
    }

    public void GoToStep1()
    {
        CurrentStep = 1;
    }

    public async Task ImportData()
    {
        if (_importCompleted)
        {
            // Close button clicked
            if (Application.Current.Windows.OfType<ImportDataWindow>().FirstOrDefault() is { } window)
            {
                window.DialogResult = true;
                window.Close();
            }
            return;
        }

        IsImporting = true;
        var statusBuilder = new StringBuilder();
        statusBuilder.AppendLine($"Starting import...");
        statusBuilder.AppendLine($"Store: {SelectedStore?.Name}");
        statusBuilder.AppendLine($"Date: {CatalogueDate:dd/MM/yyyy}");
        statusBuilder.AppendLine($"Files: {_selectedFilePaths.Length}");
        statusBuilder.AppendLine();

        ImportStatus = statusBuilder.ToString();

        try
        {
            var dbService = new DatabaseService(_dbPath);
            var importService = new JsonImportService(dbService);

            int totalImported = 0;
            int fileCount = 0;

            foreach (var filePath in _selectedFilePaths)
            {
                fileCount++;
                statusBuilder.AppendLine($"[{fileCount}/{_selectedFilePaths.Length}] Processing: {Path.GetFileName(filePath)}");
                ImportStatus = statusBuilder.ToString();

                await Task.Delay(100); // Allow UI to update

                try
                {
                    var result = await Task.Run(() => importService.ImportFromFile(filePath));

                    statusBuilder.AppendLine($"  ✓ Imported {result.ItemsProcessed} items, {result.PriceRecordsCreated} price records");
                    totalImported += result.ItemsProcessed;

                    if (result.Errors.Any())
                    {
                        statusBuilder.AppendLine($"  ⚠ {result.Errors.Count} errors/warnings");
                        foreach (var error in result.Errors.Take(3))
                        {
                            statusBuilder.AppendLine($"    - {error}");
                        }
                        if (result.Errors.Count > 3)
                        {
                            statusBuilder.AppendLine($"    ... and {result.Errors.Count - 3} more");
                        }
                    }
                }
                catch (Exception ex)
                {
                    statusBuilder.AppendLine($"  ✗ Error: {ex.Message}");
                }

                statusBuilder.AppendLine();
                ImportStatus = statusBuilder.ToString();
            }

            statusBuilder.AppendLine("═══════════════════════════════");
            statusBuilder.AppendLine($"Import completed!");
            statusBuilder.AppendLine($"Total items imported: {totalImported}");
            statusBuilder.AppendLine("═══════════════════════════════");

            ImportStatus = statusBuilder.ToString();

            _dialogService.ShowSuccess($"Successfully imported {totalImported} items from {_selectedFilePaths.Length} file(s)!");

            _importCompleted = true;
            OnPropertyChanged(nameof(ImportButtonText));
            OnPropertyChanged(nameof(CanImport));
        }
        catch (Exception ex)
        {
            statusBuilder.AppendLine();
            statusBuilder.AppendLine($"✗ Fatal error: {ex.Message}");
            ImportStatus = statusBuilder.ToString();

            _dialogService.ShowError($"Import failed: {ex.Message}");
        }
        finally
        {
            IsImporting = false;
        }
    }
}
