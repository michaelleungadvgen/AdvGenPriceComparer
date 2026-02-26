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
/// Unit tests for MainWindowViewModel
/// </summary>
public class MainWindowViewModelTests : IDisposable
{
    private readonly TestGroceryDataService _dataService;
    private readonly TestDialogService _dialogService;

    public MainWindowViewModelTests()
    {
        _dataService = new TestGroceryDataService();
        _dialogService = new TestDialogService();
    }

    public void Dispose()
    {
        _dataService.Dispose();
    }

    [Fact]
    public void Constructor_WithServices_InitializesCommands()
    {
        // Act
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);

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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);

        // Assert
        Assert.Equal(1, viewModel.TotalItems);
        Assert.Equal(1, viewModel.TrackedStores);
    }

    [Fact]
    public void RefreshDashboard_WithNoData_ShowsZeroStats()
    {
        // Arrange
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);

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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);
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
        var viewModel = new MainWindowViewModel(_dataService, _dialogService);

        // Act & Assert (should not throw)
        viewModel.Dispose();
        viewModel.Dispose();
    }

    #region Test Helpers

    /// <summary>
    /// Test implementation of IGroceryDataService
    /// </summary>
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

        public string AddTestPriceRecord(string itemId, string placeId, decimal price, DateTime? date = null)
        {
            var id = Guid.NewGuid().ToString();
            var item = _items.FirstOrDefault(i => i.Id == itemId);
            var place = _places.FirstOrDefault(p => p.Id == placeId);

            _priceRecords.Add(new PriceRecord
            {
                Id = id,
                ItemId = itemId,
                PlaceId = placeId,
                Price = price,
                DateRecorded = date ?? DateTime.Now
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
            return AddTestPriceRecord(itemId, placeId, price);
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
            var query = _priceRecords.AsEnumerable();

            if (!string.IsNullOrEmpty(itemId))
                query = query.Where(p => p.ItemId == itemId);

            if (!string.IsNullOrEmpty(placeId))
                query = query.Where(p => p.PlaceId == placeId);

            if (from.HasValue)
                query = query.Where(p => p.DateRecorded >= from.Value);

            if (to.HasValue)
                query = query.Where(p => p.DateRecorded <= to.Value);

            return query;
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
    }

    #endregion
}
