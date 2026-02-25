using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
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

    public ItemViewModelTests()
    {
        _dataService = new TestGroceryDataService();
        _dialogService = new TestDialogService();
    }

    public void Dispose()
    {
        _dataService.Dispose();
    }

    [Fact]
    public void Constructor_WithServices_InitializesCollections()
    {
        // Act
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

        // Assert
        Assert.Equal(3, viewModel.Categories.Count); // All Categories + 2 unique categories
        Assert.Contains("Dairy", viewModel.Categories);
        Assert.Contains("Bakery", viewModel.Categories);
    }

    [Fact]
    public void ItemCountText_WithItems_ReturnsCorrectCount()
    {
        // Arrange
        var viewModel = new ItemViewModel(_dataService, _dialogService);
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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);
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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);
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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

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
        var viewModel = new ItemViewModel(_dataService, _dialogService);
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
        var viewModel = new ItemViewModel(_dataService, _dialogService);

        // Assert
        var categoriesList = viewModel.Categories.Skip(1).ToList(); // Skip "All Categories"
        Assert.Equal("Apple", categoriesList[0]);
        Assert.Equal("Mango", categoriesList[1]);
        Assert.Equal("Zebra", categoriesList[2]);
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
        public IEnumerable<Place> GetAllPlaces() => _places;

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
            return _items
                .Where(i => !string.IsNullOrEmpty(i.Category))
                .GroupBy(i => i.Category!)
                .Select(g => (g.Key, 0m, g.Count()));
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
    }

    #endregion
}
