using System;

namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a record of a data import operation.
/// </summary>
public class ImportHistory
{
    /// <summary>
    /// Unique identifier for the import record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the import was performed.
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of import performed (StaticPackage, Json, Csv, etc.).
    /// </summary>
    public ImportType ImportType { get; set; } = ImportType.StaticPackage;

    /// <summary>
    /// Source of the import (file path, URL, etc.).
    /// </summary>
    public string? SourcePath { get; set; }

    /// <summary>
    /// Number of stores imported.
    /// </summary>
    public int StoresImported { get; set; }

    /// <summary>
    /// Number of stores skipped (duplicates).
    /// </summary>
    public int StoresSkipped { get; set; }

    /// <summary>
    /// Number of products/items imported.
    /// </summary>
    public int ProductsImported { get; set; }

    /// <summary>
    /// Number of products skipped (duplicates).
    /// </summary>
    public int ProductsSkipped { get; set; }

    /// <summary>
    /// Number of price records imported.
    /// </summary>
    public int PricesImported { get; set; }

    /// <summary>
    /// Number of price records skipped.
    /// </summary>
    public int PricesSkipped { get; set; }

    /// <summary>
    /// Total size of the import in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Package ID for static imports.
    /// </summary>
    public string? PackageId { get; set; }

    /// <summary>
    /// Description or notes about the import.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the import was successful.
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Error message if the import failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the import operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Duplicate handling strategy used.
    /// </summary>
    public string? DuplicateStrategy { get; set; }

    /// <summary>
    /// Number of errors encountered during import.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets the total number of entities processed (imported + skipped).
    /// </summary>
    public int TotalEntitiesProcessed => StoresImported + StoresSkipped + ProductsImported + ProductsSkipped + PricesImported + PricesSkipped;

    /// <summary>
    /// Gets a formatted string of the import size.
    /// </summary>
    public string FormattedSize
    {
        get
        {
            if (TotalSizeBytes >= 1024 * 1024 * 1024)
                return $"{TotalSizeBytes / (1024.0 * 1024 * 1024):F2} GB";
            if (TotalSizeBytes >= 1024 * 1024)
                return $"{TotalSizeBytes / (1024.0 * 1024):F2} MB";
            if (TotalSizeBytes >= 1024)
                return $"{TotalSizeBytes / 1024.0:F2} KB";
            return $"{TotalSizeBytes} B";
        }
    }
}

/// <summary>
/// Types of data imports supported.
/// </summary>
public enum ImportType
{
    StaticPackage,
    Json,
    Csv,
    Markdown,
    Xml
}
