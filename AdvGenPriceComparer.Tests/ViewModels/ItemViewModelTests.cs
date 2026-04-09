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
/// Unit tests for ItemViewModel
/// </summary>
public class ItemViewModelTests : IDisposable
{
    private readonly TestGroceryDataService _dataService;
    private readonly TestDialogService _dialogService;
    private readonly TestMediator _mediator;

    public ItemViewModelTests()
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
    public void Constructor_WithServices_InitializesCollections()
    {
        // Act
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Assert
        Assert.NotNull(viewModel.Items);
        Assert.NotNull(viewModel.Categories);
        Assert.Contains("All Categories", viewModel.Categories);
    }

    [Fact]
    public void Constructor_WithServices_LoadsItems()
    {
        // Arrange
        _dataService.AddTestItem("Test Item", "Brand", "Category");

        // Act
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("Test Item", viewModel.Items[0].Name);
    }

    [Fact]
    public void Constructor_WithItems_PopulatesCategories()
    {
        // Arrange
        _dataService.AddTestItem("Item 1", "Brand", "Dairy");
        _dataService.AddTestItem("Item 2", "Brand", "Bakery");
        _dataService.AddTestItem("Item 3", "Brand", "Dairy");

        // Act
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Assert
        Assert.Equal(3, viewModel.Categories.Count); // All Categories + 2 unique categories
        Assert.Contains("Dairy", viewModel.Categories);
        Assert.Contains("Bakery", viewModel.Categories);
    }

    [Fact]
    public void ItemCountText_WithItems_ReturnsCorrectCount()
    {
        // Arrange
        var viewModel = new ItemViewModel(_mediator, _dialogService);
        _dataService.AddTestItem("Item 1", "Brand", "Category");
        _dataService.AddTestItem("Item 2", "Brand", "Category");

        // Act - Reload items
        viewModel.RefreshCommand.Execute(null);

        // Assert
        Assert.Equal("2 items", viewModel.ItemCountText);
    }

    [Fact]
    public void SearchText_WithMatchingItems_FiltersItems()
    {
        // Arrange
        _dataService.AddTestItem("Milk", "Dairy Brand", "Dairy");
        _dataService.AddTestItem("Bread", "Bakery Brand", "Bakery");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SearchText = "Milk";

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("Milk", viewModel.Items[0].Name);
    }

    [Fact]
    public void SearchText_WithNoMatches_ShowsEmptyList()
    {
        // Arrange
        _dataService.AddTestItem("Milk", "Brand", "Dairy");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SearchText = "NonExistent";

        // Assert
        Assert.Empty(viewModel.Items);
    }

    [Fact]
    public void SearchText_CaseInsensitive_MatchesItems()
    {
        // Arrange
        _dataService.AddTestItem("MILK", "Brand", "Dairy");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SearchText = "milk";

        // Assert
        Assert.Single(viewModel.Items);
    }

    [Fact]
    public void SearchText_MatchesBrand_FiltersItems()
    {
        // Arrange
        _dataService.AddTestItem("Product", "SpecialBrand", "Category");
        _dataService.AddTestItem("Other", "OtherBrand", "Category");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SearchText = "Special";

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("Product", viewModel.Items[0].Name);
    }

    [Fact]
    public void SearchText_MatchesCategory_FiltersItems()
    {
        // Arrange
        _dataService.AddTestItem("Item 1", "Brand", "Dairy");
        _dataService.AddTestItem("Item 2", "Brand", "Bakery");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SearchText = "Dairy";

        // Assert
        Assert.Single(viewModel.Items);
    }

    [Fact]
    public void SelectedCategory_WithValidCategory_FiltersItems()
    {
        // Arrange
        _dataService.AddTestItem("Milk", "Brand", "Dairy");
        _dataService.AddTestItem("Bread", "Brand", "Bakery");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SelectedCategory = "Dairy";

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("Milk", viewModel.Items[0].Name);
    }

    [Fact]
    public void SelectedCategory_AllCategories_ShowsAllItems()
    {
        // Arrange
        _dataService.AddTestItem("Item 1", "Brand", "Dairy");
        _dataService.AddTestItem("Item 2", "Brand", "Bakery");
        var viewModel = new ItemViewModel(_mediator, _dialogService);
        viewModel.SelectedCategory = "Dairy";
        Assert.Single(viewModel.Items);

        // Act
        viewModel.SelectedCategory = "All Categories";

        // Assert
        Assert.Equal(2, viewModel.Items.Count);
    }

    [Fact]
    public void SearchText_AndCategory_BothFiltersApplied()
    {
        // Arrange
        _dataService.AddTestItem("Whole Milk", "Brand", "Dairy");
        _dataService.AddTestItem("Skim Milk", "Brand", "Dairy");
        _dataService.AddTestItem("White Bread", "Brand", "Bakery");
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Act
        viewModel.SelectedCategory = "Dairy";
        viewModel.SearchText = "Whole";

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("Whole Milk", viewModel.Items[0].Name);
    }

    [Fact]
    public void SelectedItem_SetValue_RaisesPropertyChanged()
    {
        // Arrange
        _dataService.AddTestItem("Test Item", "Brand", "Category");
        var viewModel = new ItemViewModel(_mediator, _dialogService);
        var item = viewModel.Items.First();
        var propertyChangedRaised = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(ItemViewModel.SelectedItem))
                propertyChangedRaised = true;
        };

        // Act
        viewModel.SelectedItem = item;

        // Assert
        Assert.True(propertyChangedRaised);
        Assert.Equal(item, viewModel.SelectedItem);
    }

    [Fact]
    public void Commands_AreInitialized()
    {
        // Act
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Assert
        Assert.NotNull(viewModel.AddItemCommand);
        Assert.NotNull(viewModel.EditItemCommand);
        Assert.NotNull(viewModel.DeleteItemCommand);
        Assert.NotNull(viewModel.RefreshCommand);
    }

    [Fact]
    public void RefreshCommand_Executes_ReloadsItems()
    {
        // Arrange
        var viewModel = new ItemViewModel(_mediator, _dialogService);
        Assert.Empty(viewModel.Items);

        // Act
        _dataService.AddTestItem("New Item", "Brand", "Category");
        viewModel.RefreshCommand.Execute(null);

        // Assert
        Assert.Single(viewModel.Items);
        Assert.Equal("New Item", viewModel.Items[0].Name);
    }

    [Fact]
    public void Categories_AreSortedAlphabetically()
    {
        // Arrange
        _dataService.AddTestItem("Item 1", "Brand", "Zebra");
        _dataService.AddTestItem("Item 2", "Brand", "Apple");
        _dataService.AddTestItem("Item 3", "Brand", "Mango");

        // Act
        var viewModel = new ItemViewModel(_mediator, _dialogService);

        // Assert
        var categoriesList = viewModel.Categories.Skip(1).ToList(); // Skip "All Categories"
        Assert.Equal("Apple", categoriesList[0]);
        Assert.Equal("Mango", categoriesList[1]);
        Assert.Equal("Zebra", categoriesList[2]);
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
    }
}
