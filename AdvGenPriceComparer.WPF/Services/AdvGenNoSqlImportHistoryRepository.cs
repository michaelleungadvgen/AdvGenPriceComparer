using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// AdvGenNoSQLServer implementation of import history repository.
/// Note: Import history is primarily a local concern and is not synchronized 
/// with the remote server. This implementation provides local-only storage.
/// </summary>
public class AdvGenNoSqlImportHistoryRepository : IImportHistoryRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private readonly List<ImportHistory> _localCache = new();

    public AdvGenNoSqlImportHistoryRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Add(ImportHistory importHistory)
    {
        _localCache.Add(importHistory);
        _logger.LogDebug($"Import history added to local cache: {importHistory.Id}");
    }

    /// <inheritdoc />
    public ImportHistory? GetById(string id)
    {
        return _localCache.FirstOrDefault(e => e.Id == id);
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetAll()
    {
        return _localCache.OrderByDescending(e => e.ImportedAt).ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _localCache
            .Where(e => e.ImportedAt >= startDate && e.ImportedAt <= endDate)
            .OrderByDescending(e => e.ImportedAt)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetRecent(int count)
    {
        return _localCache
            .OrderByDescending(e => e.ImportedAt)
            .Take(count)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetByType(ImportType importType)
    {
        return _localCache
            .Where(e => e.ImportType == importType)
            .OrderByDescending(e => e.ImportedAt)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetBySource(string sourcePath)
    {
        return _localCache
            .Where(e => e.SourcePath != null && e.SourcePath.Contains(sourcePath))
            .OrderByDescending(e => e.ImportedAt)
            .ToList();
    }

    /// <inheritdoc />
    public bool Delete(string id)
    {
        var item = _localCache.FirstOrDefault(e => e.Id == id);
        if (item != null)
        {
            _localCache.Remove(item);
            return true;
        }
        return false;
    }

    /// <inheritdoc />
    public int DeleteOlderThan(DateTime date)
    {
        var toDelete = _localCache.Where(e => e.ImportedAt < date).ToList();
        foreach (var item in toDelete)
        {
            _localCache.Remove(item);
        }
        return toDelete.Count;
    }

    /// <inheritdoc />
    public int Count()
    {
        return _localCache.Count;
    }

    /// <inheritdoc />
    public ImportStatistics GetStatistics(DateTime startDate, DateTime endDate)
    {
        var imports = _localCache.Where(e => e.ImportedAt >= startDate && e.ImportedAt <= endDate).ToList();
        
        if (!imports.Any())
        {
            return new ImportStatistics
            {
                TotalImports = 0,
                SuccessfulImports = 0,
                FailedImports = 0,
                TotalSizeBytes = 0,
                TotalStoresImported = 0,
                TotalProductsImported = 0,
                TotalPricesImported = 0,
                TotalEntitiesSkipped = 0,
                TotalErrors = 0,
                AverageDuration = TimeSpan.Zero
            };
        }

        var successful = imports.Where(x => x.IsSuccessful).ToList();
        var averageDuration = successful.Any() 
            ? TimeSpan.FromTicks((long)successful.Average(x => x.Duration.Ticks)) 
            : TimeSpan.Zero;

        return new ImportStatistics
        {
            TotalImports = imports.Count,
            SuccessfulImports = successful.Count,
            FailedImports = imports.Count(x => !x.IsSuccessful),
            TotalSizeBytes = imports.Sum(x => x.TotalSizeBytes),
            TotalStoresImported = imports.Sum(x => x.StoresImported),
            TotalProductsImported = imports.Sum(x => x.ProductsImported),
            TotalPricesImported = imports.Sum(x => x.PricesImported),
            TotalEntitiesSkipped = imports.Sum(x => x.StoresSkipped + x.ProductsSkipped + x.PricesSkipped),
            TotalErrors = imports.Sum(x => x.ErrorCount),
            AverageDuration = averageDuration
        };
    }

    /// <inheritdoc />
    public ImportHistory? GetMostRecentByPackageId(string packageId)
    {
        return _localCache
            .Where(e => e.PackageId == packageId)
            .OrderByDescending(e => e.ImportedAt)
            .FirstOrDefault();
    }
}
