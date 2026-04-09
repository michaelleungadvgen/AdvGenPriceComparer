using System;

namespace AdvGenPriceComparer.Core.Models;

/// <summary>
/// Represents a record of a data export operation.
/// </summary>
public class ExportHistory
{
    /// <summary>
    /// Unique identifier for the export record.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the export was performed.
    /// </summary>
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of export performed (StaticPackage, Json, Csv, etc.).
    /// </summary>
    public ExportType ExportType { get; set; } = ExportType.StaticPackage;

    /// <summary>
    /// Number of stores exported.
    /// </summary>
    public int StoresExported { get; set; }

    /// <summary>
    /// Number of products/items exported.
    /// </summary>
    public int ProductsExported { get; set; }

    /// <summary>
    /// Number of price records exported.
    /// </summary>
    public int PricesExported { get; set; }

    /// <summary>
    /// Total size of the export in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Output path or directory where the export was saved.
    /// </summary>
    public string? OutputPath { get; set; }

    /// <summary>
    /// Package ID for static exports.
    /// </summary>
    public string? PackageId { get; set; }

    /// <summary>
    /// Description or notes about the export.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether the export was successful.
    /// </summary>
    public bool IsSuccessful { get; set; } = true;

    /// <summary>
    /// Error message if the export failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Duration of the export operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Filter criteria used for the export (if any).
    /// </summary>
    public string? FilterCriteria { get; set; }

    /// <summary>
    /// Gets a formatted string of the export size.
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
/// Types of data exports supported.
/// </summary>
public enum ExportType
{
    StaticPackage,
    Json,
    Csv,
    Markdown,
    Xml
}
