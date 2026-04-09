using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// AdvGenNoSQLServer implementation of export history repository.
/// Note: Export history is primarily a local concern and is not synchronized 
/// with the remote server. This implementation provides local-only storage.
/// </summary>
public class AdvGenNoSqlExportHistoryRepository : IExportHistoryRepository
{
    private readonly AdvGenNoSqlProvider _provider;
    private readonly ILoggerService _logger;
    private readonly List<ExportHistory> _localCache = new();

    public AdvGenNoSqlExportHistoryRepository(AdvGenNoSqlProvider provider, ILoggerService logger)
    {
        _provider = provider;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Add(ExportHistory exportHistory)
    {
        _localCache.Add(exportHistory);
        _logger.LogDebug($"Export history added to local cache: {exportHistory.Id}");
    }

    /// <inheritdoc />
    public ExportHistory? GetById(string id)
    {
        return _localCache.FirstOrDefault(e => e.Id == id);
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetAll()
    {
        return _localCache.OrderByDescending(e => e.ExportedAt).ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _localCache
            .Where(e => e.ExportedAt >= startDate && e.ExportedAt <= endDate)
            .OrderByDescending(e => e.ExportedAt)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetRecent(int count)
    {
        return _localCache
            .OrderByDescending(e => e.ExportedAt)
            .Take(count)
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetByType(ExportType exportType)
    {
        return _localCache
            .Where(e => e.ExportType == exportType)
            .OrderByDescending(e => e.ExportedAt)
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
        var toDelete = _localCache.Where(e => e.ExportedAt < date).ToList();
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
    public Core.Interfaces.ExportStatistics GetStatistics(DateTime startDate, DateTime endDate)
    {
        var exports = _localCache.Where(e => e.ExportedAt >= startDate && e.ExportedAt <= endDate).ToList();
        
        if (!exports.Any())
        {
            return new Core.Interfaces.ExportStatistics
            {
                TotalExports = 0,
                SuccessfulExports = 0,
                FailedExports = 0,
                TotalSizeBytes = 0,
                TotalStoresExported = 0,
                TotalProductsExported = 0,
                TotalPricesExported = 0,
                AverageDuration = TimeSpan.Zero
            };
        }

        var successful = exports.Where(x => x.IsSuccessful).ToList();
        var averageDuration = successful.Any() 
            ? TimeSpan.FromTicks((long)successful.Average(x => x.Duration.Ticks)) 
            : TimeSpan.Zero;

        return new Core.Interfaces.ExportStatistics
        {
            TotalExports = exports.Count,
            SuccessfulExports = successful.Count,
            FailedExports = exports.Count(x => !x.IsSuccessful),
            TotalSizeBytes = exports.Sum(x => x.TotalSizeBytes),
            TotalStoresExported = exports.Sum(x => x.StoresExported),
            TotalProductsExported = exports.Sum(x => x.ProductsExported),
            TotalPricesExported = exports.Sum(x => x.PricesExported),
            AverageDuration = averageDuration
        };
    }
}
