using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.Core.Interfaces;

/// <summary>
/// Repository interface for export history records.
/// </summary>
public interface IExportHistoryRepository
{
    /// <summary>
    /// Add a new export history record.
    /// </summary>
    void Add(ExportHistory exportHistory);

    /// <summary>
    /// Get an export history record by ID.
    /// </summary>
    ExportHistory? GetById(string id);

    /// <summary>
    /// Get all export history records.
    /// </summary>
    IEnumerable<ExportHistory> GetAll();

    /// <summary>
    /// Get export history records for a specific date range.
    /// </summary>
    IEnumerable<ExportHistory> GetByDateRange(DateTime startDate, DateTime endDate);

    /// <summary>
    /// Get the most recent export history records.
    /// </summary>
    IEnumerable<ExportHistory> GetRecent(int count);

    /// <summary>
    /// Get export history records by export type.
    /// </summary>
    IEnumerable<ExportHistory> GetByType(ExportType exportType);

    /// <summary>
    /// Delete an export history record.
    /// </summary>
    bool Delete(string id);

    /// <summary>
    /// Delete export history records older than the specified date.
    /// </summary>
    int DeleteOlderThan(DateTime date);

    /// <summary>
    /// Get the total count of export history records.
    /// </summary>
    int Count();

    /// <summary>
    /// Get export statistics for a date range.
    /// </summary>
    ExportStatistics GetStatistics(DateTime startDate, DateTime endDate);
}

/// <summary>
/// Statistics for export operations.
/// </summary>
public class ExportStatistics
{
    /// <summary>
    /// Total number of exports in the period.
    /// </summary>
    public int TotalExports { get; set; }

    /// <summary>
    /// Number of successful exports.
    /// </summary>
    public int SuccessfulExports { get; set; }

    /// <summary>
    /// Number of failed exports.
    /// </summary>
    public int FailedExports { get; set; }

    /// <summary>
    /// Total size of all exports in bytes.
    /// </summary>
    public long TotalSizeBytes { get; set; }

    /// <summary>
    /// Total number of stores exported.
    /// </summary>
    public int TotalStoresExported { get; set; }

    /// <summary>
    /// Total number of products exported.
    /// </summary>
    public int TotalProductsExported { get; set; }

    /// <summary>
    /// Total number of prices exported.
    /// </summary>
    public int TotalPricesExported { get; set; }

    /// <summary>
    /// Average export duration.
    /// </summary>
    public TimeSpan AverageDuration { get; set; }
}
