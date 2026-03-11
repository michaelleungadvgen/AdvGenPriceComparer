namespace AdvGenPriceComparer.Application.DTOs;

/// <summary>
/// Request DTO for importing data
/// </summary>
public class ImportRequestDto
{
    /// <summary>
    /// Path to the file to import
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// Optional store ID to associate with imported items
    /// </summary>
    public string? StoreId { get; set; }

    /// <summary>
    /// Import options (duplicate handling, auto-categorization, etc.)
    /// </summary>
    public ImportOptionsDto Options { get; set; } = new();
}

/// <summary>
/// Options for importing data
/// </summary>
public class ImportOptionsDto
{
    /// <summary>
    /// How to handle duplicate products
    /// </summary>
    public DuplicateHandlingStrategy DuplicateStrategy { get; set; } = DuplicateHandlingStrategy.Skip;

    /// <summary>
    /// Enable auto-categorization using ML
    /// </summary>
    public bool EnableAutoCategorization { get; set; } = true;

    /// <summary>
    /// Minimum confidence threshold for auto-categorization (0.0 - 1.0)
    /// </summary>
    public float AutoCategorizationThreshold { get; set; } = 0.7f;

    /// <summary>
    /// Validate data before importing
    /// </summary>
    public bool ValidateData { get; set; } = true;

    /// <summary>
    /// Continue importing on error (skip problematic items)
    /// </summary>
    public bool ContinueOnError { get; set; } = true;
}

/// <summary>
/// Strategy for handling duplicate products during import
/// </summary>
public enum DuplicateHandlingStrategy
{
    /// <summary>
    /// Skip duplicate items (keep existing)
    /// </summary>
    Skip,

    /// <summary>
    /// Overwrite existing items with new data
    /// </summary>
    Overwrite,

    /// <summary>
    /// Update only price information (keep metadata)
    /// </summary>
    UpdatePriceOnly,

    /// <summary>
    /// Create new items even if duplicates exist
    /// </summary>
    CreateNew
}

/// <summary>
/// Preview DTO for import operations (before actual import)
/// </summary>
public class ImportPreviewDto
{
    /// <summary>
    /// Items that would be imported
    /// </summary>
    public List<ImportPreviewItemDto> Items { get; set; } = new();

    /// <summary>
    /// Total count of items
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Count of new items (not duplicates)
    /// </summary>
    public int NewItemsCount { get; set; }

    /// <summary>
    /// Count of duplicate items
    /// </summary>
    public int DuplicateCount { get; set; }

    /// <summary>
    /// Count of items that would be auto-categorized
    /// </summary>
    public int WouldBeAutoCategorizedCount { get; set; }
}
