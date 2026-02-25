using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Services;
using Moq;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Unit tests for FavoritesService
/// </summary>
public class FavoritesServiceTests
{
    private readonly Mock<IItemRepository> _mockItemRepository;
    private readonly Mock<ILoggerService> _mockLogger;
    private readonly FavoritesService _favoritesService;

    public FavoritesServiceTests()
    {
        _mockItemRepository = new Mock<IItemRepository>();
        _mockLogger = new Mock<ILoggerService>();
        _favoritesService = new FavoritesService(_mockItemRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task GetFavoritesAsync_WithFavorites_ReturnsOnlyFavorites()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = "1", Name = "Product 1", IsFavorite = true },
            new Item { Id = "2", Name = "Product 2", IsFavorite = false },
            new Item { Id = "3", Name = "Product 3", IsFavorite = true }
        };
        _mockItemRepository.Setup(r => r.GetAll()).Returns(items);

        // Act
        var result = await _favoritesService.GetFavoritesAsync();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.True(item.IsFavorite));
        Assert.Contains(result, i => i.Id == "1");
        Assert.Contains(result, i => i.Id == "3");
        Assert.DoesNotContain(result, i => i.Id == "2");
    }

    [Fact]
    public async Task GetFavoritesAsync_WithNoFavorites_ReturnsEmptyList()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = "1", Name = "Product 1", IsFavorite = false },
            new Item { Id = "2", Name = "Product 2", IsFavorite = false }
        };
        _mockItemRepository.Setup(r => r.GetAll()).Returns(items);

        // Act
        var result = await _favoritesService.GetFavoritesAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ItemNotFavorite_AddsSuccessfully()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = false };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item> { item });

        // Act
        var result = await _favoritesService.AddToFavoritesAsync("1");

        // Assert
        Assert.True(result);
        Assert.True(item.IsFavorite);
        _mockItemRepository.Verify(r => r.Update(item), Times.Once);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ItemAlreadyFavorite_ReturnsTrueWithoutUpdate()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = true };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);

        // Act
        var result = await _favoritesService.AddToFavoritesAsync("1");

        // Assert
        Assert.True(result);
        _mockItemRepository.Verify(r => r.Update(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task AddToFavoritesAsync_ItemNotFound_ReturnsFalse()
    {
        // Arrange
        _mockItemRepository.Setup(r => r.GetById("1")).Returns((Item?)null);

        // Act
        var result = await _favoritesService.AddToFavoritesAsync("1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ItemIsFavorite_RemovesSuccessfully()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = true };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item>());

        // Act
        var result = await _favoritesService.RemoveFromFavoritesAsync("1");

        // Assert
        Assert.True(result);
        Assert.False(item.IsFavorite);
        _mockItemRepository.Verify(r => r.Update(item), Times.Once);
    }

    [Fact]
    public async Task RemoveFromFavoritesAsync_ItemNotFavorite_ReturnsTrueWithoutUpdate()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = false };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);

        // Act
        var result = await _favoritesService.RemoveFromFavoritesAsync("1");

        // Assert
        Assert.True(result);
        _mockItemRepository.Verify(r => r.Update(It.IsAny<Item>()), Times.Never);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ItemNotFavorite_AddsToFavorites()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = false };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item> { item });

        // Act
        var result = await _favoritesService.ToggleFavoriteAsync("1");

        // Assert
        Assert.True(result);
        Assert.True(item.IsFavorite);
    }

    [Fact]
    public async Task ToggleFavoriteAsync_ItemIsFavorite_RemovesFromFavorites()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = true };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item>());

        // Act
        var result = await _favoritesService.ToggleFavoriteAsync("1");

        // Assert
        Assert.True(result);
        Assert.False(item.IsFavorite);
    }

    [Fact]
    public async Task IsFavoriteAsync_ItemIsFavorite_ReturnsTrue()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = true };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);

        // Act
        var result = await _favoritesService.IsFavoriteAsync("1");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsFavoriteAsync_ItemNotFavorite_ReturnsFalse()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = false };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);

        // Act
        var result = await _favoritesService.IsFavoriteAsync("1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsFavoriteAsync_ItemNotFound_ReturnsFalse()
    {
        // Arrange
        _mockItemRepository.Setup(r => r.GetById("1")).Returns((Item?)null);

        // Act
        var result = await _favoritesService.IsFavoriteAsync("1");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetFavoritesCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        var items = new List<Item>
        {
            new Item { Id = "1", Name = "Product 1", IsFavorite = true },
            new Item { Id = "2", Name = "Product 2", IsFavorite = false },
            new Item { Id = "3", Name = "Product 3", IsFavorite = true },
            new Item { Id = "4", Name = "Product 4", IsFavorite = true }
        };
        _mockItemRepository.Setup(r => r.GetAll()).Returns(items);

        // Act
        var result = await _favoritesService.GetFavoritesCountAsync();

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public void FavoritesChanged_EventRaised_WhenAddingFavorite()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = false };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item> { item });

        FavoritesChangedEventArgs? eventArgs = null;
        _favoritesService.FavoritesChanged += (s, e) => eventArgs = e;

        // Act
        _favoritesService.AddToFavoritesAsync("1").Wait();

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("1", eventArgs.ItemId);
        Assert.True(eventArgs.IsAdded);
    }

    [Fact]
    public void FavoritesChanged_EventRaised_WhenRemovingFavorite()
    {
        // Arrange
        var item = new Item { Id = "1", Name = "Product 1", IsFavorite = true };
        _mockItemRepository.Setup(r => r.GetById("1")).Returns(item);
        _mockItemRepository.Setup(r => r.GetAll()).Returns(new List<Item>());

        FavoritesChangedEventArgs? eventArgs = null;
        _favoritesService.FavoritesChanged += (s, e) => eventArgs = e;

        // Act
        _favoritesService.RemoveFromFavoritesAsync("1").Wait();

        // Assert
        Assert.NotNull(eventArgs);
        Assert.Equal("1", eventArgs.ItemId);
        Assert.False(eventArgs.IsAdded);
    }
}
