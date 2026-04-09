using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Services;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

/// <summary>
/// LiteDB implementation of export history repository.
/// </summary>
public class ExportHistoryRepository : IExportHistoryRepository
{
    private readonly ILiteCollection<ExportHistoryEntity> _collection;

    public ExportHistoryRepository(DatabaseService databaseService)
    {
        _collection = databaseService.Database.GetCollection<ExportHistoryEntity>("export_history");
        
        // Create indexes
        _collection.EnsureIndex(x => x.ExportedAt);
        _collection.EnsureIndex(x => x.ExportType);
        _collection.EnsureIndex(x => x.IsSuccessful);
        _collection.EnsureIndex(x => x.ExportId);
    }

    /// <inheritdoc />
    public void Add(ExportHistory exportHistory)
    {
        var entity = ExportHistoryEntity.FromModel(exportHistory);
        _collection.Insert(entity);
    }

    /// <inheritdoc />
    public ExportHistory? GetById(string id)
    {
        var entity = _collection.FindOne(x => x.ExportId == id);
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetAll()
    {
        return _collection.FindAll()
            .OrderByDescending(x => x.ExportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _collection.Find(x => x.ExportedAt >= startDate && x.ExportedAt <= endDate)
            .OrderByDescending(x => x.ExportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetRecent(int count)
    {
        return _collection.FindAll()
            .OrderByDescending(x => x.ExportedAt)
            .Take(count)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ExportHistory> GetByType(ExportType exportType)
    {
        return _collection.Find(x => x.ExportType == exportType)
            .OrderByDescending(x => x.ExportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public bool Delete(string id)
    {
        var entity = _collection.FindOne(x => x.ExportId == id);
        if (entity == null)
            return false;
        
        return _collection.Delete(entity.Id);
    }

    /// <inheritdoc />
    public int DeleteOlderThan(DateTime date)
    {
        var idsToDelete = _collection.Find(x => x.ExportedAt < date)
            .Select(x => x.Id)
            .ToList();
        
        int deletedCount = 0;
        foreach (var id in idsToDelete)
        {
            if (_collection.Delete(id))
                deletedCount++;
        }
        
        return deletedCount;
    }

    /// <inheritdoc />
    public int Count()
    {
        return _collection.Count();
    }

    /// <inheritdoc />
    public ExportStatistics GetStatistics(DateTime startDate, DateTime endDate)
    {
        var exports = _collection.Find(x => x.ExportedAt >= startDate && x.ExportedAt <= endDate).ToList();
        
        if (!exports.Any())
        {
            return new ExportStatistics
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
        var averageDurationMs = successful.Any() ? successful.Average(x => x.DurationMs) : 0;

        return new ExportStatistics
        {
            TotalExports = exports.Count,
            SuccessfulExports = successful.Count,
            FailedExports = exports.Count(x => !x.IsSuccessful),
            TotalSizeBytes = exports.Sum(x => x.TotalSizeBytes),
            TotalStoresExported = exports.Sum(x => x.StoresExported),
            TotalProductsExported = exports.Sum(x => x.ProductsExported),
            TotalPricesExported = exports.Sum(x => x.PricesExported),
            AverageDuration = TimeSpan.FromMilliseconds(averageDurationMs)
        };
    }

    /// <summary>
    /// Calculate the total size of files in a directory.
    /// </summary>
    public static long CalculateDirectorySize(string path)
    {
        if (!Directory.Exists(path))
            return 0;

        long size = 0;
        var dir = new DirectoryInfo(path);
        
        foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
        {
            size += file.Length;
        }

        return size;
    }
}
