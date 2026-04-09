using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Tests.Services;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;
using AdvGenPriceComparer.WPF.ViewModels;
using Xunit;

namespace AdvGenPriceComparer.Tests.ViewModels;

/// <summary>
/// Unit tests for MainWindowViewModel
/// </summary>
public class MainWindowViewModelTests : IDisposable
{
    private readonly TestGroceryDataService _dataService;
    private readonly TestDialogService _dialogService;
    private readonly TestMediator _mediator;

    public MainWindowViewModelTests()
    {
        _dataService = new TestGroceryDataService();
        _dialogService = new TestDialogService();
        _mediator = new TestMediator(_dataService);
    }

    public void Dispose()
    {
        _dataService.Dispose();
    }

    [Fact]
    public void Constructor_WithServices_InitializesCommands()
    {
        // Act
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);

        // Assert
        Assert.NotNull(viewModel.AddItemCommand);
        Assert.NotNull(viewModel.AddPlaceCommand);
    }

    [Fact]
    public void Constructor_WithServices_RefreshesDashboard()
    {
        // Arrange
        _dataService.AddTestItem("Test Item", "Test Brand", "Test Category");
        _dataService.AddTestPlace("Test Store", "TestChain");

        // Act
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);

        // Assert
        Assert.Equal(1, viewModel.TotalItems);
        Assert.Equal(1, viewModel.TrackedStores);
    }

    [Fact]
    public void RefreshDashboard_WithNoData_ShowsZeroStats()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);

        // Act
        viewModel.RefreshDashboard();

        // Assert
        Assert.Equal(0, viewModel.TotalItems);
        Assert.Equal(0, viewModel.TrackedStores);
        Assert.Equal(0, viewModel.PriceUpdates);
    }

    [Fact]
    public void RefreshDashboard_WithData_UpdatesStats()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        _dataService.AddTestItem("Item 1", "Brand 1", "Category 1");
        _dataService.AddTestItem("Item 2", "Brand 2", "Category 2");
        _dataService.AddTestPlace("Store 1", "Chain 1");
        _dataService.AddTestPlace("Store 2", "Chain 2");
        _dataService.AddTestPriceRecord("item1", "place1", 10.00m);

        // Act
        viewModel.RefreshDashboard();

        // Assert
        Assert.Equal(2, viewModel.TotalItems);
        Assert.Equal(2, viewModel.TrackedStores);
    }

    [Fact]
    public void RefreshDashboard_WithCategoryData_PopulatesCategorySeries()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        _dataService.AddTestItem("Item 1", "Brand 1", "Dairy");
        _dataService.AddTestItem("Item 2", "Brand 2", "Dairy");
        _dataService.AddTestItem("Item 3", "Brand 3", "Bakery");

        // Act
        viewModel.RefreshDashboard();

        // Assert
        Assert.NotNull(viewModel.CategorySeries);
        Assert.True(viewModel.CategorySeries.Length > 0);
    }

    [Fact]
    public void RefreshDashboard_WithPriceHistory_PopulatesPriceTrendSeries()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        var itemId = _dataService.AddTestItem("Test Item", "Brand", "Category");
        var placeId = _dataService.AddTestPlace("Test Store", "Chain");
        _dataService.AddTestPriceRecord(itemId, placeId, 10.00m, DateTime.Now.AddDays(-5));
        _dataService.AddTestPriceRecord(itemId, placeId, 12.00m, DateTime.Now.AddDays(-3));
        _dataService.AddTestPriceRecord(itemId, placeId, 11.00m, DateTime.Now.AddDays(-1));

        // Act
        viewModel.RefreshDashboard();

        // Assert
        Assert.NotNull(viewModel.PriceTrendSeries);
        Assert.True(viewModel.PriceTrendSeries.Length > 0);
        Assert.NotNull(viewModel.XAxes);
    }

    [Fact]
    public void TotalItems_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.TotalItems))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TotalItems = 5;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(5, viewModel.TotalItems);
    }

    [Fact]
    public void TrackedStores_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.TrackedStores))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.TrackedStores = 3;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(3, viewModel.TrackedStores);
    }

    [Fact]
    public void PriceUpdates_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainWindowViewModel.PriceUpdates))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.PriceUpdates = 10;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(10, viewModel.PriceUpdates);
    }

    [Fact]
    public void OnStoreAdded_EventCanBeSubscribed()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);
        var eventRaised = false;
        
        // Act - subscribe to the event (should not throw)
        viewModel.OnStoreAdded += () => eventRaised = true;

        // Assert - subscription was successful
        Assert.False(eventRaised); // Event not raised yet
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_mediator, _dialogService);

        // Act & Assert (should not throw)
        viewModel.Dispose();
        viewModel.Dispose();
    }


    /// <summary>
    /// Test implementation of IDialogService
    /// </summary>
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
    }
}
