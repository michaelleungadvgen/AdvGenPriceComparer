using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Repositories
{
    [CollectionDefinition("ItemRepositoryTests", DisableParallelization = true)]
    public class ItemRepositoryCollection : ICollectionFixture<object>
    {
    }

    [Collection("ItemRepositoryTests")]
    public class ItemRepositoryTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _database;
        private readonly ItemRepository _repository;

        public ItemRepositoryTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"AdvGenItemTests_{Guid.NewGuid()}.db");
            _database = new DatabaseService(_testDbPath);
            _repository = new ItemRepository(_database);
        }

        public void Dispose()
        {
            _database?.Dispose();
            try
            {
                if (File.Exists(_testDbPath))
                {
                    File.Delete(_testDbPath);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        #region Add Tests

        [Fact]
        public void Add_ValidItem_ReturnsId()
        {
            // Arrange
            var item = new Item
            {
                Name = "Test Milk",
                Brand = "TestBrand",
                Category = "Dairy",
                Barcode = "1234567890123"
            };

            // Act
            var id = _repository.Add(item);

            // Assert
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        public void Add_Item_SetsDateAddedAndLastUpdated()
        {
            // Arrange
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var item = new Item
            {
                Name = "Test Bread",
                Brand = "BakeryCo"
            };

            // Act
            _repository.Add(item);

            // Assert
            Assert.True(item.DateAdded >= beforeAdd);
            Assert.True(item.LastUpdated >= beforeAdd);
        }

        [Fact]
        public void Add_MultipleItems_AllAddedSuccessfully()
        {
            // Arrange
            var item1 = new Item { Name = "Item 1" };
            var item2 = new Item { Name = "Item 2" };
            var item3 = new Item { Name = "Item 3" };

            // Act
            var id1 = _repository.Add(item1);
            var id2 = _repository.Add(item2);
            var id3 = _repository.Add(item3);

            // Assert
            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id2, id3);
            Assert.Equal(3, _repository.GetTotalCount());
        }

        #endregion

        #region GetById Tests

        [Fact]
        public void GetById_ExistingItem_ReturnsItem()
        {
            // Arrange
            var item = new Item
            {
                Name = "Test Cheese",
                Brand = "DairyFarm",
                Category = "Dairy"
            };
            var id = _repository.Add(item);

            // Act
            var result = _repository.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Cheese", result.Name);
            Assert.Equal("DairyFarm", result.Brand);
            Assert.Equal("Dairy", result.Category);
        }

        [Fact]
        public void GetById_NonExistingItem_ReturnsNull()
        {
            // Act
            var result = _repository.GetById("nonexistent123");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetById_InvalidId_ReturnsNull()
        {
            // Act
            var result = _repository.GetById("invalid-id-format");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetAll Tests

        [Fact]
        public void GetAll_NoItems_ReturnsEmpty()
        {
            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WithItems_ReturnsAllActiveItems()
        {
            // Arrange
            _repository.Add(new Item { Name = "Item 1" });
            _repository.Add(new Item { Name = "Item 2" });
            _repository.Add(new Item { Name = "Item 3" });

            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void GetAll_DoesNotReturnSoftDeletedItems()
        {
            // Arrange
            var item = new Item { Name = "ToDelete" };
            var id = _repository.Add(item);
            _repository.SoftDelete(id);

            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ExistingItem_ReturnsTrue()
        {
            // Arrange
            var item = new Item { Name = "Original Name" };
            var id = _repository.Add(item);
            var retrievedItem = _repository.GetById(id)!;
            
            // Act
            retrievedItem.Name = "Updated Name";
            var result = _repository.Update(retrievedItem);

            // Assert
            Assert.True(result);
            var updatedItem = _repository.GetById(id);
            Assert.Equal("Updated Name", updatedItem?.Name);
        }

        [Fact]
        public void Update_UpdatesLastUpdatedTimestamp()
        {
            // Arrange
            var item = new Item { Name = "Test Item" };
            var id = _repository.Add(item);
            var retrievedItem = _repository.GetById(id)!;
            var originalUpdated = retrievedItem.LastUpdated;
            
            System.Threading.Thread.Sleep(100); // Ensure time difference
            
            // Act
            retrievedItem.Name = "Updated";
            _repository.Update(retrievedItem);

            // Assert
            var updatedItem = _repository.GetById(id);
            Assert.True(updatedItem?.LastUpdated > originalUpdated);
        }

        [Fact]
        public void Update_NonExistingItem_ReturnsFalse()
        {
            // Arrange
            var item = new Item { Id = "nonexistent123", Name = "Test" };

            // Act
            var result = _repository.Update(item);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_ExistingItem_RemovesItem()
        {
            // Arrange
            var item = new Item { Name = "ToDelete" };
            var id = _repository.Add(item);

            // Act
            var result = _repository.Delete(id);

            // Assert
            Assert.True(result);
            Assert.Null(_repository.GetById(id));
        }

        [Fact]
        public void Delete_NonExistingItem_ReturnsFalse()
        {
            // Act
            var result = _repository.Delete("nonexistent123");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region SoftDelete Tests

        [Fact]
        public void SoftDelete_ExistingItem_MarksAsInactive()
        {
            // Arrange
            var item = new Item { Name = "ToSoftDelete" };
            var id = _repository.Add(item);

            // Act
            var result = _repository.SoftDelete(id);

            // Assert
            Assert.True(result);
            // Note: GetById still returns soft-deleted items in current implementation
            // but GetAll/GetTotalCount filter them out
        }

        [Fact]
        public void SoftDelete_ItemStillExistsInDatabase()
        {
            // Arrange
            var item = new Item { Name = "SoftDeletedItem" };
            var id = _repository.Add(item);

            // Act
            _repository.SoftDelete(id);

            // Assert - Item should be filtered from GetAll and GetTotalCount
            Assert.Empty(_repository.GetAll());
            Assert.Equal(0, _repository.GetTotalCount());
        }

        #endregion

        #region SearchByName Tests

        [Fact]
        public void SearchByName_ExistingItem_ReturnsMatchingItems()
        {
            // Arrange
            _repository.Add(new Item { Name = "Whole Milk" });
            _repository.Add(new Item { Name = "Skim Milk" });
            _repository.Add(new Item { Name = "White Bread" });

            // Act
            var result = _repository.SearchByName("Milk").ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, item => Assert.Contains("Milk", item.Name));
        }

        [Fact]
        public void SearchByName_CaseInsensitive_ReturnsMatches()
        {
            // Arrange
            _repository.Add(new Item { Name = "WHOLE MILK" });

            // Act
            var result = _repository.SearchByName("milk").ToList();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void SearchByName_NoMatch_ReturnsEmpty()
        {
            // Arrange
            _repository.Add(new Item { Name = "Bread" });

            // Act
            var result = _repository.SearchByName("Cheese");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByCategory Tests

        [Fact]
        public void GetByCategory_ExistingCategory_ReturnsItems()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Category = "Dairy" });
            _repository.Add(new Item { Name = "Cheese", Category = "Dairy" });
            _repository.Add(new Item { Name = "Bread", Category = "Bakery" });

            // Act
            var result = _repository.GetByCategory("Dairy").ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetByCategory_NoItemsInCategory_ReturnsEmpty()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Category = "Dairy" });

            // Act
            var result = _repository.GetByCategory("Produce");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByBrand Tests

        [Fact]
        public void GetByBrand_ExistingBrand_ReturnsItems()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Brand = "BrandA" });
            _repository.Add(new Item { Name = "Cheese", Brand = "BrandA" });
            _repository.Add(new Item { Name = "Bread", Brand = "BrandB" });

            // Act
            var result = _repository.GetByBrand("BrandA").ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetByBarcode Tests

        [Fact]
        public void GetByBarcode_ExistingBarcode_ReturnsItem()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Barcode = "1234567890123" });

            // Act
            var result = _repository.GetByBarcode("1234567890123").ToList();

            // Assert
            Assert.Single(result);
        }

        #endregion

        #region GetAllCategories Tests

        [Fact]
        public void GetAllCategories_ReturnsDistinctCategories()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Category = "Dairy" });
            _repository.Add(new Item { Name = "Cheese", Category = "Dairy" });
            _repository.Add(new Item { Name = "Bread", Category = "Bakery" });

            // Act
            var result = _repository.GetAllCategories().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("Dairy", result);
            Assert.Contains("Bakery", result);
        }

        [Fact]
        public void GetAllCategories_SortedAlphabetically()
        {
            // Arrange
            _repository.Add(new Item { Name = "Item1", Category = "Zebra" });
            _repository.Add(new Item { Name = "Item2", Category = "Apple" });

            // Act
            var result = _repository.GetAllCategories().ToList();

            // Assert
            Assert.Equal("Apple", result[0]);
            Assert.Equal("Zebra", result[1]);
        }

        #endregion

        #region GetAllBrands Tests

        [Fact]
        public void GetAllBrands_ReturnsDistinctBrands()
        {
            // Arrange
            _repository.Add(new Item { Name = "Milk", Brand = "BrandA" });
            _repository.Add(new Item { Name = "Cheese", Brand = "BrandA" });
            _repository.Add(new Item { Name = "Bread", Brand = "BrandB" });

            // Act
            var result = _repository.GetAllBrands().ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetTotalCount Tests

        [Fact]
        public void GetTotalCount_EmptyDatabase_ReturnsZero()
        {
            // Act
            var result = _repository.GetTotalCount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetTotalCount_WithItems_ReturnsCount()
        {
            // Arrange
            _repository.Add(new Item { Name = "Item1" });
            _repository.Add(new Item { Name = "Item2" });

            // Act
            var result = _repository.GetTotalCount();

            // Assert
            Assert.Equal(2, result);
        }

        [Fact]
        public void GetTotalCount_AfterSoftDelete_Decreases()
        {
            // Arrange
            var id = _repository.Add(new Item { Name = "Item1" });
            _repository.Add(new Item { Name = "Item2" });

            // Act
            _repository.SoftDelete(id);
            var result = _repository.GetTotalCount();

            // Assert
            Assert.Equal(1, result);
        }

        #endregion

        #region GetRecentlyAdded Tests

        [Fact]
        public void GetRecentlyAdded_ReturnsMostRecentFirst()
        {
            // Arrange
            _repository.Add(new Item { Name = "First" });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new Item { Name = "Second" });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new Item { Name = "Third" });

            // Act
            var result = _repository.GetRecentlyAdded(2).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Third", result[0].Name);
            Assert.Equal("Second", result[1].Name);
        }

        #endregion

        #region GetRecentlyUpdated Tests

        [Fact]
        public void GetRecentlyUpdated_ReturnsMostRecentlyUpdated()
        {
            // Arrange
            var id1 = _repository.Add(new Item { Name = "First" });
            System.Threading.Thread.Sleep(50);
            var id2 = _repository.Add(new Item { Name = "Second" });
            
            // Update first item to make it most recently updated
            System.Threading.Thread.Sleep(50);
            var item1 = _repository.GetById(id1)!;
            item1.Name = "First Updated";
            _repository.Update(item1);

            // Act
            var result = _repository.GetRecentlyUpdated(2).ToList();

            // Assert
            Assert.Equal("First Updated", result[0].Name);
        }

        #endregion
    }
}
