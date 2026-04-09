using System;
using AdvGenPriceComparer.Core.Models;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Entities;

/// <summary>
/// LiteDB entity for export history records.
/// </summary>
public class ExportHistoryEntity
{
    /// <summary>
    /// LiteDB document ID.
    /// </summary>
    [BsonId]
    public ObjectId Id { get; set; } = ObjectId.NewObjectId();

    /// <summary>
    /// Business identifier for the export record.
    /// </summary>
    public string ExportId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Timestamp when the export was performed.
    /// </summary>
    public DateTime ExportedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Type of export performed.
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
    /// Duration of the export operation in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Filter criteria used for the export (if any).
    /// </summary>
    public string? FilterCriteria { get; set; }

    /// <summary>
    /// Convert entity to domain model.
    /// </summary>
    public ExportHistory ToModel()
    {
        return new ExportHistory
        {
            Id = ExportId,
            ExportedAt = ExportedAt,
            ExportType = ExportType,
            StoresExported = StoresExported,
            ProductsExported = ProductsExported,
            PricesExported = PricesExported,
            TotalSizeBytes = TotalSizeBytes,
            OutputPath = OutputPath,
            PackageId = PackageId,
            Description = Description,
            IsSuccessful = IsSuccessful,
            ErrorMessage = ErrorMessage,
            Duration = TimeSpan.FromMilliseconds(DurationMs),
            FilterCriteria = FilterCriteria
        };
    }

    /// <summary>
    /// Create entity from domain model.
    /// </summary>
    public static ExportHistoryEntity FromModel(ExportHistory model)
    {
        return new ExportHistoryEntity
        {
            ExportId = model.Id,
            ExportedAt = model.ExportedAt,
            ExportType = model.ExportType,
            StoresExported = model.StoresExported,
            ProductsExported = model.ProductsExported,
            PricesExported = model.PricesExported,
            TotalSizeBytes = model.TotalSizeBytes,
            OutputPath = model.OutputPath,
            PackageId = model.PackageId,
            Description = model.Description,
            IsSuccessful = model.IsSuccessful,
            ErrorMessage = model.ErrorMessage,
            DurationMs = (long)model.Duration.TotalMilliseconds,
            FilterCriteria = model.FilterCriteria
        };
    }
}
