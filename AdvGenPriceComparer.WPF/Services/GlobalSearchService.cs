using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.WPF.Models;

namespace AdvGenPriceComparer.WPF.Services;

public class GlobalSearchService : IGlobalSearchService
{
    private readonly IGroceryDataService _dataService;
    private readonly ILoggerService _logger;
    private readonly string _searchHistoryPath;
    private List<string> _recentSearches = new();

    public GlobalSearchService(IGroceryDataService dataService, ILoggerService logger)
    {
        _dataService = dataService;
        _logger = logger;
        
        var appDataPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer");
        _searchHistoryPath = Path.Combine(appDataPath, "search_history.json");
        
        // Load search history
        _ = LoadSearchHistoryAsync();
    }

    public async Task<IEnumerable<SearchResult>> SearchAsync(string query, SearchOptions? options = null)
    {
        options ??= new SearchOptions();
        
        if (string.IsNullOrWhiteSpace(query))
        {
            return Enumerable.Empty<SearchResult>();
        }

        _logger.LogInfo($"Global search: '{query}'");
        
        var results = new List<SearchResult>();
        var searchTerm = query.Trim().ToLowerInvariant();
        
        // Search in parallel
        var tasks = new List<Task<IEnumerable<SearchResult>>>();
        
        if (options.IncludeItems)
        {
            tasks.Add(SearchItemsAsync(searchTerm, options.MaxResultsPerType));
        }
        
        if (options.IncludePlaces)
        {
            tasks.Add(SearchPlacesAsync(searchTerm, options.MaxResultsPerType));
        }
        
        if (options.IncludePriceRecords)
        {
            tasks.Add(SearchPriceRecordsAsync(searchTerm, options.MaxResultsPerType));
        }
        
        var searchResults = await Task.WhenAll(tasks);
        
        foreach (var resultSet in searchResults)
        {
            results.AddRange(resultSet);
        }
        
        // Sort by relevance score (descending)
        var sortedResults = results
            .Where(r => r.RelevanceScore >= options.MinimumRelevance)
            .OrderByDescending(r => r.RelevanceScore)
            .ThenByDescending(r => r.LastUpdated)
            .Take(options.MaxResults)
            .ToList();
        
        _logger.LogInfo($"Found {sortedResults.Count} results for '{query}'");
        
        // Save to history
        await SaveSearchToHistoryAsync(query.Trim());
        
        return sortedResults;
    }

    public Task<IEnumerable<SearchResult>> SearchItemsAsync(string query, int limit = 20)
    {
        var results = new List<SearchResult>();
        var searchTerm = query.ToLowerInvariant();
        
        try
        {
            var items = _dataService.GetAllItems().ToList();
            
            foreach (var item in items)
            {
                double relevanceScore = 0;
                string? matchedField = null;
                string? matchedValue = null;
                
                // Check name (highest weight)
                if (!string.IsNullOrEmpty(item.Name))
                {
                    var nameLower = item.Name.ToLowerInvariant();
                    if (nameLower == searchTerm)
                    {
                        relevanceScore = 1.0;
                        matchedField = "Name";
                        matchedValue = item.Name;
                    }
                    else if (nameLower.StartsWith(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.9);
                        matchedField ??= "Name";
                        matchedValue ??= item.Name;
                    }
                    else if (nameLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.7);
                        matchedField ??= "Name";
                        matchedValue ??= item.Name;
                    }
                }
                
                // Check brand
                if (!string.IsNullOrEmpty(item.Brand))
                {
                    var brandLower = item.Brand.ToLowerInvariant();
                    if (brandLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.6);
                        matchedField ??= "Brand";
                        matchedValue ??= item.Brand;
                    }
                }
                
                // Check barcode (exact or partial match, high relevance)
                if (!string.IsNullOrEmpty(item.Barcode) && 
                    item.Barcode.Replace("-", "").Replace(" ", "").Contains(query.Replace("-", "").Replace(" ", "")))
                {
                    relevanceScore = Math.Max(relevanceScore, 0.95);
                    matchedField ??= "Barcode";
                    matchedValue ??= item.Barcode;
                }
                
                // Check category
                if (!string.IsNullOrEmpty(item.Category))
                {
                    var catLower = item.Category.ToLowerInvariant();
                    if (catLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.5);
                        matchedField ??= "Category";
                        matchedValue ??= item.Category;
                    }
                }
                
