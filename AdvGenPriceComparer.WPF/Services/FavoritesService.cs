using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Implementation of favorites service
/// </summary>
public class FavoritesService : IFavoritesService
{
    private readonly IItemRepository _itemRepository;
    private readonly ILoggerService _logger;

    public event EventHandler<FavoritesChangedEventArgs>? FavoritesChanged;

    public FavoritesService(IItemRepository itemRepository, ILoggerService logger)
    {
        _itemRepository = itemRepository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<List<Item>> GetFavoritesAsync()
    {
        try
        {
            var allItems = _itemRepository.GetAll();
            var favorites = allItems.Where(i => i.IsFavorite).ToList();
            _logger.LogInfo($"Retrieved {favorites.Count} favorite items");
            return favorites;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get favorite items", ex);
            return new List<Item>();
        }
    }

    /// <inheritdoc />
    public async Task<bool> AddToFavoritesAsync(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                _logger.LogWarning($"Cannot add to favorites: Item {itemId} not found");
                return false;
            }

            if (item.IsFavorite)
            {
                _logger.LogInfo($"Item {itemId} is already in favorites");
                return true;
            }

            item.IsFavorite = true;
            item.MarkAsUpdated();
            _itemRepository.Update(item);

            var count = await GetFavoritesCountAsync();
            OnFavoritesChanged(itemId, true, count);

            _logger.LogInfo($"Added item {itemId} to favorites");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to add item {itemId} to favorites", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> RemoveFromFavoritesAsync(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                _logger.LogWarning($"Cannot remove from favorites: Item {itemId} not found");
                return false;
            }

            if (!item.IsFavorite)
            {
                _logger.LogInfo($"Item {itemId} is not in favorites");
                return true;
            }

            item.IsFavorite = false;
            item.MarkAsUpdated();
            _itemRepository.Update(item);

            var count = await GetFavoritesCountAsync();
            OnFavoritesChanged(itemId, false, count);

            _logger.LogInfo($"Removed item {itemId} from favorites");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to remove item {itemId} from favorites", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public async Task<bool> ToggleFavoriteAsync(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            if (item == null)
            {
                _logger.LogWarning($"Cannot toggle favorite: Item {itemId} not found");
                return false;
            }

            if (item.IsFavorite)
            {
                return await RemoveFromFavoritesAsync(itemId);
            }
            else
            {
                return await AddToFavoritesAsync(itemId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to toggle favorite for item {itemId}", ex);
            return false;
        }
    }

    /// <inheritdoc />
    public Task<bool> IsFavoriteAsync(string itemId)
    {
        try
        {
            var item = _itemRepository.GetById(itemId);
            return Task.FromResult(item?.IsFavorite ?? false);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to check favorite status for item {itemId}", ex);
            return Task.FromResult(false);
        }
    }

    /// <inheritdoc />
    public Task<int> GetFavoritesCountAsync()
    {
        try
        {
            var count = _itemRepository.GetAll().Count(i => i.IsFavorite);
            return Task.FromResult(count);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to get favorites count", ex);
            return Task.FromResult(0);
        }
    }

    private void OnFavoritesChanged(string itemId, bool isAdded, int totalFavorites)
    {
        FavoritesChanged?.Invoke(this, new FavoritesChangedEventArgs(itemId, isAdded, totalFavorites));
    }
}
