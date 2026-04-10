using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Application.Services;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Tests.Services;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using AdvGenPriceComparer.WPF.Views;
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
    private readonly TestMediator _mediator;
    private readonly DatabaseService _dbService;
    private readonly JsonImportService _jsonImportService;

    public ImportDataViewModelTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_import_vm_{Guid.NewGuid():N}.db");
        _dataService = new TestGroceryDataService();
        _dialogService = new TestDialogService();
        _mediator = new TestMediator(_dataService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Assert
        Assert.NotNull(viewModel.Stores);
        Assert.NotNull(viewModel.SelectedFiles);
        Assert.NotNull(viewModel.PreviewItems);
    }

    [Fact]
    public void Constructor_SetsDefaultValues()
    {
        // Act
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Assert
        Assert.Equal("No files selected", viewModel.SelectedFilesText);
    }

    [Fact]
    public void SetSelectedFiles_WithFiles_UpdatesCollections()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Act
        viewModel.LoadStoresAsync().GetAwaiter().GetResult();

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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Act
        viewModel.LoadStoresAsync().GetAwaiter().GetResult();

        // Assert
        Assert.Equal("Alpha Store", viewModel.Stores[0].Name);
        Assert.Equal("Beta Store", viewModel.Stores[1].Name);
        Assert.Equal("Zebra Store", viewModel.Stores[2].Name);
    }

    [Fact]
    public void GoToStep1_SetsCurrentStepTo1()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Assert
        Assert.Equal("Import", viewModel.ImportButtonText);
    }

    [Fact]
    public void CatalogueDate_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);
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
        var viewModel = new ImportDataViewModel(_dialogService, _mediator, _jsonImportService);

        // Act & Assert (should not throw)
        viewModel.CancelOperation();
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
        public void ShowShoppingListsDialog() { }
        public void ShowSettingsDialog() { }
        public void ShowMLModelManagementDialog() { }
        public void ShowPriceForecastDialog() { }
        public void ShowChatDialog() { }
        public void ShowExportDataDialog() { }
        public void ShowImportFromUrlDialog() { }
        public void ShowIllusoryDiscountDetectionDialog() { }
        public void ShowServerDataTransferDialog() { }
        public void ShowBestPricesDialog() { }
        public void ShowEditPlaceDialog(Core.Models.Place place) { }
        public void ShowTripOptimizerDialog() { }
        public void ShowPriceAlertsDialog() { }
        public void ShowWeeklySpecialsImportDialog() { }
        public void ShowCloudSyncDialog() { }
        public void ShowStaticPeerConfigDialog() { }
        public bool ShowQuestion(string title, string message) => true;
        public ExportProgressWindow ShowExportProgressDialog(string title = "Exporting Data...") => null!;
        public ImportProgressWindow ShowImportProgressDialog(string title = "Importing Data...") => null!;
        public void ShowValidationReportDialog(Core.Models.ValidationReport report) { }
    }
}


