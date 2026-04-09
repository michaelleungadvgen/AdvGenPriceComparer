using System;
using System.Collections.Generic;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Repository interface for import history records.
/// </summary>
public interface IImportHistoryRepository
{
    /// <summary>
    /// Add a new import history record.
    /// </summary>
    void Add(ImportHistory importHistory);

    /// <summary>
    /// Get an import history record by ID.
    /// </summary>
    ImportHistory? GetById(string id);

    /// <summary>
    /// Get all import history records.
    /// </summary>
    IEnumerable<ImportHistory> GetAll();

    /// <summary>
    /// Get import history records for a specific date range.
    /// </summary>
    IEnumerable<ImportHistory> GetByDateRange(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get the most recent import history records.
    /// </summary>
    IEnumerable<ImportHistory> GetRecent(int count);

    /// <summary>
    /// Get import history records by import type.
    /// </summary>
    IEnumerable<ImportHistory> GetByType(ImportType importType);

    /// <summary>
    /// Get import history records by source path (contains search).
    /// </summary>
    IEnumerable<ImportHistory> GetBySource(string sourcePath);

    /// <summary>
    /// Delete an import history record.
    /// </summary>
    bool Delete(string id);

    /// <summary>
    /// Delete import history records older than the specified date.
    /// </summary>
    int DeleteOlderThan(DateTime date);

    /// <summary>
    /// Get the total count of import history records.
    /// </summary>
    int Count();

    /// <summary>
    /// Get import statistics for a date range.
    /// </summary>
    ImportStatistics GetStatistics(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get the most recent import for a specific package ID.
    /// </summary>
    ImportHistory? GetMostRecentByPackageId(string packageId);
}

/// <summary>
/// Statistics for import operations.
/// </summary>
public class ImportStatistics
{
    /// <summary>
    /// Total number of imports in the period.
    /// </summary>
    public int TotalImports { get; set; }

    /// <summary>
    /// Number of successful imports.
    /// </summary>
    public int SuccessfulImports { get; set; }

    /// <summary>
    /// Number of failed imports.
    /// </summary>
    public int FailedImports { get; set; }

    /// <summary>
    /// Total size of all imports in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Total number of stores imported.
    /// </summary>
    public int TotalStoresImported { get; set; }

    /// <summary>
    /// Total number of products imported.
    /// </summary>
    public int TotalProductsImported { get; set; }

    /// <summary>
    /// Total number of prices imported.
    /// </summary>
    public int TotalPricesImported { get; set; }

    /// <summary>
    /// Total number of entities skipped (duplicates).
    /// </summary>
    public int TotalEntitiesSkipped { get; set; }

    /// <summary>
    /// Total number of errors across all imports.
    /// </summary>
    public int TotalErrors { get; set; }

    /// <summary>
    /// Average import duration.
    /// </summary>
    public TimeSpan AverageDuration { get; set; }
}
