using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Tests for duplicate detection strategies in JsonImportService
/// </summary>
public class DuplicateDetectionTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _dbService;
    private readonly JsonImportService _importService;
    private readonly ItemRepository _itemRepository;
    private readonly PlaceRepository _placeRepository;

    public DuplicateDetectionTests()
    {
        // Create a unique test database path for each test run
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_duplicate_{Guid.NewGuid():N}.db");
        _dbService = new DatabaseService(_testDbPath);
        _importService = new JsonImportService(_dbService);
        _itemRepository = new ItemRepository(_dbService);
        _placeRepository = new PlaceRepository(_dbService);
    }

    public void Dispose()
    {
        // Clean up test database
        _dbService.Dispose();
        if (File.Exists(_testDbPath))
        {
            File.Delete(_testDbPath);
        }
    }

    #region Test: Create New Items (No Duplicates)

    [Fact]
    public void ImportColesProducts_NoExistingItems_CreatesNewItems()
    {
        // Arrange
        var store = CreateTestStore("Coles", "Coles");
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Test Product 1",
                Brand = "TestBrand",
                Category = "Test Category",
                Price = "$5.00",
                OriginalPrice = "$10.00",
                Savings = "$5.00",
                SpecialType = "Half Price"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        if (!result.Success)
        {
            var errorMsg = $"Import failed: {result.ErrorMessage}. Errors: {string.Join(", ", result.Errors)}";
            Assert.True(result.Success, errorMsg);
        }
        Assert.Equal(1, result.ItemsProcessed);
        Assert.Equal(1, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items);
        Assert.Equal("Test Product 1", items[0].Name);
        Assert.Equal("TestBrand", items[0].Brand);
    }

    [Fact]
    public void ImportColesProducts_MultipleNewItems_CreatesAllItems()
    {
        // Arrange
        var store = CreateTestStore("Coles", "Coles");
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Product A",
                Brand = "BrandA",
                Category = "Category A",
                Price = "$5.00"
            },
            new()
            {
                ProductID = "CL002",
                ProductName = "Product B",
                Brand = "BrandB",
                Category = "Category B",
                Price = "$3.00"
            },
            new()
            {
                ProductID = "CL003",
                ProductName = "Product C",
                Brand = "BrandC",
                Category = "Category C",
                Price = "$7.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(3, result.ItemsProcessed);
        Assert.Equal(3, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Equal(3, items.Count);
    }

    #endregion

    #region Test: Duplicate Detection by Name and Brand

    [Fact]
    public void ImportColesProducts_SameNameSameBrand_UpdatesExistingItem()
    {
        // Arrange - Create existing item
        var store = CreateTestStore("Coles", "Coles");
        var existingItem = new Item
        {
            Name = "Coca-Cola 2L",
            Brand = "Coca-Cola",
            Category = "Beverages",
            Description = "Original 2L bottle",
            IsActive = true
        };
        var existingId = _itemRepository.Add(existingItem);
        existingItem.Id = existingId;

        // Now import the same product
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Coca-Cola 2L",  // Same name
                Brand = "Coca-Cola",            // Same brand - should be detected as duplicate
                Category = "Beverages",
                Description = "Updated description",
                Price = "$3.50",
                OriginalPrice = "$4.50",
                Savings = "$1.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ItemsProcessed); // Should update existing
        Assert.Equal(1, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items); // Should still have only 1 item
        Assert.Equal(existingId, items[0].Id); // Should be the same item
        Assert.Equal("Updated description", items[0].Description); // Should be updated
    }

    [Fact]
    public void ImportColesProducts_SameNameDifferentBrand_CreatesNewItem()
    {
        // Arrange - Create existing item
        var store = CreateTestStore("Coles", "Coles");
        var existingItem = new Item
        {
            Name = "Cola 2L",
            Brand = "Coca-Cola",
            Category = "Beverages",
            IsActive = true
        };
        _itemRepository.Add(existingItem);

        // Now import similar product with different brand
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Cola 2L",        // Same name
                Brand = "Pepsi",                // Different brand - creates new item
                Category = "Beverages",
                Price = "$3.00"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert - Different brand creates new item (brand filter excludes match)
        Assert.True(result.Success);
        Assert.Equal(1, result.ItemsProcessed); // 1 new item
        Assert.Equal(1, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Equal(2, items.Count); // 2 separate items
        Assert.Contains(items, i => i.Brand == "Coca-Cola");
        Assert.Contains(items, i => i.Brand == "Pepsi");
    }

    [Fact]
    public void ImportColesProducts_DifferentNameSameBrand_CreatesNewItem()
    {
        // Arrange - Create existing item
        var store = CreateTestStore("Coles", "Coles");
        var existingItem = new Item
        {
            Name = "Coca-Cola 2L",
            Brand = "Coca-Cola",
            Category = "Beverages",
            IsActive = true
        };
        _itemRepository.Add(existingItem);

        // Now import different product with same brand but different name
        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Coca-Cola 1.25L",  // Different name (size)
                Brand = "Coca-Cola",              // Same brand
                Category = "Beverages",
                Price = "$2.50"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert - Different name creates new item (no name match found)
        Assert.True(result.Success);
        Assert.Equal(1, result.ItemsProcessed); // 1 new item created
        Assert.Equal(1, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Equal(2, items.Count); // 1 existing + 1 new in repository
    }

    #endregion

    #region Test: Price Record Only Mode

    [Fact]
    public void ImportColesProducts_ExistingItemMapping_AddsPriceRecordOnly()
    {
        // Arrange - Create existing item
        var store = CreateTestStore("Coles", "Coles");
        var existingItem = new Item
        {
            Name = "Test Product",
            Brand = "TestBrand",
            Category = "Test",
            IsActive = true
        };
        var existingId = _itemRepository.Add(existingItem);

        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "Test Product",
                Brand = "TestBrand",
                Category = "Test",
                Price = "$10.00"
            }
        };

        // Create mapping to use existing item
        var existingItemMappings = new Dictionary<string, string>
        {
            { "CL001", existingId }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today, existingItemMappings);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ItemsProcessed); // No new items created
        Assert.Equal(1, result.PriceRecordsCreated); // But price record added
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items); // Still only 1 item
    }

    [Fact]
    public void ImportColesProducts_MixedMappings_HandlesCorrectly()
    {
        // Arrange
        var store = CreateTestStore("Coles", "Coles");
        
        // Create one existing item
        var existingItem = new Item
        {
            Name = "Existing Product",
            Brand = "BrandA",
            Category = "Test",
            IsActive = true
        };
        var existingId = _itemRepository.Add(existingItem);

        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",  // Will use existing item
                ProductName = "Existing Product",
                Brand = "BrandA",
                Category = "Test",
                Price = "$5.00"
            },
            new()
            {
                ProductID = "CL002",  // Will create new item
                ProductName = "New Product",
                Brand = "BrandB",
                Category = "Test",
                Price = "$7.00"
            }
        };

        var existingItemMappings = new Dictionary<string, string>
        {
            { "CL001", existingId }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today, existingItemMappings);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.ItemsProcessed); // Only CL002 creates new item
        Assert.Equal(2, result.PriceRecordsCreated); // Both get price records
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Equal(2, items.Count);
    }

    #endregion

    #region Test: Case Sensitivity

    [Fact]
    public void ImportColesProducts_CaseInsensitiveNameMatch_UpdatesExistingItem()
    {
        // Arrange
        var store = CreateTestStore("Coles", "Coles");
        var existingItem = new Item
        {
            Name = "COCA-COLA 2L",
            Brand = "Coca-Cola",
            Category = "Beverages",
            IsActive = true
        };
        _itemRepository.Add(existingItem);

        var products = new List<ColesProduct>
        {
            new()
            {
                ProductID = "CL001",
                ProductName = "coca-cola 2l",  // Different case
                Brand = "Coca-Cola",
                Category = "Beverages",
                Price = "$3.50"
            }
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert - Based on current implementation, search is case-insensitive in LiteDB
        // But update will happen regardless since search finds the item
        Assert.True(result.Success);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Single(items); // Should update, not create new
    }

    #endregion

    #region Test: Batch Import with Duplicates

    [Fact]
    public void ImportColesProducts_BatchWithDuplicates_ProcessesCorrectly()
    {
        // Arrange - Create some existing items
        var store = CreateTestStore("Coles", "Coles");
        
        // Pre-create 2 items
        _itemRepository.Add(new Item
        {
            Name = "Product 1",
            Brand = "Brand1",
            Category = "Test",
            IsActive = true
        });
        _itemRepository.Add(new Item
        {
            Name = "Product 2",
            Brand = "Brand2",
            Category = "Test",
            IsActive = true
        });

        // Import 5 items: 2 duplicates, 3 new
        var products = new List<ColesProduct>
        {
            new() { ProductID = "P1", ProductName = "Product 1", Brand = "Brand1", Price = "$1.00" }, // Duplicate
            new() { ProductID = "P2", ProductName = "Product 2", Brand = "Brand2", Price = "$2.00" }, // Duplicate
            new() { ProductID = "P3", ProductName = "Product 3", Brand = "Brand3", Price = "$3.00" }, // New
            new() { ProductID = "P4", ProductName = "Product 4", Brand = "Brand4", Price = "$4.00" }, // New
            new() { ProductID = "P5", ProductName = "Product 5", Brand = "Brand5", Price = "$5.00" }  // New
        };

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(5, result.ItemsProcessed);
        Assert.Equal(5, result.PriceRecordsCreated);
        
        var items = _itemRepository.GetAll().ToList();
        Assert.Equal(5, items.Count); // 2 updated + 3 new = 5 total
    }

    #endregion

    #region Test: Error Handling

    [Fact]
    public void ImportColesProducts_InvalidStore_ReturnsError()
    {
        // Arrange
        var products = new List<ColesProduct>
        {
            new() { ProductID = "CL001", ProductName = "Test", Brand = "Test", Price = "$5.00" }
        };

        // Act - Use non-existent store ID
        var result = _importService.ImportColesProducts(products, "non-existent-id", DateTime.Today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Store", result.ErrorMessage);
    }

    [Fact]
    public void ImportColesProducts_EmptyProductList_ReturnsError()
    {
        // Arrange
        var store = CreateTestStore("Coles", "Coles");
        var products = new List<ColesProduct>();

        // Act
        var result = _importService.ImportColesProducts(products, store.Id!, DateTime.Today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("No products", result.ErrorMessage);
    }

    #endregion

    #region Helper Methods

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
        
        // Verify the store was created and can be retrieved
        var retrieved = _placeRepository.GetById(id);
        if (retrieved == null)
        {
            throw new InvalidOperationException($"Failed to create test store. ID: {id}");
        }
        
        return store;
    }

    #endregion
}
