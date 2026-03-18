using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Supported supermarket chains for weekly specials import
/// </summary>
public enum SupermarketChain
{
    Coles,
    Woolworths,
    Aldi,
    Drakes
}

/// <summary>
/// Represents the result of a weekly specials import operation
/// </summary>
public class WeeklySpecialsImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalItems { get; set; }
    public int ImportedItems { get; set; }
    public int SkippedItems { get; set; }
    public int FailedItems { get; set; }
    public List<string> Errors { get; set; } = new();
    public DateTime ImportDate { get; set; } = DateTime.Now;
    public SupermarketChain Chain { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}

/// <summary>
/// Options for importing weekly specials
/// </summary>
public class WeeklySpecialsImportOptions
{
    public SupermarketChain Chain { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool SkipDuplicates { get; set; } = true;
    public bool AutoCategorize { get; set; } = true;
    public IProgress<WeeklySpecialsImportProgress>? Progress { get; set; }
}

/// <summary>
/// Progress information for weekly specials import
/// </summary>
public class WeeklySpecialsImportProgress
{
    public int CurrentItem { get; set; }
    public int TotalItems { get; set; }
    public string CurrentProductName { get; set; } = string.Empty;
    public int PercentageComplete => TotalItems > 0 ? (CurrentItem * 100) / TotalItems : 0;
    public string StatusMessage { get; set; } = string.Empty;
}

/// <summary>
/// Service interface for importing weekly specials from supermarket catalogues
/// </summary>
public interface IWeeklySpecialsImportService
{
    /// <summary>
    /// Import weekly specials from a file (JSON or Markdown format)
    /// </summary>
    Task<WeeklySpecialsImportResult> ImportFromFileAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import weekly specials from JSON format (Coles, Woolworths)
    /// </summary>
    Task<WeeklySpecialsImportResult> ImportFromJsonAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Import weekly specials from Markdown format (ALDI, Drakes)
    /// </summary>
    Task<WeeklySpecialsImportResult> ImportFromMarkdownAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Preview the specials before importing
    /// </summary>
    Task<List<WeeklySpecialItem>> PreviewImportAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get supported file extensions for a supermarket chain
    /// </summary>
    string[] GetSupportedExtensions(SupermarketChain chain);

    /// <summary>
    /// Detect the supermarket chain from file content
    /// </summary>
    Task<SupermarketChain?> DetectChainAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract date range from catalogue file (if available)
    /// </summary>
    Task<(DateTime? ValidFrom, DateTime? ValidTo)> ExtractDateRangeAsync(string filePath, SupermarketChain chain, CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a weekly special item for preview/import
/// </summary>
public class WeeklySpecialItem
{
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? Savings => OriginalPrice.HasValue ? OriginalPrice.Value - Price : null;
    public string SpecialType { get; set; } = string.Empty;
    public SupermarketChain Chain { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsHalfPrice => SpecialType?.ToLower().Contains("half") ?? false;
    public decimal? SavingsPercentage => OriginalPrice.HasValue && OriginalPrice.Value > 0
        ? ((OriginalPrice.Value - Price) / OriginalPrice.Value) * 100
        : null;
}
