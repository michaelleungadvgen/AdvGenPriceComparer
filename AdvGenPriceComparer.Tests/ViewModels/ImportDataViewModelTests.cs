using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

/// <summary>
/// Unit tests for ImportDataViewModel
/// </summary>
public class ImportDataViewModelTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly TestGroceryDataService _dataService;
    private readonly TestDialogService _dialogService;
    private readonly DatabaseService _dbService;
    private readonly JsonImportService _jsonImportService;

    public ImportDataViewModelTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_import_vm_{Guid.NewGuid():N}.db");
        _dataService = new TestGroceryDataService();
        _dialogService = new TestDialogService();
        _dbService = new DatabaseService(_testDbPath);
        var itemRepo = new ItemRepository(_dbService);
        var placeRepo = new PlaceRepository(_dbService);
        var priceRepo = new PriceRecordRepository(_dbService);
        _jsonImportService = new JsonImportService(itemRepo, placeRepo, priceRepo);
    }

    public void Dispose()
    {
        _dbService.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    [Fact]
    public void Constructor_WithServices_InitializesCollections()
    {
        // Act
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Assert
        Assert.NotNull(viewModel.Stores);
        Assert.NotNull(viewModel.SelectedFiles);
        Assert.NotNull(viewModel.PreviewItems);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Assert
        Assert.Equal(1, viewModel.CurrentStep);
        Assert.Equal(DateTime.Today, viewModel.CatalogueDate);
        Assert.Equal("Ready to import...", viewModel.ImportStatus);
        Assert.False(viewModel.IsImporting);
    }

    [Fact]
    public void StepVisibility_Properties_ReturnCorrectValues()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Assert - Step 1
        Assert.Equal(System.Windows.Visibility.Visible, viewModel.Step1Visibility);
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step2Visibility);
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step3Visibility);

        // Act - Move to Step 2
        viewModel.CurrentStep = 2;

        // Assert - Step 2
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step1Visibility);
        Assert.Equal(System.Windows.Visibility.Visible, viewModel.Step2Visibility);
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step3Visibility);

        // Act - Move to Step 3
        viewModel.CurrentStep = 3;

        // Assert - Step 3
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step1Visibility);
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.Step2Visibility);
        Assert.Equal(System.Windows.Visibility.Visible, viewModel.Step3Visibility);
    }

    [Fact]
    public void SelectedFilesText_NoFiles_ReturnsCorrectText()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Assert
        Assert.Equal("No files selected", viewModel.SelectedFilesText);
    }

    [Fact]
    public void SetSelectedFiles_WithFiles_UpdatesCollections()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        var filePaths = new[] { @"C:\test\file1.json", @"C:\test\file2.json" };

        // Act
        viewModel.SetSelectedFiles(filePaths);

        // Assert
        Assert.Equal(2, viewModel.FileCount);
        Assert.Equal("2 file(s) selected", viewModel.SelectedFilesText);
        Assert.Equal(2, viewModel.SelectedFiles.Count);
        Assert.Contains("file1.json", viewModel.SelectedFiles);
        Assert.Contains("file2.json", viewModel.SelectedFiles);
    }

    [Fact]
    public void LoadStores_WithStores_PopulatesStoresCollection()
    {
        // Arrange
        _dataService.AddTestPlace("Coles Brisbane", "Coles");
        _dataService.AddTestPlace("Woolworths Sydney", "Woolworths");
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Act
        viewModel.LoadStores();

        // Assert
        Assert.Equal(2, viewModel.Stores.Count);
    }

    [Fact]
    public void LoadStores_SortsStoresAlphabetically()
    {
        // Arrange
        _dataService.AddTestPlace("Zebra Store", "Zebra");
        _dataService.AddTestPlace("Alpha Store", "Alpha");
        _dataService.AddTestPlace("Beta Store", "Beta");
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Act
        viewModel.LoadStores();

        // Assert
        Assert.Equal("Alpha Store", viewModel.Stores[0].Name);
        Assert.Equal("Beta Store", viewModel.Stores[1].Name);
        Assert.Equal("Zebra Store", viewModel.Stores[2].Name);
    }

    [Fact]
    public void GoToStep1_SetsCurrentStepTo1()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        viewModel.CurrentStep = 2;

        // Act
        viewModel.GoToStep1();

        // Assert
        Assert.Equal(1, viewModel.CurrentStep);
    }

    [Fact]
    public void GoToStep3_SetsCurrentStepTo3()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Act
        viewModel.GoToStep3();

        // Assert
        Assert.Equal(3, viewModel.CurrentStep);
        Assert.Contains("Ready to import", viewModel.ImportStatus);
    }

    [Fact]
    public void IsImporting_WhenTrue_SetsProgressVisibility()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Act
        viewModel.IsImporting = true;

        // Assert
        Assert.Equal(System.Windows.Visibility.Visible, viewModel.ProgressVisibility);
        Assert.False(viewModel.CanImport);
        Assert.False(viewModel.CanGoBack);
    }

    [Fact]
    public void IsImporting_WhenFalse_SetsProgressVisibilityCollapsed()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        viewModel.IsImporting = true;

        // Act
        viewModel.IsImporting = false;

        // Assert
        Assert.Equal(System.Windows.Visibility.Collapsed, viewModel.ProgressVisibility);
        Assert.True(viewModel.CanImport);
        Assert.True(viewModel.CanGoBack);
    }

    [Fact]
    public void ImportButtonText_Initially_ReturnsImport()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Assert
        Assert.Equal("Import", viewModel.ImportButtonText);
    }

    [Fact]
    public void CatalogueDate_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ImportDataViewModel.CatalogueDate))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.CatalogueDate = DateTime.Now.AddDays(-1);

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void SelectedStore_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ImportDataViewModel.SelectedStore))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedStore = new Place { Name = "Test Store" };

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void ImportStatus_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ImportDataViewModel.ImportStatus))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.ImportStatus = "Test Status";

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void CurrentStep_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);
        var propertyChangedRaised = false;
        string? changedProperty = null;
        viewModel.PropertyChanged += (s, e) =>
        {
            propertyChangedRaised = true;
            changedProperty = e.PropertyName;
        };

        // Act
        viewModel.CurrentStep = 2;

        // Assert
        Assert.True(propertyChangedRaised);
    }

    [Fact]
    public void CancelOperation_DoesNotThrow_WhenNotImporting()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dataService, _dialogService, _jsonImportService);

        // Act & Assert (should not throw)
        viewModel.CancelOperation();
    }

    #region Test Helpers

    private class TestGroceryDataService : IGroceryDataService
    {
        private readonly List<Item> _items = new();
        private readonly List<Place> _places = new();
        private readonly List<PriceRecord> _priceRecords = new();

        public IItemRepository Items => throw new NotImplementedException();
        public IPlaceRepository Places => throw new NotImplementedException();
        public IPriceRecordRepository PriceRecords => throw new NotImplementedException();
        public IAlertRepository Alerts => throw new NotImplementedException();

        public string AddTestPlace(string name, string chain)
        {
            var id = Guid.NewGuid().ToString();
            _places.Add(new Place
            {
                Id = id,
                Name = name,
                Chain = chain,
                DateAdded = DateTime.UtcNow,
                IsActive = true
            });
            return id;
        }

        public string AddTestItem(string name, string brand, string category)
        {
            var id = Guid.NewGuid().ToString();
            _items.Add(new Item
            {
                Id = id,
                Name = name,
                Brand = brand,
                Category = category,
                DateAdded = DateTime.UtcNow,
                IsActive = true
            });
            return id;
        }

        public string AddGroceryItem(string name, string? brand = null, string? category = null, string? barcode = null, string? packageSize = null, string? unit = null)
        {
            return AddTestItem(name, brand ?? "", category ?? "");
        }

        public Item? GetItemById(string id) => _items.FirstOrDefault(i => i.Id == id);
        public IEnumerable<Item> GetAllItems() => _items;

        public string AddSupermarket(string name, string chain, string? address = null, string? suburb = null, string? state = null, string? postcode = null)
        {
            return AddTestPlace(name, chain);
        }

        public Place? GetPlaceById(string id) => _places.FirstOrDefault(p => p.Id == id);
        public IEnumerable<Place> GetAllPlaces() => _places.OrderBy(p => p.Name);

        public string RecordPrice(string itemId, string placeId, decimal price, bool isOnSale = false, decimal? originalPrice = null, string? saleDescription = null, DateTime? validFrom = null, DateTime? validTo = null, string source = "manual")
        {
            var id = Guid.NewGuid().ToString();
            _priceRecords.Add(new PriceRecord
            {
                Id = id,
                ItemId = itemId,
                PlaceId = placeId,
                Price = price,
                DateRecorded = DateTime.Now
            });
            return id;
        }

        public IEnumerable<PriceRecord> GetRecentPriceUpdates(int count = 10) => _priceRecords.Take(count);

        public IEnumerable<(Item item, decimal lowestPrice, Place place)> FindBestDeals(string? category = null)
        {
            return Enumerable.Empty<(Item, decimal, Place)>();
        }

        public Dictionary<string, object> GetDashboardStats()
        {
            return new Dictionary<string, object>
            {
                ["totalItems"] = _items.Count,
                ["totalStores"] = _places.Count,
                ["recentUpdates"] = _priceRecords.Count
            };
        }

        public IEnumerable<PriceRecord> GetPriceHistory(string? itemId = null, string? placeId = null, DateTime? from = null, DateTime? to = null)
        {
            return _priceRecords;
        }

        public IEnumerable<(string category, decimal avgPrice, int count)> GetCategoryStats()
        {
            return Enumerable.Empty<(string, decimal, int)>();
        }

        public IEnumerable<(string storeName, decimal avgPrice, int productCount)> GetStoreComparisonStats()
        {
            return Enumerable.Empty<(string, decimal, int)>();
        }

        public void Dispose() { }
    }

    private class TestDialogService : IDialogService
    {
        public bool ShowConfirmation(string message, string title) => true;
        public void ShowError(string message, string title = "Error") { }
        public void ShowInfo(string message, string title = "Information") { }
        public void ShowSuccess(string message, string title = "Success") { }
        public void ShowWarning(string message, string title = "Warning") { }
        public void ShowComparePricesDialog(string? category = null) { }
        public SearchResult? ShowGlobalSearchDialog() => null;
        public void ShowBarcodeScannerDialog() { }
        public void ShowPriceDropNotificationsDialog() { }
        public void ShowFavoritesDialog() { }
        public void ShowDealExpirationRemindersDialog() { }
        public void ShowWeeklySpecialsDigestDialog() { }
    }

    #endregion
}
