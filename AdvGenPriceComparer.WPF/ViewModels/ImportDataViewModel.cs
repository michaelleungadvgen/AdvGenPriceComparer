using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.Views;

// Use WPF Models ImportPreviewItem and Service ColesProduct
using ImportPreviewItem = AdvGenPriceComparer.WPF.Models.ImportPreviewItem;
using ColesProduct = AdvGenPriceComparer.Data.LiteDB.Services.ColesProduct;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for importing data using JsonImportService
/// </summary>
public class ImportDataViewModel : ViewModelBase
{
    private readonly IGroceryDataService _dataService;
    private readonly IDialogService _dialogService;
    private readonly JsonImportService _jsonImportService;
    private readonly string _dbPath;
    private CancellationTokenSource? _cancellationTokenSource;

    private int _currentStep = 1;
    private string[] _selectedFilePaths = Array.Empty<string>();
    private Place? _selectedStore;
    private DateTime _catalogueDate = DateTime.Today;
    private DateTime? _expiryDate = null; // Optional expiry date for discounts
    private string _importStatus = "Ready to import...";
    private bool _isImporting = false;
    private bool _importCompleted = false;

    public ImportDataViewModel(IGroceryDataService dataService, IDialogService dialogService, string dbPath)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _dbPath = dbPath;
        _jsonImportService = new JsonImportService(new DatabaseService(dbPath));

