using System;
using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

/// <summary>
/// LiteDB entity for import history records.
/// </summary>
public class ImportHistoryEntity
{
    /// <summary>
    /// LiteDB document ID.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// Business identifier for the import record.
    /// </summary>
    public string ImportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the import was performed.
    /// </summary>
    public DateTime ImportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of import performed.
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
    /// Number of stores skipped.
    /// </summary>
    public int StoresSkipped { get; set; }

    /// <summary>
    /// Number of products/items imported.
    /// </summary>
    public int ProductsImported { get; set; }

    /// <summary>
    /// Number of products skipped.
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
    /// Duration of the import operation in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Duplicate handling strategy used.
    /// </summary>
    public string? DuplicateStrategy { get; set; }

    /// <summary>
    /// Number of errors encountered during import.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Convert entity to domain model.
    /// </summary>
    public ImportHistory ToModel()
    {
        return new ImportHistory
        {
            Id = ImportId,
            ImportedAt = ImportedAt,
            ImportType = ImportType,
            SourcePath = SourcePath,
            StoresImported = StoresImported,
            StoresSkipped = StoresSkipped,
            ProductsImported = ProductsImported,
            ProductsSkipped = ProductsSkipped,
            PricesImported = PricesImported,
            PricesSkipped = PricesSkipped,
            TotalSizeBytes = TotalSizeBytes,
            PackageId = PackageId,
            Description = Description,
            IsSuccessful = IsSuccessful,
            ErrorMessage = ErrorMessage,
            Duration = TimeSpan.FromMilliseconds(DurationMs),
            DuplicateStrategy = DuplicateStrategy,
            ErrorCount = ErrorCount
        };
    }

    /// <summary>
    /// Create entity from domain model.
    /// </summary>
    public static ImportHistoryEntity FromModel(ImportHistory model)
    {
        return new ImportHistoryEntity
        {
            ImportId = model.Id,
            ImportedAt = model.ImportedAt,
            ImportType = model.ImportType,
            SourcePath = model.SourcePath,
            StoresImported = model.StoresImported,
            StoresSkipped = model.StoresSkipped,
            ProductsImported = model.ProductsImported,
            ProductsSkipped = model.ProductsSkipped,
            PricesImported = model.PricesImported,
            PricesSkipped = model.PricesSkipped,
            TotalSizeBytes = model.TotalSizeBytes,
            PackageId = model.PackageId,
            Description = model.Description,
            IsSuccessful = model.IsSuccessful,
            ErrorMessage = model.ErrorMessage,
            DurationMs = (long)model.Duration.TotalMilliseconds,
            DuplicateStrategy = model.DuplicateStrategy,
            ErrorCount = model.ErrorCount
        };
    }
}
