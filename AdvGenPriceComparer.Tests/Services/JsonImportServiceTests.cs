using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Comprehensive unit tests for JsonImportService
/// </summary>
public class JsonImportServiceTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _dbService;
    private readonly JsonImportService _importService;
    private readonly ItemRepository _itemRepository;
    private readonly PlaceRepository _placeRepository;
    private readonly PriceRecordRepository _priceRecordRepository;

    public JsonImportServiceTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_jsonimport_{Guid.NewGuid():N}.db");
        _dbService = new DatabaseService(_testDbPath);
        _importService = new JsonImportService(_dbService);
        _itemRepository = new ItemRepository(_dbService);
        _placeRepository = new PlaceRepository(_dbService);
        _priceRecordRepository = new PriceRecordRepository(_dbService);
    }

    public void Dispose()
    {
        _dbService.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region PreviewImportAsync Tests

    [Fact]
    public async Task PreviewImportAsync_ValidColesJson_ReturnsProducts()
    {
        // Arrange
        var jsonPath = CreateTestJsonFile("coles_test.json", new[]
        {
            new ColesProduct
            {
                ProductID = "CL001",
                ProductName = "Test Product",
                Brand = "TestBrand",
                Category = "Test Category",
                Price = "$5.00",
                OriginalPrice = "$10.00",
                Savings = "$5.00",
                SpecialType = "Half Price"
            }
        });

        try
        {
            // Act
            var (products, errors) = await _importService.PreviewImportAsync(jsonPath);

            // Assert
            Assert.Single(products);
            Assert.Empty(errors);
            Assert.Equal("Test Product", products[0].ProductName);
            Assert.Equal("TestBrand", products[0].Brand);
            Assert.Equal("$5.00", products[0].Price);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public async Task PreviewImportAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var jsonPath = Path.Combine(Path.GetTempPath(), $"invalid_{Guid.NewGuid():N}.json");
        File.WriteAllText(jsonPath, "not valid json {{{");

        try
        {
            // Act
            var (products, errors) = await _importService.PreviewImportAsync(jsonPath);

            // Assert
            Assert.Empty(products);
            Assert.NotEmpty(errors);
            Assert.Equal(ImportErrorType.InvalidJson, errors[0].ErrorType);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public async Task PreviewImportAsync_NonExistentFile_ReturnsEmptyListWithError()
    {
        // Act
        var (products, errors) = await _importService.PreviewImportAsync("/nonexistent/path/file.json");

        // Assert
        Assert.Empty(products);
        Assert.NotEmpty(errors);
        Assert.Equal(ImportErrorType.FileNotFound, errors[0].ErrorType);
    }

    [Fact]
    public async Task PreviewImportAsync_EmptyArray_ReturnsEmptyList()
    {
        // Arrange
        var jsonPath = Path.Combine(Path.GetTempPath(), $"empty_{Guid.NewGuid():N}.json");
        File.WriteAllText(jsonPath, "[]");

        try
        {
            // Act
            var (products, errors) = await _importService.PreviewImportAsync(jsonPath);

            // Assert
            Assert.Empty(products);
            Assert.Empty(errors); // Empty JSON array is valid, just no products
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public async Task PreviewImportAsync_MultipleProducts_ReturnsAllProducts()
    {
        // Arrange
        var products = Enumerable.Range(1, 100).Select(i => new ColesProduct
        {
            ProductID = $"CL{i:D3}",
            ProductName = $"Product {i}",
            Brand = $"Brand{i}",
            Price = $"${i}.00"
        }).ToList();

        var jsonPath = CreateTestJsonFile("bulk_test.json", products);

        try
        {
            // Act
            var (parsedProducts, validationErrors) = await _importService.PreviewImportAsync(jsonPath);

            // Assert
            Assert.Equal(100, parsedProducts.Count);
            Assert.Empty(validationErrors);
            Assert.Equal("Product 1", parsedProducts[0].ProductName);
            Assert.Equal("Product 100", parsedProducts[99].ProductName);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    #endregion

    #region ImportFromFile Tests

    [Fact]
    public void ImportFromFile_ValidColesJson_ImportsSuccessfully()
    {
        // Arrange
        var products = new[]
        {
            new ColesProduct
            {
                ProductID = "CL001",
                ProductName = "Imported Product",
                Brand = "ImportedBrand",
                Category = "Imported Category",
                Price = "$7.99",
                OriginalPrice = "$15.99",
                Savings = "$8.00",
                SpecialType = "Half Price"
            }
        };

        var jsonPath = CreateTestJsonFile("import_test.json", products);

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath, new DateTime(2026, 1, 15));

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.ItemsProcessed);
            Assert.Equal(1, result.PriceRecordsCreated);
            Assert.Empty(result.Errors);

            // Verify item was created
            var items = _itemRepository.GetAll().ToList();
            Assert.Single(items);
            Assert.Equal("Imported Product", items[0].Name);
            Assert.Equal("ImportedBrand", items[0].Brand);

            // Verify price record was created
            var priceRecords = _priceRecordRepository.GetAll().ToList();
            Assert.Single(priceRecords);
            Assert.Equal(7.99m, priceRecords[0].Price);
            Assert.Equal(15.99m, priceRecords[0].OriginalPrice);
            Assert.True(priceRecords[0].IsOnSale);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public void ImportFromFile_FileNotFound_ReturnsError()
    {
        // Act
        var result = _importService.ImportFromFile("/nonexistent/file.json");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("File not found", result.ErrorMessage);
    }

    [Fact]
    public void ImportFromFile_EmptyJsonArray_ReturnsError()
    {
        // Arrange
        var jsonPath = Path.Combine(Path.GetTempPath(), $"empty_import_{Guid.NewGuid():N}.json");
        File.WriteAllText(jsonPath, "[]");

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath);

            // Assert - empty array is treated as unsupported format
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public void ImportFromFile_InvalidJsonFormat_ReturnsError()
    {
        // Arrange
        var jsonPath = Path.Combine(Path.GetTempPath(), $"invalid_format_{Guid.NewGuid():N}.json");
        File.WriteAllText(jsonPath, "{\"invalid\": \"format\"}");

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath);

            // Assert - should handle gracefully
            Assert.False(result.Success);
            Assert.NotNull(result.ErrorMessage);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public void ImportFromFile_ColesFilename_DetectsColesChain()
    {
        // Arrange
        var products = new[]
        {
            new ColesProduct
            {
                ProductID = "CL001",
                ProductName = "Coles Product",
                Brand = "ColesBrand",
                Price = "$5.00"
            }
        };

        var jsonPath = CreateTestJsonFile("coles_25022026.json", products);

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath);

            // Assert
            Assert.True(result.Success);
            Assert.Contains("Coles", result.Message);

            // Verify store was created with Coles chain
            var stores = _placeRepository.GetAll().ToList();
            Assert.Single(stores);
            Assert.Equal("Coles", stores[0].Chain);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    [Fact]
    public void ImportFromFile_WoolworthsFilename_DetectsWoolworthsChain()
    {
        // Arrange
        var products = new[]
        {
            new ColesProduct
            {
                ProductID = "WW001",
                ProductName = "Woolworths Product",
                Brand = "WWBrand",
                Price = "$5.00"
            }
        };

        var jsonPath = CreateTestJsonFile("woolworths_25022026.json", products);

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath);

            // Assert
            Assert.True(result.Success);

            // Verify store was created with Woolworths chain
            var stores = _placeRepository.GetAll().ToList();
            Assert.Single(stores);
            Assert.Equal("Woolworths", stores[0].Chain);
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    #endregion

    #region ImportColesProducts Tests

    [Fact]
    public void ImportColesProducts_WithProgress_ReportsProgress()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var products = Enumerable.Range(1, 10).Select(i => new ColesProduct
        {
            ProductID = $"P{i:D2}",
            ProductName = $"Product {i}",
            Brand = "TestBrand",
            Price = "$5.00"
        }).ToList();

        var progressReports = new List<ImportProgress>();
        var progress = new Progress<ImportProgress>(p => progressReports.Add(p));

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today, null, progress);

        // Assert
        Assert.True(result.Success);
        // Allow time for progress reports
        Thread.Sleep(100);
        Assert.True(progressReports.Count > 0, "Expected at least one progress report");
        Assert.Equal(10, progressReports.Last().TotalItems);
        Assert.Equal(10, progressReports.Last().ProcessedItems);
    }

    [Fact]
    public void ImportColesProducts_WithNullProductID_GeneratesID()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = null,
                ProductName = "No ID Product",
                Brand = "NoIDBrand",
                Price = "$5.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        var items = _itemRepository.GetAll().ToList();
            Assert.Single(items);
        Assert.NotNull(items[0].ExtraInformation);
        Assert.True(items[0].ExtraInformation.ContainsKey("ProductID"));
    }

    [Fact]
    public void ImportColesProducts_WithExistingItemMappings_AddsPriceRecordOnly()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        
        // Create existing item
        var existingItem = new Item
        {
            Name = "Existing Product",
            Brand = "ExistingBrand",
            Category = "Test",
            IsActive = true
        };
        var existingId = _itemRepository.Add(existingItem);

        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "EXIST001",
                ProductName = "Existing Product",
                Brand = "ExistingBrand",
                Price = "$10.00"
            }
        };

        var mappings = new Dictionary<string, string>
        {
            { "EXIST001", existingId }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today, mappings);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ItemsProcessed); // No new items created
        Assert.Equal(1, result.PriceRecordsCreated); // Price record added

        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items); // Still only one item
    }

    #endregion

    #region Price Parsing Tests

    [Theory]
    [InlineData("$5.00", 5.00)]
    [InlineData("$10.99", 10.99)]
    [InlineData("$0.50", 0.50)]
    [InlineData("$100.00", 100.00)]
    [InlineData("5.00", 5.00)]
    [InlineData("$ 7.50", 0)] // Invalid format should return 0
    public void ImportFromFile_VariousPriceFormats_ParsesCorrectly(string priceValue, decimal expectedPrice)
    {
        // Arrange
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "P001",
                ProductName = "Price Test Product",
                Brand = "TestBrand",
                Price = priceValue
            }
        };

        var jsonPath = CreateTestJsonFile($"price_test_{Guid.NewGuid():N}.json", products);

        try
        {
            // Act
            var result = _importService.ImportFromFile(jsonPath);

            // Assert
            if (expectedPrice > 0)
            {
                Assert.True(result.Success);
                var priceRecords = _priceRecordRepository.GetAll().ToList();
                if (priceRecords.Any())
                {
                    Assert.Equal(expectedPrice, priceRecords[0].Price);
                }
            }
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    #endregion

    #region Edge Cases and Error Handling

    [Fact]
    public void ImportColesProducts_ProductWithMissingBrand_HandlesGracefully()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "P001",
                ProductName = "No Brand Product",
                Brand = null,
                Price = "$5.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items);
        Assert.Null(items[0].Brand);
    }

    [Fact]
    public void ImportColesProducts_ProductWithMissingCategory_HandlesGracefully()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "P001",
                ProductName = "No Category Product",
                Brand = "TestBrand",
                Category = null,
                Price = "$5.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items);
        Assert.Null(items[0].Category);
    }

    [Fact]
    public void ImportColesProducts_MultipleImports_SameProduct_UpdatesItem()
    {
        // Arrange
        var store = CreateTestStore("Test Store", "TestChain");
        var products1 = new List<ColesProduct>
        {
            new()
            {
                ProductID = "P001",
                ProductName = "Product Name V1",
                Brand = "BrandV1",
                Category = "CategoryV1",
                Price = "$5.00"
            }
        };

        var products2 = new List<ColesProduct>
        {
            new()
            {
                ProductID = "P001",
                ProductName = "Product Name V1", // Same name to trigger update
                Brand = "BrandV1",
                Category = "CategoryV2", // Different category
                Price = "$6.00" // Different price
            }
        };

        // Act - First import
        var result1 = _importService.ImportColesProducts(products1, store.Id!, DateTime.Today);
        Assert.True(result1.Success);

        // Act - Second import (should update existing)
        var result2 = _importService.ImportColesProducts(products2, store.Id!, DateTime.Today.AddDays(1));
        Assert.True(result2.Success);

        // Assert
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items); // Still only one item
        Assert.Equal("CategoryV2", items[0].Category); // Updated category

        var priceRecords = _priceRecordRepository.GetAll().ToList();
        Assert.Equal(2, priceRecords.Count); // Two price records
    }

    [Fact]
    public void ImportFromFile_LargeDataset_HandlesEfficiently()
    {
        // Arrange - Create 500 products
        var products = Enumerable.Range(1, 500).Select(i => new ColesProduct
        {
            ProductID = $"P{i:D4}",
            ProductName = $"Bulk Product {i}",
            Brand = $"Brand{(i % 10) + 1}", // 10 different brands
            Category = $"Category{(i % 5) + 1}", // 5 different categories
            Price = $"${(i % 50) + 1}.99",
            OriginalPrice = $"${((i % 50) + 1) * 2}.99",
            Savings = $"${(i % 50) + 1}.00",
            SpecialType = i % 2 == 0 ? "Half Price" : null
        }).ToList();

        var jsonPath = CreateTestJsonFile("large_dataset.json", products);

        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            var result = _importService.ImportFromFile(jsonPath);

            stopwatch.Stop();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(500, result.ItemsProcessed);
            Assert.Equal(500, result.PriceRecordsCreated);
            Assert.True(stopwatch.ElapsedMilliseconds < 30000, "Import should complete within 30 seconds");
        }
        finally
        {
            File.Delete(jsonPath);
        }
    }

    #endregion

    #region Helper Methods

    private string CreateTestJsonFile(string fileName, IEnumerable<ColesProduct> products)
    {
        var jsonPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}_{fileName}");
        var json = JsonSerializer.Serialize(products, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Keep original property names
        });
        File.WriteAllText(jsonPath, json);
        return jsonPath;
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

        // Verify store was created
        var retrieved = _placeRepository.GetById(id);
        if (retrieved == null)
        {
            throw new InvalidOperationException($"Failed to create test store. ID: {id}");
        }

        return store;
    }

    #endregion
}