        Stores = new ObservableCollection<Place>();
        SelectedFiles = new ObservableCollection<string>();
        PreviewItems = new ObservableCollection<ImportPreviewItem>();
    }

    /// <summary>
    /// Constructor with JsonImportService injection (preferred)
    /// </summary>
    public ImportDataViewModel(IGroceryDataService dataService, IDialogService dialogService, JsonImportService jsonImportService)
    {
        _dataService = dataService;
        _dialogService = dialogService;
        _jsonImportService = jsonImportService;
        _dbPath = string.Empty;

        Stores = new ObservableCollection<Place>();
        SelectedFiles = new ObservableCollection<string>();
        PreviewItems = new ObservableCollection<ImportPreviewItem>();
    }

    public ObservableCollection<Place> Stores { get; }
    public ObservableCollection<string> SelectedFiles { get; }
    public ObservableCollection<ImportPreviewItem> PreviewItems { get; }

    public int CurrentStep
    {
        get => _currentStep;
        set
        {
            if (SetProperty(ref _currentStep, value))
            {
                OnPropertyChanged(nameof(Step1Visibility));
                OnPropertyChanged(nameof(Step2Visibility));
                OnPropertyChanged(nameof(Step3Visibility));
            }
        }
    }

    public Visibility Step1Visibility => CurrentStep == 1 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step2Visibility => CurrentStep == 2 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility Step3Visibility => CurrentStep == 3 ? Visibility.Visible : Visibility.Collapsed;

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
        set 
        {
            if (SetProperty(ref _catalogueDate, value))
            {
                // When catalogue date changes and expiry date is not set or was same as old catalogue date,
                // update expiry date to match new catalogue date
                if (_expiryDate == null || _expiryDate == value)
                {
                    ExpiryDate = value;
                }
            }
        }
    }

    public DateTime? ExpiryDate
    {
        get => _expiryDate;
        set => SetProperty(ref _expiryDate, value);
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

    public async void GoToStep2()
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
        await LoadPreviewData();
    }

    public void GoToStep1()
    {
        CurrentStep = 1;
    }

    public void GoToStep3()
    {
        CurrentStep = 3;
        ImportStatus = "Ready to import...\n\nClick 'Import' to begin.";
    }

    private async Task LoadPreviewData()
    {
        PreviewItems.Clear();
        IsImporting = true;
        _cancellationTokenSource = new CancellationTokenSource();
        var allErrors = new List<ImportError>();

        try
        {
            foreach (var filePath in _selectedFilePaths)
            {
                // Use JsonImportService for preview - get raw products and errors
                var (products, errors) = await _jsonImportService.PreviewImportAsync(filePath, _cancellationTokenSource.Token);
                
                allErrors.AddRange(errors);
                
                foreach (var product in products)
                {
                    // Create preview item from product
                    var previewItem = CreatePreviewItem(product);
                    PreviewItems.Add(previewItem);
                }
            }

            // Show validation warnings if any
            if (allErrors.Count > 0)
            {
                var warningMessage = $"Import preview completed with {allErrors.Count} warnings:\n\n" +
                    string.Join("\n", allErrors.Take(5).Select(e => $"• {e.Message}"));
                
                if (allErrors.Count > 5)
                {
                    warningMessage += $"\n\n... and {allErrors.Count - 5} more warnings";
                }
                
                _dialogService.ShowWarning(warningMessage);
            }
        }
        catch (OperationCanceledException)
        {
            // Preview was cancelled
            ImportStatus = "Preview cancelled by user";
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error loading preview: {ex.Message}");
        }
        finally
        {
            IsImporting = false;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }
    }

    private ImportPreviewItem CreatePreviewItem(ColesProduct product)
    {
        var price = ParsePrice(product.Price);
        var originalPrice = ParsePrice(product.OriginalPrice);
        var savings = ParsePrice(product.Savings);

        // Try to find existing item by name and brand
        var existingItems = _dataService.Items.SearchByName(product.ProductName)
            .Where(i => i.Brand == product.Brand)
            .ToList();

        var previewItem = new ImportPreviewItem
        {
            ProductName = product.ProductName,
            Category = product.Category ?? string.Empty,
            Brand = product.Brand ?? string.Empty,
            Price = price,
            OriginalPrice = originalPrice > 0 ? originalPrice : null,
            IsOnSale = savings > 0,
            RawProductData = product
        };

        if (existingItems.Any())
        {
            var existingItem = existingItems.First();
            previewItem.ExistingItemId = existingItem.Id;
            previewItem.ExistingItemInfo = $"Match: {existingItem.Name}";
            if (!string.IsNullOrEmpty(existingItem.PackageSize))
            {
                previewItem.ExistingItemInfo += $" {existingItem.PackageSize}";
            }
            if (!string.IsNullOrEmpty(existingItem.Category))
            {
                previewItem.ExistingItemInfo += $" - {existingItem.Category}";
            }
            previewItem.AddPriceRecordOnly = true; // Default to adding price record
        }
        else
        {
            previewItem.AddPriceRecordOnly = false; // Default to creating new item
        }

        return previewItem;
    }

    /// <summary>
    /// Cancel ongoing import or preview operation
    /// </summary>
    public void CancelOperation()
    {
        _cancellationTokenSource?.Cancel();
    }

    private decimal ParsePrice(string? priceString)
    {
        if (string.IsNullOrEmpty(priceString))
            return 0;

        var cleanPrice = priceString.Replace("$", "").Trim();
        if (decimal.TryParse(cleanPrice, out var price))
            return price;

        return 0;
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

        if (SelectedStore?.Id == null)
        {
            _dialogService.ShowWarning("Please select a store before importing.");
            return;
        }

        IsImporting = true;
        var statusBuilder = new StringBuilder();
        statusBuilder.AppendLine($"Starting import...");
        statusBuilder.AppendLine($"Store: {SelectedStore?.Name}");
        statusBuilder.AppendLine($"Date: {CatalogueDate:dd/MM/yyyy}");
        statusBuilder.AppendLine($"Items: {PreviewItems.Count}");
        statusBuilder.AppendLine();

        ImportStatus = statusBuilder.ToString();

        try
        {
            // Create progress reporter for UI updates
            var progress = new Progress<ImportProgress>(p =>
            {
                statusBuilder.AppendLine($"Processing: {p.ProcessedItems}/{p.TotalItems} - {p.CurrentItem}");
                ImportStatus = statusBuilder.ToString();
            });

            // Build mapping of products to existing items (for items where we only want to add price records)
            var existingItemMappings = PreviewItems
                .Where(p => p.AddPriceRecordOnly && !string.IsNullOrEmpty(p.ExistingItemId))
                .ToDictionary(
                    p => (p.RawProductData as ColesProduct)?.GetProductId() ?? Guid.NewGuid().ToString(),
                    p => p.ExistingItemId!
                );

            // Get the list of ColesProducts from preview items
            var products = PreviewItems
                .Select(p => p.RawProductData as ColesProduct)
                .Where(p => p != null)
                .Cast<ColesProduct>()
                .ToList();

            // Use JsonImportService to import the products
            var result = await Task.Run(() => 
                _jsonImportService.ImportColesProducts(products, SelectedStore!.Id!, CatalogueDate, existingItemMappings, progress, ExpiryDate));

            statusBuilder.AppendLine();
            statusBuilder.AppendLine("═══════════════════════════════");
            statusBuilder.AppendLine($"Import completed!");
            statusBuilder.AppendLine($"New items created: {result.ItemsProcessed}");
            statusBuilder.AppendLine($"Price records added: {result.PriceRecordsCreated}");
            if (result.Errors.Count > 0)
            {
                statusBuilder.AppendLine($"Errors: {result.Errors.Count}");
                foreach (var error in result.Errors.Take(5))
                {
                    statusBuilder.AppendLine($"  ✗ {error}");
                }
                if (result.Errors.Count > 5)
                {
                    statusBuilder.AppendLine($"  ... and {result.Errors.Count - 5} more");
                }
            }
            statusBuilder.AppendLine("═══════════════════════════════");

            ImportStatus = statusBuilder.ToString();

            if (result.Success)
            {
                _dialogService.ShowSuccess($"Successfully imported {result.ItemsProcessed} new items and {result.PriceRecordsCreated} price records!");
            }
            else
            {
                _dialogService.ShowWarning($"Import completed with {result.Errors.Count} errors. {result.ItemsProcessed} items created.");
            }

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
