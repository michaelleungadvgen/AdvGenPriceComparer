using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Models;
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

        try
        {
            foreach (var filePath in _selectedFilePaths)
            {
                await Task.Run(() =>
                {
                    var jsonContent = File.ReadAllText(filePath);
                    var products = JsonSerializer.Deserialize<List<ColesProduct>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (products != null)
                    {
                        foreach (var product in products)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                var previewItem = CreatePreviewItem(product);
                                PreviewItems.Add(previewItem);
                            });
                        }
                    }
                });
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Error loading preview: {ex.Message}");
        }
        finally
        {
            IsImporting = false;
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
            int totalItemsCreated = 0;
            int totalPriceRecordsCreated = 0;
            int itemCount = 0;

            foreach (var previewItem in PreviewItems)
            {
                itemCount++;

                if (itemCount % 10 == 0) // Update UI every 10 items
                {
                    statusBuilder.AppendLine($"Processing: {itemCount}/{PreviewItems.Count}");
                    ImportStatus = statusBuilder.ToString();
                    await Task.Delay(10); // Allow UI to update
                }

                try
                {
                    var product = previewItem.RawProductData as ColesProduct;
                    if (product == null) continue;

                    string itemId;

                    if (previewItem.AddPriceRecordOnly && !string.IsNullOrEmpty(previewItem.ExistingItemId))
                    {
                        // Add price record to existing item
                        itemId = previewItem.ExistingItemId;
                    }
                    else
                    {
                        // Create new item
                        var item = new Item
                        {
                            Name = product.ProductName,
                            Description = product.Description,
                            Brand = product.Brand,
                            Category = product.Category,
                            PackageSize = product.Description,
                            IsActive = true,
                            DateAdded = DateTime.UtcNow,
                            LastUpdated = DateTime.UtcNow,
                            ExtraInformation = new Dictionary<string, string>
                            {
                                ["ProductID"] = product.ProductID,
                                ["Store"] = SelectedStore?.Chain ?? "Unknown"
                            }
                        };

                        if (!string.IsNullOrEmpty(product.UnitPrice))
                        {
                            item.ExtraInformation["UnitPrice"] = product.UnitPrice;
                        }

                        itemId = _dataService.Items.Add(item);
                        totalItemsCreated++;
                    }

                    // Create price record
                    var price = previewItem.Price;
                    var originalPrice = previewItem.OriginalPrice;
                    var savings = originalPrice.HasValue ? originalPrice.Value - price : 0;

                    var saleDescription = savings > 0 ? $"Save ${savings:F2}" : null;
                    if (!string.IsNullOrEmpty(product.SpecialType))
                    {
                        saleDescription = product.SpecialType;
                    }

                    var priceRecord = new PriceRecord
                    {
                        ItemId = itemId,
                        PlaceId = SelectedStore!.Id!,
                        Price = price,
                        OriginalPrice = originalPrice,
                        IsOnSale = previewItem.IsOnSale,
                        SaleDescription = saleDescription,
                        DateRecorded = CatalogueDate,
                        ValidFrom = CatalogueDate,
                        ValidTo = CatalogueDate.AddDays(7),
                        Source = "Catalogue",
                        CatalogueDate = CatalogueDate
                    };

                    _dataService.PriceRecords.Add(priceRecord);
                    totalPriceRecordsCreated++;
                }
                catch (Exception ex)
                {
                    statusBuilder.AppendLine($"  ✗ Error processing {previewItem.ProductName}: {ex.Message}");
                }
            }

            statusBuilder.AppendLine();
            statusBuilder.AppendLine("═══════════════════════════════");
            statusBuilder.AppendLine($"Import completed!");
            statusBuilder.AppendLine($"New items created: {totalItemsCreated}");
            statusBuilder.AppendLine($"Price records added: {totalPriceRecordsCreated}");
            statusBuilder.AppendLine("═══════════════════════════════");

            ImportStatus = statusBuilder.ToString();

            _dialogService.ShowSuccess($"Successfully imported {totalItemsCreated} new items and {totalPriceRecordsCreated} price records!");

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

/// <summary>
/// Model for individual Coles product from JSON
/// </summary>
public class ColesProduct
{
    public string ProductID { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string Price { get; set; } = string.Empty;
    public string? OriginalPrice { get; set; }
    public string? Savings { get; set; }
    public string? UnitPrice { get; set; }
    public string? SpecialType { get; set; }
}
