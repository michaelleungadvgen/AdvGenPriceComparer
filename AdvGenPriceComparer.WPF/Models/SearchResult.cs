using System;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Models;

/// <summary>
/// Represents a single search result across all entity types
/// </summary>
public class SearchResult
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// The type of entity found (Item, Place, PriceRecord)
    /// </summary>
    public SearchResultType ResultType { get; set; }
    
    /// <summary>
    /// Display title for the result
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Secondary display text (subtitle)
    /// </summary>
    public string? Subtitle { get; set; }
    
    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Category or grouping information
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Icon or emoji to display
    /// </summary>
    public string Icon { get; set; } = "🔍";
    
    /// <summary>
    /// The actual entity object (Item, Place, or PriceRecord)
    /// </summary>
    public object? Entity { get; set; }
    
    /// <summary>
    /// The entity ID for navigation
    /// </summary>
    public string EntityId { get; set; } = string.Empty;
    
    /// <summary>
    /// The field that matched the search query
    /// </summary>
    public string? MatchedField { get; set; }
    
    /// <summary>
    /// The value that matched (for highlighting)
    /// </summary>
    public string? MatchedValue { get; set; }
    
    /// <summary>
    /// Relevance score for ranking results
    /// </summary>
    public double RelevanceScore { get; set; }
    
    /// <summary>
    /// When the entity was last updated
    /// </summary>
    public DateTime? LastUpdated { get; set; }

    // Factory methods for creating search results
    
    public static SearchResult FromItem(Item item, string matchedField, string? matchedValue, double relevanceScore)
    {
        return new SearchResult
        {
            ResultType = SearchResultType.Item,
            Title = item.DisplayName,
            Subtitle = item.Category ?? "Uncategorized",
            Description = item.Description,
            Category = item.Category,
            Icon = GetItemIcon(item.Category),
            Entity = item,
            EntityId = item.Id,
            MatchedField = matchedField,
            MatchedValue = matchedValue,
            RelevanceScore = relevanceScore,
            LastUpdated = item.LastUpdated
        };
    }
    
    public static SearchResult FromPlace(Place place, string matchedField, string? matchedValue, double relevanceScore)
    {
        var location = $"{place.Suburb}{(place.State != null ? ", " + place.State : "")}";
        
        return new SearchResult
        {
            ResultType = SearchResultType.Place,
            Title = place.Name,
            Subtitle = place.Chain ?? "Independent Store",
            Description = !string.IsNullOrEmpty(location) ? location : place.Address,
            Category = place.Chain ?? "Store",
            Icon = "🏪",
            Entity = place,
            EntityId = place.Id,
            MatchedField = matchedField,
            MatchedValue = matchedValue,
            RelevanceScore = relevanceScore,
            LastUpdated = place.DateAdded
        };
    }
    
    public static SearchResult FromPriceRecord(PriceRecord record, string matchedField, string? matchedValue, double relevanceScore, string? itemName = null, string? category = null)
    {
        var displayItemName = itemName ?? $"Item {record.ItemId[..Math.Min(8, record.ItemId.Length)]}...";
        
        return new SearchResult
        {
            ResultType = SearchResultType.PriceRecord,
            Title = $"${record.Price:F2} - {displayItemName}",
            Subtitle = $"Store {record.PlaceId[..Math.Min(8, record.PlaceId.Length)]}...",
            Description = record.IsOnSale ? $"On sale! (was ${record.OriginalPrice:F2})" : "Regular price",
            Category = category ?? "Price Record",
            Icon = record.IsOnSale ? "🏷️" : "💰",
            Entity = record,
            EntityId = record.Id,
            MatchedField = matchedField,
            MatchedValue = matchedValue,
            RelevanceScore = relevanceScore,
            LastUpdated = record.DateRecorded
        };
    }
    
    private static string GetItemIcon(string? category)
    {
        return category?.ToLowerInvariant() switch
        {
            var c when c?.Contains("dairy") == true || c?.Contains("milk") == true || c?.Contains("cheese") == true => "🥛",
            var c when c?.Contains("meat") == true || c?.Contains("seafood") == true => "🥩",
            var c when c?.Contains("fruit") == true || c?.Contains("vegetable") == true => "🥬",
            var c when c?.Contains("bakery") == true || c?.Contains("bread") == true => "🍞",
            var c when c?.Contains("beverage") == true || c?.Contains("drink") == true => "🥤",
            var c when c?.Contains("snack") == true || c?.Contains("confectionery") == true => "🍫",
            var c when c?.Contains("frozen") == true => "🧊",
            var c when c?.Contains("household") == true => "🏠",
            var c when c?.Contains("personal") == true => "🧴",
            var c when c?.Contains("baby") == true => "👶",
            var c when c?.Contains("pet") == true => "🐾",
            _ => "🛒"
        };
    }
}

public enum SearchResultType
{
    Item,
    Place,
    PriceRecord
}
