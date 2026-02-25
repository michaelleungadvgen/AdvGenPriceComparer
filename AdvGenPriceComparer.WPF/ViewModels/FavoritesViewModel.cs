using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AdvGenPriceComparer.WPF.ViewModels;

/// <summary>
/// ViewModel for managing favorite items
/// </summary>
public class FavoritesViewModel : ViewModelBase
{
    private readonly IFavoritesService _favoritesService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;

    private ObservableCollection<Item> _favoriteItems = new();
    private Item? _selectedItem;
    private string _searchText = string.Empty;
    private bool _isLoading;
    private int _totalFavorites;

    /// <summary>
    /// Collection of favorite items
    /// </summary>
    public ObservableCollection<Item> FavoriteItems
    {
        get => _favoriteItems;
        set
        {
            _favoriteItems = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Currently selected item
    /// </summary>
    public Item? SelectedItem
    {
        get => _selectedItem;
        set
        {
            _selectedItem = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanRemoveFromFavorites));
        }
    }

    /// <summary>
    /// Search text for filtering favorites
    /// </summary>
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            FilterFavorites();
        }
    }

    /// <summary>
    /// Whether data is being loaded
    /// </summary>
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            _isLoading = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Total count of favorite items
    /// </summary>
    public int TotalFavorites
    {
        get => _totalFavorites;
        set
        {
            _totalFavorites = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasFavorites));
            OnPropertyChanged(nameof(EmptyMessage));
        }
    }

    /// <summary>
    /// Whether user has any favorite items
    /// </summary>
    public bool HasFavorites => TotalFavorites > 0;

    /// <summary>
    /// Message shown when no favorites exist
    /// </summary>
    public string EmptyMessage => HasFavorites 
        ? "No favorites match your search." 
        : "You don't have any favorite items yet.\\n\\nAdd items to your favorites to quickly access them here.";

    /// <summary>
    /// Whether the selected item can be removed from favorites
    /// </summary>
    public bool CanRemoveFromFavorites => SelectedItem != null;

    // Commands
    public ICommand LoadFavoritesCommand { get; }
    public ICommand RemoveFromFavoritesCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand RefreshCommand { get; }

    public FavoritesViewModel(
        IFavoritesService favoritesService,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _favoritesService = favoritesService;
        _dialogService = dialogService;
        _logger = logger;

        LoadFavoritesCommand = new RelayCommand(async () => await LoadFavoritesAsync());
        RemoveFromFavoritesCommand = new RelayCommand(async () => await RemoveFromFavoritesAsync(), () => CanRemoveFromFavorites);
        ToggleFavoriteCommand = new RelayCommand<Item>(async (item) => await ToggleFavoriteAsync(item));
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
        RefreshCommand = new RelayCommand(async () => await LoadFavoritesAsync());

        // Subscribe to favorites changed events
        _favoritesService.FavoritesChanged += OnFavoritesChanged;

        // Load favorites on initialization
        _ = LoadFavoritesAsync();
    }

    /// <summary>
    /// Loads all favorite items
    /// </summary>
    public async Task LoadFavoritesAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInfo("Loading favorite items...");

            var favorites = await _favoritesService.GetFavoritesAsync();
            
            FavoriteItems.Clear();
            foreach (var item in favorites.OrderBy(i => i.DisplayName))
            {
                FavoriteItems.Add(item);
            }

            TotalFavorites = favorites.Count;
            _logger.LogInfo($"Loaded {favorites.Count} favorite items");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load favorites", ex);
            _dialogService.ShowError("Failed to load favorite items. Please try again.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Removes the selected item from favorites
    /// </summary>
    private async Task RemoveFromFavoritesAsync()
    {
        if (SelectedItem == null) return;

        try
        {
            var result = await _favoritesService.RemoveFromFavoritesAsync(SelectedItem.Id);
            if (result)
            {
                FavoriteItems.Remove(SelectedItem);
                TotalFavorites--;
                SelectedItem = null;
                _logger.LogInfo("Item removed from favorites");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to remove from favorites", ex);
            _dialogService.ShowError("Failed to remove item from favorites.");
        }
    }

    /// <summary>
    /// Toggles favorite status for an item
    /// </summary>
    private async Task ToggleFavoriteAsync(Item? item)
    {
        if (item == null) return;

        try
        {
            var result = await _favoritesService.ToggleFavoriteAsync(item.Id);
            if (result)
            {
                // Reload to reflect changes
                await LoadFavoritesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to toggle favorite", ex);
            _dialogService.ShowError("Failed to update favorite status.");
        }
    }

    /// <summary>
    /// Filters favorites based on search text
    /// </summary>
    private void FilterFavorites()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            // Reload all favorites
            _ = LoadFavoritesAsync();
            return;
        }

        var filtered = FavoriteItems.Where(i => 
            i.MatchesSearch(SearchText)).ToList();

        FavoriteItems.Clear();
        foreach (var item in filtered)
        {
            FavoriteItems.Add(item);
        }
    }

    /// <summary>
    /// Handles favorites changed event
    /// </summary>
    private void OnFavoritesChanged(object? sender, FavoritesChangedEventArgs e)
    {
        _logger.LogInfo($"Favorites changed: Item {e.ItemId} {(e.IsAdded ? "added" : "removed")}");
        
        // Refresh the list on the UI thread
        System.Windows.Application.Current?.Dispatcher.Invoke(async () =>
        {
            await LoadFavoritesAsync();
        });
    }

    public void Dispose()
    {
        _favoritesService.FavoritesChanged -= OnFavoritesChanged;
    }
}
