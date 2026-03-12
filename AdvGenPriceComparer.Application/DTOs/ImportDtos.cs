namespace AdvGenPriceComparer.Application.DTOs;

/// <summary>
/// Result of an import operation
/// </summary>
public class ImportResultDto
{
    /// <summary>
    /// Whether the import was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total number of items processed
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items successfully imported
    /// </summary>
    public int ImportedCount { get; set; }

    /// <summary>
    /// Number of items skipped (duplicates)
    /// </summary>
    public int SkippedCount { get; set; }

    /// <summary>
    /// Number of items that failed to import
    /// </summary>
    public int FailedCount { get; set; }

    /// <summary>
    /// Number of items auto-categorized by ML
    /// </summary>
    public int AutoCategorizedCount { get; set; }

    /// <summary>
    /// Duration of the import operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Error messages if any
    /// </summary>
    public List<ImportErrorDto> Errors { get; set; } = new();

    /// <summary>
    /// Imported item summaries
    /// </summary>
    public List<ImportedItemSummaryDto> ImportedItems { get; set; } = new();
}

/// <summary>
/// Progress information during import
/// </summary>
public class ImportProgressDto
{
    /// <summary>
    /// Total number of items to process
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items processed so far
    /// </summary>
    public int ProcessedItems { get; set; }

    /// <summary>
    /// Current item being processed
    /// </summary>
    public string? CurrentItemName { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int PercentComplete => TotalItems > 0 ? (ProcessedItems * 100) / TotalItems : 0;

    /// <summary>
    /// Status message
    /// </summary>
    public string? StatusMessage { get; set; }
}

/// <summary>
/// Error information for import failures
/// </summary>
public class ImportErrorDto
{
    /// <summary>
    /// Type of error
    /// </summary>
    public ImportErrorType ErrorType { get; set; }

    /// <summary>
    /// Error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Field that caused the error (if applicable)
    /// </summary>
    public string? FieldName { get; set; }

    /// <summary>
    /// Product name that caused the error
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Line number in source file (if applicable)
    /// </summary>
    public int? LineNumber { get; set; }

    /// <summary>
    /// Raw data that caused the error
    /// </summary>
    public string? RawData { get; set; }
}

/// <summary>
/// Types of import errors
/// </summary>
public enum ImportErrorType
{
    ValidationError,
    FileNotFound,
    InvalidJson,
    InvalidData,
    DatabaseError,
    ParsingError,
    UnknownError
}

/// <summary>
/// Summary of an imported item
/// </summary>
public class ImportedItemSummaryDto
{
    /// <summary>
    /// Item ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brand name
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Whether the item was auto-categorized
    /// </summary>
    public bool WasAutoCategorized { get; set; }

    /// <summary>
    /// Auto-categorization confidence (0-1)
    /// </summary>
    public float? AutoCategorizationConfidence { get; set; }
}

/// <summary>
/// Preview item for import preview
/// </summary>
public class ImportPreviewItemDto
{
    /// <summary>
    /// Product ID from source
    /// </summary>
    public string? ProductId { get; set; }

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brand name
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Current price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Original price (for sales)
    /// </summary>
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// Savings amount
    /// </summary>
    public decimal? Savings { get; set; }

    /// <summary>
    /// Special type (e.g., "Half Price")
    /// </summary>
    public string? SpecialType { get; set; }

    /// <summary>
    /// Package size
    /// </summary>
    public string? PackageSize { get; set; }

    /// <summary>
    /// Unit (ea, kg, litre, etc.)
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Whether this item already exists in the database
    /// </summary>
    public bool IsDuplicate { get; set; }

    /// <summary>
    /// Action that will be taken (Import, Skip, Update)
    /// </summary>
    public string Action { get; set; } = "Import";

    /// <summary>
    /// Validation errors for this item
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();
}
