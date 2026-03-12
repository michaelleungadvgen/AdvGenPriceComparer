using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.WPF.Services;
using ILoggerService = AdvGenPriceComparer.WPF.Services.ILoggerService;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Comprehensive unit tests for ExportService
/// </summary>
public class ExportServiceTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _dbService;
    private readonly ExportService _exportService;
    private readonly ItemRepository _itemRepository;
    private readonly PlaceRepository _placeRepository;
    private readonly PriceRecordRepository _priceRecordRepository;
    private readonly TestLoggerService _logger;

    public ExportServiceTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_export_{Guid.NewGuid():N}.db");
        _dbService = new DatabaseService(_testDbPath);
        _itemRepository = new ItemRepository(_dbService);
        _placeRepository = new PlaceRepository(_dbService);
        _priceRecordRepository = new PriceRecordRepository(_dbService);
        _logger = new TestLoggerService();
        _exportService = new ExportService(_itemRepository, _placeRepository, _priceRecordRepository, _logger);
    }

    public void Dispose()
    {
        _dbService.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region ExportToJsonAsync Tests

    [Fact]
    public async Task ExportToJsonAsync_NoItems_ReturnsEmptyExport()
    {
        // Arrange
        var outputPath = Path.Combine(Path.GetTempPath(), $"export_empty_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.ItemsExported);
            Assert.True(File.Exists(outputPath));
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Empty(exportData.Items);
            Assert.Equal("1.0", exportData.ExportVersion);
            Assert.Equal("AdvGenPriceComparer", exportData.Source);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_SingleItem_ExportsSuccessfully()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var item = CreateTestItem("Test Product", "TestBrand", "TestCategory", 9.99m);
        CreateTestPriceRecord(item.Id!, store.Id!, 9.99m, null, false);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_single_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported);
            Assert.True(result.FileSizeBytes > 0);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Single(exportData.Items);
            Assert.Equal("Test Product", exportData.Items[0].Name);
            Assert.Equal("TestBrand", exportData.Items[0].Brand);
            Assert.Equal(9.99m, exportData.Items[0].Price);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_MultipleItems_ExportsAll()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        for (int i = 1; i <= 50; i++)
        {
            var item = CreateTestItem($"Product {i}", $"Brand{i % 5}", $"Category{i % 3}", (decimal)(i * 1.99));
            CreateTestPriceRecord(item.Id!, store.Id!, (decimal)(i * 1.99m), null, i % 2 == 0);
        }

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_multiple_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(50, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Equal(50, exportData.Items.Count);
            Assert.Equal(50, exportData.Statistics.TotalItems);
            Assert.Equal(1, exportData.Statistics.UniqueStores);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithCategoryFilter_FiltersCorrectly()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var item1 = CreateTestItem("Product A", "BrandA", "Category1", 10.00m);
        var item2 = CreateTestItem("Product B", "BrandB", "Category2", 20.00m);
        var item3 = CreateTestItem("Product C", "BrandC", "Category1", 30.00m);
        
        CreateTestPriceRecord(item1.Id!, store.Id!, 10.00m);
        CreateTestPriceRecord(item2.Id!, store.Id!, 20.00m);
        CreateTestPriceRecord(item3.Id!, store.Id!, 30.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_filter_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { Category = "Category1" };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Equal(2, exportData.Items.Count);
            Assert.All(exportData.Items, item => Assert.Equal("Category1", item.Category));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithBrandFilter_FiltersCorrectly()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var item1 = CreateTestItem("Product A", "TargetBrand", "Category1", 10.00m);
        var item2 = CreateTestItem("Product B", "OtherBrand", "Category2", 20.00m);
        var item3 = CreateTestItem("Product C", "TargetBrand", "Category3", 30.00m);
        
        CreateTestPriceRecord(item1.Id!, store.Id!, 10.00m);
        CreateTestPriceRecord(item2.Id!, store.Id!, 20.00m);
        CreateTestPriceRecord(item3.Id!, store.Id!, 30.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_brand_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { Brand = "TargetBrand" };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.All(exportData.Items, item => Assert.Equal("TargetBrand", item.Brand));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithPriceRangeFilter_FiltersCorrectly()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var item1 = CreateTestItem("Cheap Product", "BrandA", "Category1", 5.00m);
        var item2 = CreateTestItem("Mid Product", "BrandB", "Category2", 15.00m);
        var item3 = CreateTestItem("Expensive Product", "BrandC", "Category3", 50.00m);
        
        CreateTestPriceRecord(item1.Id!, store.Id!, 5.00m);
        CreateTestPriceRecord(item2.Id!, store.Id!, 15.00m);
        CreateTestPriceRecord(item3.Id!, store.Id!, 50.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_price_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { MinPrice = 10.00m, MaxPrice = 20.00m };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Single(exportData.Items);
            Assert.Equal("Mid Product", exportData.Items[0].Name);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithOnlyOnSaleFilter_OnlyExportsSaleItems()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var item1 = CreateTestItem("Sale Product", "BrandA", "Category1", 5.00m);
        var item2 = CreateTestItem("Regular Product", "BrandB", "Category2", 15.00m);
        var item3 = CreateTestItem("Another Sale", "BrandC", "Category3", 10.00m);
        
        CreateTestPriceRecord(item1.Id!, store.Id!, 5.00m, 10.00m, true);
        CreateTestPriceRecord(item2.Id!, store.Id!, 15.00m, null, false);
        CreateTestPriceRecord(item3.Id!, store.Id!, 10.00m, 20.00m, true);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_sale_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { OnlyOnSale = true };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.All(exportData.Items, item => Assert.True(item.IsOnSale));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithStoreFilter_FiltersCorrectly()
    {
        // Arrange
        var store1 = CreateTestStore("Store One", "ChainA");
        var store2 = CreateTestStore("Store Two", "ChainB");
        
        var item = CreateTestItem("Test Product", "BrandA", "Category1", 10.00m);
        
        CreateTestPriceRecord(item.Id!, store1.Id!, 10.00m);
        CreateTestPriceRecord(item.Id!, store2.Id!, 12.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_store_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { StoreIds = new List<string> { store1.Id! } };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Single(exportData.Items);
            Assert.Equal(store1.Id, exportData.Items[0].StoreId);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithProgress_ReportsProgress()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        for (int i = 1; i <= 20; i++)
        {
            var item = CreateTestItem($"Product {i}", "Brand", "Category", (decimal)(i * 1.00));
            CreateTestPriceRecord(item.Id!, store.Id!, (decimal)(i * 1.00));
        }

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_progress_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();
        var progressReports = new List<ExportProgress>();
        var progress = new Progress<ExportProgress>(p => progressReports.Add(p));

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath, progress);

            // Allow time for progress reports
            await Task.Delay(200);

            // Assert
            Assert.True(result.Success);
            Assert.True(progressReports.Count > 0, "Expected at least one progress report");
            Assert.Contains(progressReports, p => p.Percentage == 0);
            Assert.Contains(progressReports, p => p.Percentage == 100);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_InvalidDirectory_ReturnsError()
    {
        // Arrange
        var outputPath = Path.Combine("Z:\\NonExistent\\Path", "export.json");
        var options = new ExportOptions();

        // Act
        var result = await _exportService.ExportToJsonAsync(options, outputPath);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Export failed", result.ErrorMessage);
    }

    [Fact]
    public async Task ExportToJsonAsync_SetsCorrectMetadata()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var item = CreateTestItem("Test Product", "Brand", "Category", 10.00m);
        CreateTestPriceRecord(item.Id!, store.Id!, 10.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_meta_{Guid.NewGuid():N}.json");
        var options = new ExportOptions 
        { 
            LocationSuburb = "Brisbane",
            LocationState = "QLD",
            LocationCountry = "Australia",
            Category = "Category"
        };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Equal("Brisbane", exportData.Location.Suburb);
            Assert.Equal("QLD", exportData.Location.State);
            Assert.Equal("Australia", exportData.Location.Country);
            Assert.Equal("Category", exportData.ExportOptions.Category);
            Assert.Equal(1, exportData.Statistics.TotalItems);
            Assert.Single(exportData.Statistics.Categories);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region ExportToJsonGzAsync Tests

    [Fact]
    public async Task ExportToJsonGzAsync_CreatesCompressedFile()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        for (int i = 1; i <= 10; i++)
        {
            var item = CreateTestItem($"Product {i}", "Brand", "Category", (decimal)(i * 1.00));
            CreateTestPriceRecord(item.Id!, store.Id!, (decimal)(i * 1.00));
        }

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_gz_{Guid.NewGuid():N}.json.gz");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonGzAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.True(File.Exists(outputPath));
            Assert.True(result.FileSizeBytes > 0);
            Assert.True(result.CompressionRatio > 0);
            Assert.True(result.CompressionRatio <= 1.0);
            
            // Verify file can be decompressed and read
            var decompressedData = await DecompressAndReadAsync(outputPath);
            Assert.Equal(10, decompressedData.Items.Count);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonGzAsync_EmptyData_CreatesValidCompressedFile()
    {
        // Arrange
        var outputPath = Path.Combine(Path.GetTempPath(), $"export_gz_empty_{Guid.NewGuid():N}.json.gz");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonGzAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.ItemsExported);
            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region IncrementalExportAsync Tests

    [Fact]
    public async Task IncrementalExportAsync_OnlyExportsItemsUpdatedAfterDate()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var cutoffDate = DateTime.UtcNow.AddDays(-7);
        
        // Old item (should not be exported)
        var oldItem = new Item
        {
            Name = "Old Product",
            Brand = "OldBrand",
            Category = "OldCategory",
            IsActive = true,
            DateAdded = DateTime.UtcNow.AddDays(-30),
            LastUpdated = DateTime.UtcNow.AddDays(-14) // Before cutoff
        };
        var oldItemId = _itemRepository.Add(oldItem);
        CreateTestPriceRecord(oldItemId, store.Id!, 5.00m);
        
        // New item (should be exported)
        var newItem = new Item
        {
            Name = "New Product",
            Brand = "NewBrand",
            Category = "NewCategory",
            IsActive = true,
            DateAdded = DateTime.UtcNow.AddDays(-2),
            LastUpdated = DateTime.UtcNow.AddDays(-2) // After cutoff
        };
        var newItemId = _itemRepository.Add(newItem);
        CreateTestPriceRecord(newItemId, store.Id!, 10.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_incremental_{Guid.NewGuid():N}.json");

        try
        {
            // Act
            var result = await _exportService.IncrementalExportAsync(cutoffDate, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Single(exportData.Items);
            Assert.Equal("New Product", exportData.Items[0].Name);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region Export with Sales Data Tests

    [Fact]
    public async Task ExportToJsonAsync_WithSaleData_IncludesSaleInfo()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var item = CreateTestItem("Sale Product", "Brand", "Category", 5.00m);
        
        var priceRecord = new PriceRecord
        {
            ItemId = item.Id!,
            PlaceId = store.Id!,
            Price = 5.00m,
            OriginalPrice = 10.00m,
            IsOnSale = true,
            SaleDescription = "50% Off Special",
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(6),
            DateRecorded = DateTime.UtcNow
        };
        _priceRecordRepository.Add(priceRecord);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_sale_data_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Single(exportData.Items);
            Assert.True(exportData.Items[0].IsOnSale);
            Assert.Equal(5.00m, exportData.Items[0].Price);
            Assert.Equal(10.00m, exportData.Items[0].OriginalPrice);
            Assert.Equal("50% Off Special", exportData.Items[0].SaleDescription);
            Assert.Equal("Test Store", exportData.Items[0].Store);
            Assert.Equal("TestChain", exportData.Items[0].StoreChain);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region Date Range Filter Tests

    [Fact]
    public async Task ExportToJsonAsync_WithValidDateRange_FiltersCorrectly()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var item = CreateTestItem("Test Product", "Brand", "Category", 10.00m);
        
        // Price record within date range
        var priceRecord = new PriceRecord
        {
            ItemId = item.Id!,
            PlaceId = store.Id!,
            Price = 10.00m,
            DateRecorded = DateTime.UtcNow.AddDays(-5),
            ValidFrom = DateTime.UtcNow.AddDays(-5),
            ValidTo = DateTime.UtcNow.AddDays(2)
        };
        _priceRecordRepository.Add(priceRecord);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_date_range_{Guid.NewGuid():N}.json");
        var options = new ExportOptions 
        { 
            ValidFrom = DateTime.UtcNow.AddDays(-7),
            ValidTo = DateTime.UtcNow.AddDays(7)
        };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_WithDateRangeOutsideRecord_ExcludesRecord()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var item = CreateTestItem("Test Product", "Brand", "Category", 10.00m);
        
        // Price record outside date range
        var priceRecord = new PriceRecord
        {
            ItemId = item.Id!,
            PlaceId = store.Id!,
            Price = 10.00m,
            DateRecorded = DateTime.UtcNow.AddDays(-30),
            ValidFrom = DateTime.UtcNow.AddDays(-30),
            ValidTo = DateTime.UtcNow.AddDays(-20)
        };
        _priceRecordRepository.Add(priceRecord);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_date_outside_{Guid.NewGuid():N}.json");
        var options = new ExportOptions 
        { 
            ValidFrom = DateTime.UtcNow.AddDays(-7),
            ValidTo = DateTime.UtcNow
        };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0, result.ItemsExported); // Record is outside date range
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region ActiveOnly Filter Tests

    [Fact]
    public async Task ExportToJsonAsync_ActiveOnlyFalse_IncludesInactiveItems()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var activeItem = CreateTestItem("Active Product", "Brand", "Category", 10.00m);
        CreateTestPriceRecord(activeItem.Id!, store.Id!, 10.00m);
        
        var inactiveItem = new Item
        {
            Name = "Inactive Product",
            Brand = "Brand",
            Category = "Category",
            IsActive = false,
            DateAdded = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        var inactiveItemId = _itemRepository.Add(inactiveItem);
        CreateTestPriceRecord(inactiveItemId, store.Id!, 20.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_active_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { ActiveOnly = false };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.ItemsExported); // Both active and inactive
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    [Fact]
    public async Task ExportToJsonAsync_ActiveOnlyTrue_ExcludesInactiveItems()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        var activeItem = CreateTestItem("Active Product", "Brand", "Category", 10.00m);
        CreateTestPriceRecord(activeItem.Id!, store.Id!, 10.00m);
        
        var inactiveItem = new Item
        {
            Name = "Inactive Product",
            Brand = "Brand",
            Category = "Category",
            IsActive = false,
            DateAdded = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        var inactiveItemId = _itemRepository.Add(inactiveItem);
        CreateTestPriceRecord(inactiveItemId, store.Id!, 20.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_active_only_{Guid.NewGuid():N}.json");
        var options = new ExportOptions { ActiveOnly = true };

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsExported); // Only active
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Equal("Active Product", exportData.Items[0].Name);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region Statistics Tests

    [Fact]
    public async Task ExportToJsonAsync_CalculatesCorrectStatistics()
    {
        // Arrange
        var store1 = CreateTestStore("Store One", "ChainA");
        var store2 = CreateTestStore("Store Two", "ChainB");
        
        var item1 = CreateTestItem("Product 1", "Brand1", "CategoryA", 10.00m);
        var item2 = CreateTestItem("Product 2", "Brand2", "CategoryB", 20.00m);
        var item3 = CreateTestItem("Product 3", "Brand1", "CategoryA", 30.00m);
        
        CreateTestPriceRecord(item1.Id!, store1.Id!, 10.00m);
        CreateTestPriceRecord(item2.Id!, store2.Id!, 20.00m);
        CreateTestPriceRecord(item3.Id!, store1.Id!, 30.00m);

        var outputPath = Path.Combine(Path.GetTempPath(), $"export_stats_{Guid.NewGuid():N}.json");
        var options = new ExportOptions();

        try
        {
            // Act
            var result = await _exportService.ExportToJsonAsync(options, outputPath);

            // Assert
            Assert.True(result.Success);
            
            var exportData = await ReadExportDataAsync(outputPath);
            Assert.Equal(3, exportData.Statistics.TotalItems);
            Assert.Equal(3, exportData.Statistics.TotalPriceRecords);
            Assert.Equal(2, exportData.Statistics.UniqueStores);
            Assert.Equal(2, exportData.Statistics.Categories.Count);
            Assert.Contains("CategoryA", exportData.Statistics.Categories);
            Assert.Contains("CategoryB", exportData.Statistics.Categories);
        }
        finally
        {
            if (File.Exists(outputPath)) File.Delete(outputPath);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<ExportData> ReadExportDataAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<ExportData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    private async Task<ExportData> DecompressAndReadAsync(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        using var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);
        using var reader = new StreamReader(gzipStream);
        var json = await reader.ReadToEndAsync();
        return JsonSerializer.Deserialize<ExportData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;
    }

    private Place CreateTestStore(string name, string chain)
    {
        var store = new Place
        {
            Name = name,
            Chain = chain,
            IsActive = true,
            DateAdded = DateTime.UtcNow
        };
        var id = _placeRepository.Add(store);
        store.Id = id;
        return store;
    }

    private Item CreateTestItem(string name, string brand, string category, decimal price)
    {
        var item = new Item
        {
            Name = name,
            Brand = brand,
            Category = category,
            IsActive = true,
            DateAdded = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };
        var id = _itemRepository.Add(item);
        item.Id = id;
        return item;
    }

    private void CreateTestPriceRecord(string itemId, string placeId, decimal price, decimal? originalPrice = null, bool isOnSale = false)
    {
        var priceRecord = new PriceRecord
        {
            ItemId = itemId,
            PlaceId = placeId,
            Price = price,
            OriginalPrice = originalPrice,
            IsOnSale = isOnSale,
            DateRecorded = DateTime.UtcNow,
            ValidFrom = DateTime.UtcNow.AddDays(-1),
            ValidTo = DateTime.UtcNow.AddDays(7)
        };
        _priceRecordRepository.Add(priceRecord);
    }

    #endregion
}

/// <summary>
/// Test implementation of ILoggerService for unit tests
/// </summary>
public class TestLoggerService : ILoggerService
{
    public List<string> LogMessages { get; } = new();

    public void LogInfo(string message)
    {
        LogMessages.Add($"[INFO] {message}");
    }

    public void LogWarning(string message)
    {
        LogMessages.Add($"[WARN] {message}");
    }

    public void LogError(string message, Exception? ex = null)
    {
        LogMessages.Add($"[ERROR] {message}");
        if (ex != null)
        {
            LogMessages.Add($"[EXCEPTION] {ex.Message}");
        }
    }

    public void LogDebug(string message)
    {
        LogMessages.Add($"[DEBUG] {message}");
    }

    public void LogCritical(string message, Exception? ex = null)
    {
        LogMessages.Add($"[CRITICAL] {message}");
        if (ex != null)
        {
            LogMessages.Add($"[EXCEPTION] {ex.Message}");
        }
    }

    public string GetLogFilePath()
    {
        return string.Empty;
    }
}
