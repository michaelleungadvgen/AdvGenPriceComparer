using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AdvGenPriceComparer.WPF.Commands;
using AdvGenPriceComparer.WPF.Models;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.ViewModels;

public class GlobalSearchViewModel : ViewModelBase
{
    private readonly IGlobalSearchService _searchService;
    private readonly IDialogService _dialogService;
    private readonly ILoggerService _logger;
    
    private string _searchQuery = string.Empty;
    private ObservableCollection<SearchResult> _searchResults = new();
    private ObservableCollection<string> _recentSearches = new();
    private SearchResult? _selectedResult;
    private bool _isSearching;
    private bool _hasResults;
    private bool _hasNoResults;
    private string _resultsSummary = string.Empty;
    private SearchOptions _searchOptions = new();

    public GlobalSearchViewModel(
        IGlobalSearchService searchService,
        IDialogService dialogService,
        ILoggerService logger)
    {
        _searchService = searchService;
        _dialogService = dialogService;
        _logger = logger;
        
        SearchCommand = new RelayCommand(async () => await PerformSearchAsync(), () => !IsSearching && !string.IsNullOrWhiteSpace(SearchQuery));
        ClearSearchCommand = new RelayCommand(ClearSearch);
        UseRecentSearchCommand = new RelayCommand<string>(UseRecentSearch);
        ClearHistoryCommand = new RelayCommand(async () => await ClearHistoryAsync());
        SelectResultCommand = new RelayCommand<SearchResult>(SelectResult);
        
        // Load recent searches
        _ = LoadRecentSearchesAsync();
    }

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            if (SetProperty(ref _searchQuery, value))
            {
                // Enable/disable search command
                ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
                
                // Auto-search after a delay if 3+ characters
                if (value?.Length >= 3)
                {
                    _ = DebouncedSearchAsync();
                }
                else if (string.IsNullOrEmpty(value))
                {
                    ClearSearch();
                }
            }
        }
    }

    public ObservableCollection<SearchResult> SearchResults
    {
        get => _searchResults;
        set => SetProperty(ref _searchResults, value);
    }

    public ObservableCollection<string> RecentSearches
    {
        get => _recentSearches;
        set => SetProperty(ref _recentSearches, value);
    }

    public SearchResult? SelectedResult
    {
        get => _selectedResult;
        set
        {
            if (SetProperty(ref _selectedResult, value) && value != null)
            {
                // Handle selection
                SelectResult(value);
            }
        }
    }

    public bool IsSearching
    {
        get => _isSearching;
        set
        {
            if (SetProperty(ref _isSearching, value))
            {
                ((RelayCommand)SearchCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool HasResults
    {
        get => _hasResults;
        set => SetProperty(ref _hasResults, value);
    }

    public bool HasNoResults
    {
        get => _hasNoResults;
        set => SetProperty(ref _hasNoResults, value);
    }

    public string ResultsSummary
    {
        get => _resultsSummary;
        set => SetProperty(ref _resultsSummary, value);
    }

    public SearchOptions SearchOptions
    {
        get => _searchOptions;
        set => SetProperty(ref _searchOptions, value);
    }

    // Grouped results for display
    public List<SearchResultGroup> GroupedResults
    {
        get
        {
            var groups = SearchResults
                .GroupBy(r => r.ResultType)
                .Select(g => new SearchResultGroup
                {
                    GroupType = g.Key,
                    GroupTitle = GetGroupTitle(g.Key),
                    Results = new ObservableCollection<SearchResult>(g.OrderByDescending(r => r.RelevanceScore))
                })
                .OrderBy(g => g.GroupType)
                .ToList();
            
            return groups;
        }
    }

    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand UseRecentSearchCommand { get; }
    public ICommand ClearHistoryCommand { get; }
    public ICommand SelectResultCommand { get; }

    private async Task DebouncedSearchAsync()
    {
        // Wait a bit to allow user to finish typing
        await Task.Delay(300);
        
        // Only search if the query hasn't changed
        if (SearchQuery.Length >= 3)
        {
            await PerformSearchAsync();
        }
    }

    private async Task PerformSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
            return;

        IsSearching = true;
        SearchResults.Clear();
        HasResults = false;
        HasNoResults = false;
        ResultsSummary = string.Empty;

        try
        {
            var results = await _searchService.SearchAsync(SearchQuery, SearchOptions);
            var resultList = results.ToList();
            
            SearchResults = new ObservableCollection<SearchResult>(resultList);
            HasResults = resultList.Any();
            HasNoResults = !resultList.Any();
            
            if (resultList.Any())
            {
                var itemsCount = resultList.Count(r => r.ResultType == SearchResultType.Item);
                var placesCount = resultList.Count(r => r.ResultType == SearchResultType.Place);
                var pricesCount = resultList.Count(r => r.ResultType == SearchResultType.PriceRecord);
                
                var parts = new List<string>();
                if (itemsCount > 0) parts.Add($"{itemsCount} item(s)");
                if (placesCount > 0) parts.Add($"{placesCount} store(s)");
                if (pricesCount > 0) parts.Add($"{pricesCount} price(s)");
                
                ResultsSummary = $"Found {string.Join(", ", parts)}";
            }
            else
            {
                ResultsSummary = $"No results found for '{SearchQuery}'";
            }
            
            OnPropertyChanged(nameof(GroupedResults));
        }
        catch (Exception ex)
        {
            _logger.LogError("Search failed", ex);
            _dialogService.ShowError($"Search failed: {ex.Message}");
        }
        finally
        {
            IsSearching = false;
        }
    }

    private void ClearSearch()
    {
        SearchQuery = string.Empty;
        SearchResults.Clear();
        HasResults = false;
        HasNoResults = false;
        ResultsSummary = string.Empty;
        OnPropertyChanged(nameof(GroupedResults));
    }

    private void UseRecentSearch(string? query)
    {
        if (!string.IsNullOrEmpty(query))
        {
            SearchQuery = query;
            _ = PerformSearchAsync();
        }
    }

    private async Task LoadRecentSearchesAsync()
    {
        try
        {
            var searches = await _searchService.GetRecentSearchesAsync(10);
            RecentSearches = new ObservableCollection<string>(searches);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load recent searches", ex);
        }
    }

    private async Task ClearHistoryAsync()
    {
        try
        {
            await _searchService.ClearSearchHistoryAsync();
            RecentSearches.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to clear search history", ex);
        }
    }

    private void SelectResult(SearchResult? result)
    {
        if (result == null) return;
        
        _logger.LogInfo($"Selected search result: {result.Title} ({result.ResultType})");
        
        // The dialog will handle navigation based on result type
        // For now, just close with the selected result
        SelectedResult = result;
    }

    private static string GetGroupTitle(SearchResultType type)
    {
        return type switch
        {
            SearchResultType.Item => "🛒 Products",
            SearchResultType.Place => "🏪 Stores",
            SearchResultType.PriceRecord => "💰 Prices",
            _ => "Other"
        };
    }
}

/// <summary>
/// Represents a group of search results by type
/// </summary>
public class SearchResultGroup
{
    public SearchResultType GroupType { get; set; }
    public string GroupTitle { get; set; } = string.Empty;
    public ObservableCollection<SearchResult> Results { get; set; } = new();
}
