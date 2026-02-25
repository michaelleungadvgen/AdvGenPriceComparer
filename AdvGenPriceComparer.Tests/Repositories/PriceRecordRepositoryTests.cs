using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using LiteDB;
using Xunit;

namespace AdvGenPriceComparer.Tests.Repositories
{
    [CollectionDefinition("PriceRecordRepositoryTests", DisableParallelization = true)]
    public class PriceRecordRepositoryCollection : ICollectionFixture<object>
    {
    }

    [Collection("PriceRecordRepositoryTests")]
    public class PriceRecordRepositoryTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _database;
        private readonly PriceRecordRepository _repository;
        private readonly ItemRepository _itemRepository;
        private readonly PlaceRepository _placeRepository;

        public PriceRecordRepositoryTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"AdvGenPriceRecordTests_{Guid.NewGuid()}.db");
            _database = new DatabaseService(_testDbPath);
            _repository = new PriceRecordRepository(_database);
            _itemRepository = new ItemRepository(_database);
            _placeRepository = new PlaceRepository(_database);
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

        private string CreateTestItem(string name = "Test Item")
        {
            var item = new Item { Name = name };
            return _itemRepository.Add(item);
        }

        private string CreateTestPlace(string name = "Test Store")
        {
            var place = new Place { Name = name, Chain = "TestChain" };
            return _placeRepository.Add(place);
        }

        #region Add Tests

        [Fact]
        public void Add_ValidPriceRecord_ReturnsId()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var record = new PriceRecord
            {
                ItemId = itemId,
                PlaceId = placeId,
                Price = 5.99m
            };

            // Act
            var id = _repository.Add(record);

            // Assert
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        public void Add_PriceRecord_SetsDateRecorded()
        {
            // Arrange
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var record = new PriceRecord
            {
                ItemId = itemId,
                PlaceId = placeId,
                Price = 10.00m
            };

            // Act
            _repository.Add(record);

            // Assert
            Assert.True(record.DateRecorded >= beforeAdd);
        }

        [Fact]
        public void Add_MultiplePriceRecords_AllAddedSuccessfully()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();

            // Act
            var id1 = _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            var id2 = _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });
            var id3 = _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m });

            // Assert
            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id2, id3);
            Assert.Equal(3, _repository.GetTotalRecordsCount());
        }

        #endregion

        #region GetById Tests

        [Fact]
        public void GetById_ExistingRecord_ReturnsRecord()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var record = new PriceRecord
            {
                ItemId = itemId,
                PlaceId = placeId,
                Price = 9.99m,
                IsOnSale = true,
                SaleDescription = "Half Price"
            };
            var id = _repository.Add(record);

            // Act
            var result = _repository.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(9.99m, result.Price);
            Assert.True(result.IsOnSale);
            Assert.Equal("Half Price", result.SaleDescription);
        }

        [Fact]
        public void GetById_NonExistingRecord_ReturnsNull()
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
        public void GetAll_NoRecords_ReturnsEmpty()
        {
            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WithRecords_ReturnsAllRecords()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });

            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var record = new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m };
            var id = _repository.Add(record);
            var retrievedRecord = _repository.GetById(id)!;
            
            // Act
            retrievedRecord.Price = 10.00m;
            var result = _repository.Update(retrievedRecord);

            // Assert
            Assert.True(result);
            var updatedRecord = _repository.GetById(id);
            Assert.Equal(10.00m, updatedRecord?.Price);
        }

        [Fact]
        public void Update_NonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var record = new PriceRecord { Id = "nonexistent123", ItemId = "item1", PlaceId = "place1", Price = 5.00m };

            // Act
            var result = _repository.Update(record);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_ExistingRecord_RemovesRecord()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var record = new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m };
            var id = _repository.Add(record);

            // Act
            var result = _repository.Delete(id);

            // Assert
            Assert.True(result);
            Assert.Null(_repository.GetById(id));
        }

        [Fact]
        public void Delete_NonExistingRecord_ReturnsFalse()
        {
            // Act
            var result = _repository.Delete("nonexistent123");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetByItem Tests

        [Fact]
        public void GetByItem_ExistingItem_ReturnsRecordsOrderedByDate()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });

            // Act
            var result = _repository.GetByItem(itemId).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[0].DateRecorded >= result[1].DateRecorded); // Most recent first
        }

        [Fact]
        public void GetByItem_InvalidItemId_ReturnsEmpty()
        {
            // Act
            var result = _repository.GetByItem("invalid-id");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByPlace Tests

        [Fact]
        public void GetByPlace_ExistingPlace_ReturnsRecordsOrderedByDate()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });

            // Act
            var result = _repository.GetByPlace(placeId).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.True(result[0].DateRecorded >= result[1].DateRecorded);
        }

        [Fact]
        public void GetByPlace_InvalidPlaceId_ReturnsEmpty()
        {
            // Act
            var result = _repository.GetByPlace("invalid-id");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByItemAndPlace Tests

        [Fact]
        public void GetByItemAndPlace_ValidIds_ReturnsRecords()
        {
            // Arrange
            var itemId1 = CreateTestItem("Item 1");
            var itemId2 = CreateTestItem("Item 2");
            var placeId1 = CreateTestPlace("Store 1");
            var placeId2 = CreateTestPlace("Store 2");
            
            _repository.Add(new PriceRecord { ItemId = itemId1, PlaceId = placeId1, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId1, PlaceId = placeId2, Price = 6.00m });
            _repository.Add(new PriceRecord { ItemId = itemId2, PlaceId = placeId1, Price = 7.00m });

            // Act
            var result = _repository.GetByItemAndPlace(itemId1, placeId1).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(5.00m, result[0].Price);
        }

        #endregion

        #region GetLatestPrice Tests

        [Fact]
        public void GetLatestPrice_MultipleRecords_ReturnsMostRecent()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m });

            // Act
            var result = _repository.GetLatestPrice(itemId, placeId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(7.00m, result.Price);
        }

        [Fact]
        public void GetLatestPrice_NoRecords_ReturnsNull()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();

            // Act
            var result = _repository.GetLatestPrice(itemId, placeId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetCurrentSales Tests

        [Fact]
        public void GetCurrentSales_ReturnsActiveSales()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 5.00m,
                IsOnSale = true,
                ValidFrom = DateTime.UtcNow.AddDays(-1),
                ValidTo = DateTime.UtcNow.AddDays(7)
            });
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 10.00m,
                IsOnSale = false
            });

            // Act
            var result = _repository.GetCurrentSales().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal(5.00m, result[0].Price);
        }

        [Fact]
        public void GetCurrentSales_ExpiredSale_NotIncluded()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 5.00m,
                IsOnSale = true,
                ValidFrom = DateTime.UtcNow.AddDays(-10),
                ValidTo = DateTime.UtcNow.AddDays(-5) // Expired
            });

            // Act
            var result = _repository.GetCurrentSales();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetPriceHistory Tests

        [Fact]
        public void GetPriceHistory_WithDateRange_ReturnsFilteredRecords()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var now = DateTime.UtcNow;
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 5.00m,
                DateRecorded = now.AddDays(-10)
            });
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 6.00m,
                DateRecorded = now.AddDays(-5)
            });
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 7.00m,
                DateRecorded = now
            });

            // Act - Get records from 7 days ago onwards (should include today and -5 days)
            var result = _repository.GetPriceHistory(itemId, now.AddDays(-7), now.AddDays(1)).ToList();

            // Assert - Should get the records from -5 days and today (within range)
            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void GetPriceHistory_OrderedByDate()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            var now = DateTime.UtcNow;
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 5.00m,
                DateRecorded = now.AddDays(-5)
            });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 7.00m,
                DateRecorded = now
            });

            // Act
            var result = _repository.GetPriceHistory(itemId).ToList();

            // Assert
            Assert.True(result.Count >= 2);
            // Results should be in chronological order (oldest first)
            Assert.True(result[0].DateRecorded <= result[result.Count - 1].DateRecorded);
        }

        #endregion

        #region GetLowestPrice Tests

        [Fact]
        public void GetLowestPrice_MultipleRecords_ReturnsLowest()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 10.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m });

            // Act
            var result = _repository.GetLowestPrice(itemId);

            // Assert
            Assert.Equal(5.00m, result);
        }

        [Fact]
        public void GetLowestPrice_NoRecords_ReturnsNull()
        {
            // Arrange
            var itemId = CreateTestItem();

            // Act
            var result = _repository.GetLowestPrice(itemId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetHighestPrice Tests

        [Fact]
        public void GetHighestPrice_MultipleRecords_ReturnsHighest()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 15.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 10.00m });

            // Act
            var result = _repository.GetHighestPrice(itemId);

            // Assert
            Assert.Equal(15.00m, result);
        }

        #endregion

        #region GetAveragePrice Tests

        [Fact]
        public void GetAveragePrice_MultipleRecords_ReturnsAverage()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 10.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 15.00m });

            // Act
            var result = _repository.GetAveragePrice(itemId);

            // Assert
            Assert.Equal(10.00m, result);
        }

        [Fact]
        public void GetAveragePrice_NoRecords_ReturnsNull()
        {
            // Arrange
            var itemId = CreateTestItem();

            // Act
            var result = _repository.GetAveragePrice(itemId);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetBestDeals Tests

        [Fact]
        public void GetBestDeals_ReturnsItemsWithHighestDiscount()
        {
            // Arrange
            var itemId1 = CreateTestItem("Item 1");
            var itemId2 = CreateTestItem("Item 2");
            var itemId3 = CreateTestItem("Item 3");
            var placeId = CreateTestPlace();
            
            // 50% discount
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId1, 
                PlaceId = placeId, 
                Price = 5.00m,
                OriginalPrice = 10.00m,
                IsOnSale = true
            });
            // 25% discount
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId2, 
                PlaceId = placeId, 
                Price = 7.50m,
                OriginalPrice = 10.00m,
                IsOnSale = true
            });
            // 10% discount
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId3, 
                PlaceId = placeId, 
                Price = 9.00m,
                OriginalPrice = 10.00m,
                IsOnSale = true
            });

            // Act
            var result = _repository.GetBestDeals(2).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(5.00m, result[0].Price); // Highest discount first
        }

        [Fact]
        public void GetBestDeals_NotOnSale_NotIncluded()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord 
            { 
                ItemId = itemId, 
                PlaceId = placeId, 
                Price = 5.00m,
                IsOnSale = false
            });

            // Act
            var result = _repository.GetBestDeals();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region ComparePrices Tests

        [Fact]
        public void ComparePrices_ReturnsLatestPricePerPlace_OrderedByPrice()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId1 = CreateTestPlace("Store 1");
            var placeId2 = CreateTestPlace("Store 2");
            var placeId3 = CreateTestPlace("Store 3");
            var now = DateTime.UtcNow;
            
            // Older prices (should be ignored in favor of newer ones)
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId1, Price = 15.00m, DateRecorded = now.AddDays(-5) });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId2, Price = 20.00m, DateRecorded = now.AddDays(-5) });
            
            // Latest prices
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId1, Price = 12.00m, DateRecorded = now });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId2, Price = 10.00m, DateRecorded = now.AddMilliseconds(10) });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId3, Price = 15.00m, DateRecorded = now.AddMilliseconds(20) });

            // Act
            var result = _repository.ComparePrices(itemId).ToList();

            // Assert - Should have one price per place, sorted by price ascending
            Assert.Equal(3, result.Count);
            // Verify the lowest price is first (sorted by price)
            Assert.True(result[0].Price <= result[1].Price);
            Assert.True(result[1].Price <= result[2].Price);
        }

        #endregion

        #region GetTotalRecordsCount Tests

        [Fact]
        public void GetTotalRecordsCount_EmptyDatabase_ReturnsZero()
        {
            // Act
            var result = _repository.GetTotalRecordsCount();

            // Assert
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetTotalRecordsCount_WithRecords_ReturnsCount()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });

            // Act
            var result = _repository.GetTotalRecordsCount();

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region GetRecordsCountThisWeek Tests

        [Fact]
        public void GetRecordsCountThisWeek_RecentRecords_ReturnsCount()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });

            // Act
            var result = _repository.GetRecordsCountThisWeek();

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region GetSaleRecordsCount Tests

        [Fact]
        public void GetSaleRecordsCount_OnlyCountsOnSale()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m, IsOnSale = true });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m, IsOnSale = false });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m, IsOnSale = true });

            // Act
            var result = _repository.GetSaleRecordsCount();

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region GetRecentPriceUpdates Tests

        [Fact]
        public void GetRecentPriceUpdates_ReturnsMostRecentFirst()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m });
            System.Threading.Thread.Sleep(50);
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m });

            // Act
            var result = _repository.GetRecentPriceUpdates(2).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal(7.00m, result[0].Price);
            Assert.Equal(6.00m, result[1].Price);
        }

        #endregion

        #region GetPriceRecordsBySource Tests

        [Fact]
        public void GetPriceRecordsBySource_ReturnsGroupedCounts()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m, Source = "catalogue" });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m, Source = "catalogue" });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m, Source = "manual" });

            // Act
            var result = _repository.GetPriceRecordsBySource();

            // Assert
            Assert.Equal(2, result["catalogue"]);
            Assert.Equal(1, result["manual"]);
        }

        [Fact]
        public void GetPriceRecordsBySource_NoSource_NotIncluded()
        {
            // Arrange
            var itemId = CreateTestItem();
            var placeId = CreateTestPlace();
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 5.00m, Source = null });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 6.00m, Source = "" });
            _repository.Add(new PriceRecord { ItemId = itemId, PlaceId = placeId, Price = 7.00m, Source = "catalogue" });

            // Act
            var result = _repository.GetPriceRecordsBySource();

            // Assert
            Assert.Single(result);
            Assert.Equal(1, result["catalogue"]);
        }

        #endregion
    }
}
