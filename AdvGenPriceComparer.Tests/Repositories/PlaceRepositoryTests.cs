using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Repositories
{
    [CollectionDefinition("PlaceRepositoryTests", DisableParallelization = true)]
    public class PlaceRepositoryCollection : ICollectionFixture<object>
    {
    }

    [Collection("PlaceRepositoryTests")]
    public class PlaceRepositoryTests : IDisposable
    {
        private readonly string _testDbPath;
        private readonly DatabaseService _database;
        private readonly PlaceRepository _repository;

        public PlaceRepositoryTests()
        {
            _testDbPath = Path.Combine(Path.GetTempPath(), $"AdvGenPlaceTests_{Guid.NewGuid()}.db");
            _database = new DatabaseService(_testDbPath);
            _repository = new PlaceRepository(_database);
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
        public void Add_ValidPlace_ReturnsId()
        {
            // Arrange
            var place = new Place
            {
                Name = "Coles Brisbane CBD",
                Chain = "Coles",
                Address = "123 Main St",
                Suburb = "Brisbane",
                State = "QLD",
                Postcode = "4000"
            };

            // Act
            var id = _repository.Add(place);

            // Assert
            Assert.NotNull(id);
            Assert.NotEmpty(id);
        }

        [Fact]
        public void Add_Place_SetsDateAdded()
        {
            // Arrange
            var beforeAdd = DateTime.UtcNow.AddSeconds(-1);
            var place = new Place
            {
                Name = "Test Store",
                Chain = "TestChain"
            };

            // Act
            _repository.Add(place);

            // Assert
            Assert.True(place.DateAdded >= beforeAdd);
        }

        [Fact]
        public void Add_MultiplePlaces_AllAddedSuccessfully()
        {
            // Arrange
            var place1 = new Place { Name = "Store 1", Chain = "ChainA" };
            var place2 = new Place { Name = "Store 2", Chain = "ChainB" };
            var place3 = new Place { Name = "Store 3", Chain = "ChainA" };

            // Act
            var id1 = _repository.Add(place1);
            var id2 = _repository.Add(place2);
            var id3 = _repository.Add(place3);

            // Assert
            Assert.NotEqual(id1, id2);
            Assert.NotEqual(id2, id3);
            Assert.Equal(3, _repository.GetTotalCount());
        }

        #endregion

        #region GetById Tests

        [Fact]
        public void GetById_ExistingPlace_ReturnsPlace()
        {
            // Arrange
            var place = new Place
            {
                Name = "Woolworths Sydney",
                Chain = "Woolworths",
                Address = "456 George St",
                Suburb = "Sydney",
                State = "NSW",
                Postcode = "2000"
            };
            var id = _repository.Add(place);

            // Act
            var result = _repository.GetById(id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Woolworths Sydney", result.Name);
            Assert.Equal("Woolworths", result.Chain);
            Assert.Equal("Sydney", result.Suburb);
        }

        [Fact]
        public void GetById_NonExistingPlace_ReturnsNull()
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
        public void GetAll_NoPlaces_ReturnsEmpty()
        {
            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetAll_WithPlaces_ReturnsAllActivePlaces()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "ChainA" });
            _repository.Add(new Place { Name = "Store 2", Chain = "ChainB" });

            // Act
            var result = _repository.GetAll().ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetAll_DoesNotReturnSoftDeletedPlaces()
        {
            // Arrange
            var place = new Place { Name = "ToDelete", Chain = "ChainA" };
            var id = _repository.Add(place);
            _repository.SoftDelete(id);

            // Act
            var result = _repository.GetAll();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Update Tests

        [Fact]
        public void Update_ExistingPlace_ReturnsTrue()
        {
            // Arrange
            var place = new Place { Name = "Original Name", Chain = "ChainA" };
            var id = _repository.Add(place);
            var retrievedPlace = _repository.GetById(id)!;
            
            // Act
            retrievedPlace.Name = "Updated Name";
            var result = _repository.Update(retrievedPlace);

            // Assert
            Assert.True(result);
            var updatedPlace = _repository.GetById(id);
            Assert.Equal("Updated Name", updatedPlace?.Name);
        }

        [Fact]
        public void Update_NonExistingPlace_ReturnsFalse()
        {
            // Arrange
            var place = new Place { Id = "nonexistent123", Name = "Test", Chain = "ChainA" };

            // Act
            var result = _repository.Update(place);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Delete Tests

        [Fact]
        public void Delete_ExistingPlace_RemovesPlace()
        {
            // Arrange
            var place = new Place { Name = "ToDelete", Chain = "ChainA" };
            var id = _repository.Add(place);

            // Act
            var result = _repository.Delete(id);

            // Assert
            Assert.True(result);
            Assert.Null(_repository.GetById(id));
        }

        [Fact]
        public void Delete_NonExistingPlace_ReturnsFalse()
        {
            // Act
            var result = _repository.Delete("nonexistent123");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region SoftDelete Tests

        [Fact]
        public void SoftDelete_ExistingPlace_MarksAsInactive()
        {
            // Arrange
            var place = new Place { Name = "ToSoftDelete", Chain = "ChainA" };
            var id = _repository.Add(place);

            // Act
            var result = _repository.SoftDelete(id);

            // Assert
            Assert.True(result);
            // Note: GetById still returns soft-deleted items in current implementation
            // but GetAll/GetTotalCount filter them out
            Assert.Empty(_repository.GetAll());
        }

        #endregion

        #region SearchByName Tests

        [Fact]
        public void SearchByName_ExistingPlace_ReturnsMatchingPlaces()
        {
            // Arrange
            _repository.Add(new Place { Name = "Coles Brisbane", Chain = "Coles" });
            _repository.Add(new Place { Name = "Coles Sydney", Chain = "Coles" });
            _repository.Add(new Place { Name = "Woolworths Brisbane", Chain = "Woolworths" });

            // Act
            var result = _repository.SearchByName("Coles").ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, place => Assert.Contains("Coles", place.Name));
        }

        [Fact]
        public void SearchByName_CaseInsensitive_ReturnsMatches()
        {
            // Arrange
            _repository.Add(new Place { Name = "COLES BRISBANE", Chain = "Coles" });

            // Act
            var result = _repository.SearchByName("coles").ToList();

            // Assert
            Assert.Single(result);
        }

        #endregion

        #region GetByChain Tests

        [Fact]
        public void GetByChain_ExistingChain_ReturnsPlaces()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Woolworths" });

            // Act
            var result = _repository.GetByChain("Coles").ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetByChain_NoPlacesInChain_ReturnsEmpty()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles" });

            // Act
            var result = _repository.GetByChain("IGA");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBySuburb Tests

        [Fact]
        public void GetBySuburb_ExistingSuburb_ReturnsPlaces()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles", Suburb = "Brisbane" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Woolworths", Suburb = "Brisbane" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Coles", Suburb = "Sydney" });

            // Act
            var result = _repository.GetBySuburb("Brisbane").ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetByState Tests

        [Fact]
        public void GetByState_ExistingState_ReturnsPlaces()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles", State = "QLD" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Woolworths", State = "QLD" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Coles", State = "NSW" });

            // Act
            var result = _repository.GetByState("QLD").ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetByLocation Tests

        [Fact]
        public void GetByLocation_NearbyPlaces_ReturnsPlacesWithinRadius()
        {
            // Arrange
            // Brisbane CBD coordinates
            _repository.Add(new Place 
            { 
                Name = "Close Store", 
                Chain = "Coles",
                Latitude = -27.4698, 
                Longitude = 153.0251 
            });
            // Far away (Sydney)
            _repository.Add(new Place 
            { 
                Name = "Far Store", 
                Chain = "Coles",
                Latitude = -33.8688, 
                Longitude = 151.2093 
            });

            // Act - Search within 10km of Brisbane CBD
            var result = _repository.GetByLocation(-27.4698, 153.0251, 10).ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Close Store", result[0].Name);
        }

        [Fact]
        public void GetByLocation_NoCoordinates_ReturnsEmpty()
        {
            // Arrange
            _repository.Add(new Place 
            { 
                Name = "NoCoords Store", 
                Chain = "Coles"
                // No latitude/longitude
            });

            // Act
            var result = _repository.GetByLocation(-27.4698, 153.0251, 10);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetAllChains Tests

        [Fact]
        public void GetAllChains_ReturnsDistinctChains()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Woolworths" });
            _repository.Add(new Place { Name = "Store 4", Chain = "IGA" });

            // Act
            var result = _repository.GetAllChains().ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains("Coles", result);
            Assert.Contains("Woolworths", result);
            Assert.Contains("IGA", result);
        }

        [Fact]
        public void GetAllChains_SortedAlphabetically()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store1", Chain = "Zebra" });
            _repository.Add(new Place { Name = "Store2", Chain = "Apple" });

            // Act
            var result = _repository.GetAllChains().ToList();

            // Assert
            Assert.Equal("Apple", result[0]);
            Assert.Equal("Zebra", result[1]);
        }

        #endregion

        #region GetAllSuburbs Tests

        [Fact]
        public void GetAllSuburbs_ReturnsDistinctSuburbs()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles", Suburb = "Brisbane" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Woolworths", Suburb = "Brisbane" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Coles", Suburb = "Sydney" });

            // Act
            var result = _repository.GetAllSuburbs().ToList();

            // Assert
            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetAllStates Tests

        [Fact]
        public void GetAllStates_ReturnsDistinctStates()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles", State = "QLD" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Woolworths", State = "NSW" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Coles", State = "VIC" });

            // Act
            var result = _repository.GetAllStates().ToList();

            // Assert
            Assert.Equal(3, result.Count);
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
        public void GetTotalCount_WithPlaces_ReturnsCount()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store1", Chain = "ChainA" });
            _repository.Add(new Place { Name = "Store2", Chain = "ChainB" });

            // Act
            var result = _repository.GetTotalCount();

            // Assert
            Assert.Equal(2, result);
        }

        #endregion

        #region GetChainCounts Tests

        [Fact]
        public void GetChainCounts_ReturnsCorrectCounts()
        {
            // Arrange
            _repository.Add(new Place { Name = "Store 1", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 2", Chain = "Coles" });
            _repository.Add(new Place { Name = "Store 3", Chain = "Woolworths" });
            _repository.Add(new Place { Name = "Store 4", Chain = "IGA" });

            // Act
            var result = _repository.GetChainCounts();

            // Assert
            Assert.Equal(2, result["Coles"]);
            Assert.Equal(1, result["Woolworths"]);
            Assert.Equal(1, result["IGA"]);
        }

        #endregion
    }
}
