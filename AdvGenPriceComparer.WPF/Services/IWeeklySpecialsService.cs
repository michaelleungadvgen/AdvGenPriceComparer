using System;
using System.Collections.Generic;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Represents a special deal in the weekly digest
/// </summary>
public class WeeklySpecialDeal
{
    public string ItemId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal Savings => OriginalPrice.HasValue ? OriginalPrice.Value - Price : 0;
    public double SavingsPercentage => OriginalPrice.HasValue && OriginalPrice.Value > 0 
        ? (double)((OriginalPrice.Value - Price) / OriginalPrice.Value * 100) 
        : 0;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public bool IsHalfPrice => SavingsPercentage >= 45;
}

/// <summary>
/// Weekly digest report containing all specials organized by category and store
/// </summary>
public class WeeklyDigestReport
{
    public DateTime ReportDate { get; set; } = DateTime.Today;
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public int TotalDeals { get; set; }
    public int HalfPriceDeals { get; set; }
    public List<WeeklySpecialDeal> AllDeals { get; set; } = new();
    public Dictionary<string, List<WeeklySpecialDeal>> ByCategory { get; set; } = new();
    public Dictionary<string, List<WeeklySpecialDeal>> ByStore { get; set; } = new();
    public List<WeeklySpecialDeal> BestDeals => AllDeals
        .Where(d => d.SavingsPercentage > 0)
        .OrderByDescending(d => d.SavingsPercentage)
        .Take(10)
        .ToList();
}

/// <summary>
/// Service for generating weekly specials digests
/// </summary>
public interface IWeeklySpecialsService
{
    /// <summary>
    /// Generates a weekly digest report for the current week
    /// </summary>
    WeeklyDigestReport GenerateWeeklyDigest();

    /// <summary>
    /// Generates a weekly digest for a specific date range
    /// </summary>
    WeeklyDigestReport GenerateDigestForDateRange(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Gets deals for a specific category
    /// </summary>
    List<WeeklySpecialDeal> GetDealsByCategory(string category);

    /// <summary>
    /// Gets deals for a specific store
    /// </summary>
    List<WeeklySpecialDeal> GetDealsByStore(string storeName);

    /// <summary>
    /// Exports the digest to markdown format for sharing
    /// </summary>
    string ExportToMarkdown(WeeklyDigestReport report);

    /// <summary>
    /// Exports the digest to plain text format
    /// </summary>
    string ExportToPlainText(WeeklyDigestReport report);

    /// <summary>
    /// Gets all available categories with deals
    /// </summary>
    List<string> GetAvailableCategories();

    /// <summary>
    /// Gets all available stores with deals
    /// </summary>
    List<string> GetAvailableStores();
}
