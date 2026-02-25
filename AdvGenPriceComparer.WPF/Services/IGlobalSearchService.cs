using System.Collections.Generic;
using System.Threading.Tasks;
using AdvGenPriceComparer.WPF.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for searching across all entities (Items, Places, PriceRecords)
/// </summary>
public interface IGlobalSearchService
{
    /// <summary>
    /// Performs a global search across all entities
    /// </summary>
    /// <param name="query">The search query</param>
    /// <param name="options">Search options (optional)</param>
    /// <returns>Collection of search results ordered by relevance</returns>
    Task<IEnumerable<SearchResult>> SearchAsync(string query, SearchOptions? options = null);
    
    /// <summary>
    /// Searches for items only
    /// </summary>
    Task<IEnumerable<SearchResult>> SearchItemsAsync(string query, int limit = 20);
    
    /// <summary>
    /// Searches for places/stores only
    /// </summary>
    Task<IEnumerable<SearchResult>> SearchPlacesAsync(string query, int limit = 20);
    
    /// <summary>
    /// Searches for price records only
    /// </summary>
    Task<IEnumerable<SearchResult>> SearchPriceRecordsAsync(string query, int limit = 20);
    
    /// <summary>
    /// Gets recent search history
    /// </summary>
    Task<IEnumerable<string>> GetRecentSearchesAsync(int count = 10);
    
    /// <summary>
    /// Saves a search query to history
    /// </summary>
    Task SaveSearchToHistoryAsync(string query);
    
    /// <summary>
    /// Clears search history
    /// </summary>
    Task ClearSearchHistoryAsync();
}

/// <summary>
/// Options for configuring search behavior
/// </summary>
public class SearchOptions
{
    /// <summary>
    /// Maximum total results to return
    /// </summary>
    public int MaxResults { get; set; } = 50;
    
    /// <summary>
    /// Maximum results per entity type
    /// </summary>
    public int MaxResultsPerType { get; set; } = 20;
    
    /// <summary>
    /// Whether to include items in search
    /// </summary>
    public bool IncludeItems { get; set; } = true;
    
    /// <summary>
    /// Whether to include places/stores in search
    /// </summary>
    public bool IncludePlaces { get; set; } = true;
    
    /// <summary>
    /// Whether to include price records in search
    /// </summary>
    public bool IncludePriceRecords { get; set; } = true;
    
    /// <summary>
    /// Whether to search within descriptions
    /// </summary>
    public bool SearchDescriptions { get; set; } = true;
    
    /// <summary>
    /// Whether to search within barcodes (exact match only)
    /// </summary>
    public bool SearchBarcodes { get; set; } = true;
    
    /// <summary>
    /// Minimum relevance score for results (0.0 to 1.0)
    /// </summary>
    public double MinimumRelevance { get; set; } = 0.0;
    
    /// <summary>
    /// Whether to use fuzzy matching
    /// </summary>
    public bool UseFuzzyMatching { get; set; } = true;
}
