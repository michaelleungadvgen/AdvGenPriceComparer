using AdvGenPriceComparer.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for managing favorite items
/// </summary>
public interface IFavoritesService
{
    /// <summary>
    /// Gets all favorite items
    /// </summary>
    Task<List<Item>> GetFavoritesAsync();

    /// <summary>
    /// Adds an item to favorites
    /// </summary>
    Task<bool> AddToFavoritesAsync(string itemId);

    /// <summary>
    /// Removes an item from favorites
    /// </summary>
    Task<bool> RemoveFromFavoritesAsync(string itemId);

    /// <summary>
    /// Toggles favorite status for an item
    /// </summary>
    Task<bool> ToggleFavoriteAsync(string itemId);

    /// <summary>
    /// Checks if an item is in favorites
    /// </summary>
    Task<bool> IsFavoriteAsync(string itemId);

    /// <summary>
    /// Gets the count of favorite items
    /// </summary>
    Task<int> GetFavoritesCountAsync();

    /// <summary>
    /// Event raised when favorites change
    /// </summary>
    event EventHandler<FavoritesChangedEventArgs>? FavoritesChanged;
}

/// <summary>
/// Event arguments for favorites changed event
/// </summary>
public class FavoritesChangedEventArgs : EventArgs
{
    public string ItemId { get; }
    public bool IsAdded { get; }
    public int TotalFavorites { get; }

    public FavoritesChangedEventArgs(string itemId, bool isAdded, int totalFavorites)
    {
        ItemId = itemId;
        IsAdded = isAdded;
        TotalFavorites = totalFavorites;
    }
}