                // Check subcategory
                if (!string.IsNullOrEmpty(item.SubCategory))
                {
                    var subCatLower = item.SubCategory.ToLowerInvariant();
                    if (subCatLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.45);
                        matchedField ??= "SubCategory";
                        matchedValue ??= item.SubCategory;
                    }
                }
                
                // Check description
                if (!string.IsNullOrEmpty(item.Description))
                {
                    var descLower = item.Description.ToLowerInvariant();
                    if (descLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.4);
                        matchedField ??= "Description";
                        matchedValue ??= item.Description;
                    }
                }
                
                // Check tags
                if (item.Tags.Any(tag => tag.ToLowerInvariant().Contains(searchTerm)))
                {
                    relevanceScore = Math.Max(relevanceScore, 0.35);
                    matchedField ??= "Tags";
                }
                
                if (relevanceScore > 0)
                {
                    results.Add(SearchResult.FromItem(item, matchedField ?? "Name", matchedValue, relevanceScore));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error searching items", ex);
        }
        
        return Task.FromResult(results.OrderByDescending(r => r.RelevanceScore).Take(limit).AsEnumerable());
    }

    public Task<IEnumerable<SearchResult>> SearchPlacesAsync(string query, int limit = 20)
    {
        var results = new List<SearchResult>();
        var searchTerm = query.ToLowerInvariant();
        
        try
        {
            var places = _dataService.GetAllPlaces().ToList();
            
            foreach (var place in places)
            {
                double relevanceScore = 0;
                string? matchedField = null;
                string? matchedValue = null;
                
                // Check name
                if (!string.IsNullOrEmpty(place.Name))
                {
                    var nameLower = place.Name.ToLowerInvariant();
                    if (nameLower == searchTerm)
                    {
                        relevanceScore = 1.0;
                        matchedField = "Name";
                        matchedValue = place.Name;
                    }
                    else if (nameLower.StartsWith(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.9);
                        matchedField ??= "Name";
                        matchedValue ??= place.Name;
                    }
                    else if (nameLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.7);
                        matchedField ??= "Name";
                        matchedValue ??= place.Name;
                    }
                }
                
                // Check chain
                if (!string.IsNullOrEmpty(place.Chain))
                {
                    var chainLower = place.Chain.ToLowerInvariant();
                    if (chainLower == searchTerm)
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.85);
                        matchedField ??= "Chain";
                        matchedValue ??= place.Chain;
                    }
                    else if (chainLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.6);
                        matchedField ??= "Chain";
                        matchedValue ??= place.Chain;
                    }
                }
                
                // Check suburb
                if (!string.IsNullOrEmpty(place.Suburb))
                {
                    var suburbLower = place.Suburb.ToLowerInvariant();
                    if (suburbLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.5);
                        matchedField ??= "Suburb";
                        matchedValue ??= place.Suburb;
                    }
                }
                
                // Check state
                if (!string.IsNullOrEmpty(place.State))
                {
                    var stateLower = place.State.ToLowerInvariant();
                    if (stateLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.4);
                        matchedField ??= "State";
                        matchedValue ??= place.State;
                    }
                }
                
                // Check address
                if (!string.IsNullOrEmpty(place.Address))
                {
                    var addrLower = place.Address.ToLowerInvariant();
                    if (addrLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.35);
                        matchedField ??= "Address";
                        matchedValue ??= place.Address;
                    }
                }
                
                // Check postcode
                if (!string.IsNullOrEmpty(place.Postcode) && place.Postcode.Contains(query))
                {
                    relevanceScore = Math.Max(relevanceScore, 0.45);
                    matchedField ??= "Postcode";
                    matchedValue ??= place.Postcode;
                }
                
                if (relevanceScore > 0)
                {
                    results.Add(SearchResult.FromPlace(place, matchedField ?? "Name", matchedValue, relevanceScore));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error searching places", ex);
        }
        
        return Task.FromResult(results.OrderByDescending(r => r.RelevanceScore).Take(limit).AsEnumerable());
    }

    public Task<IEnumerable<SearchResult>> SearchPriceRecordsAsync(string query, int limit = 20)
    {
        var results = new List<SearchResult>();
        var searchTerm = query.ToLowerInvariant();
        
        try
        {
            var records = _dataService.GetPriceHistory().ToList();
            
            // Get items for name lookup
            var items = _dataService.GetAllItems().ToDictionary(i => i.Id, i => i);
            
            foreach (var record in records)
            {
                double relevanceScore = 0;
                string? matchedField = null;
                string? matchedValue = null;
                string? itemName = null;
                string? category = null;
                
                // Search by item name
                if (items.TryGetValue(record.ItemId, out var item))
                {
                    itemName = item.DisplayName;
                    category = item.Category;
                    var itemNameLower = item.Name.ToLowerInvariant();
                    if (itemNameLower.Contains(searchTerm))
                    {
                        relevanceScore = 0.6;
                        matchedField = "Item Name";
                        matchedValue = item.Name;
                    }
                    
                    // Check barcode
                    if (!string.IsNullOrEmpty(item.Barcode) && 
                        item.Barcode.Contains(query))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.85);
                        matchedField ??= "Item Barcode";
                        matchedValue ??= item.Barcode;
                    }
                }
                
                // Search by price (if query is a number)
                if (decimal.TryParse(query, out var searchPrice))
                {
                    if (Math.Abs(record.Price - searchPrice) < 0.01m)
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.9);
                        matchedField ??= "Price";
                        matchedValue ??= $"${record.Price:F2}";
                    }
                }
                
                // Search by sale description
                if (!string.IsNullOrEmpty(record.SaleDescription))
                {
                    var descLower = record.SaleDescription.ToLowerInvariant();
                    if (descLower.Contains(searchTerm))
                    {
                        relevanceScore = Math.Max(relevanceScore, 0.4);
                        matchedField ??= "Sale Description";
                        matchedValue ??= record.SaleDescription;
                    }
                }
                
                if (relevanceScore > 0)
                {
                    results.Add(SearchResult.FromPriceRecord(record, matchedField ?? "Item", matchedValue, relevanceScore, itemName, category));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Error searching price records", ex);
        }
        
        return Task.FromResult(results.OrderByDescending(r => r.RelevanceScore).Take(limit).AsEnumerable());
    }

    public Task<IEnumerable<string>> GetRecentSearchesAsync(int count = 10)
    {
        var searches = _recentSearches
            .Take(count)
            .AsEnumerable();
        
        return Task.FromResult(searches);
    }

    public async Task SaveSearchToHistoryAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;
        
        // Remove if already exists (to move to top)
        _recentSearches.Remove(query);
        
        // Add to top
        _recentSearches.Insert(0, query);
        
        // Keep only last 50 searches
        if (_recentSearches.Count > 50)
        {
            _recentSearches = _recentSearches.Take(50).ToList();
        }
        
        await SaveSearchHistoryAsync();
    }

    public async Task ClearSearchHistoryAsync()
    {
        _recentSearches.Clear();
        await SaveSearchHistoryAsync();
    }

    private async Task LoadSearchHistoryAsync()
    {
        try
        {
            if (File.Exists(_searchHistoryPath))
            {
                var json = await File.ReadAllTextAsync(_searchHistoryPath);
                var history = JsonSerializer.Deserialize<List<string>>(json);
                if (history != null)
                {
                    _recentSearches = history;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to load search history", ex);
        }
    }

    private async Task SaveSearchHistoryAsync()
    {
        try
        {
            var directory = Path.GetDirectoryName(_searchHistoryPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            var json = JsonSerializer.Serialize(_recentSearches);
            await File.WriteAllTextAsync(_searchHistoryPath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to save search history", ex);
        }
    }
}
