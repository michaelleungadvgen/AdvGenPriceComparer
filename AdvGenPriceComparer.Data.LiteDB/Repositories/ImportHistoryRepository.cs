using System;
using System.Collections.Generic;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Services;
using LiteDB;

namespace AdvGenPriceComparer.Data.LiteDB.Repositories;

/// <summary>
/// LiteDB implementation of import history repository.
/// </summary>
public class ImportHistoryRepository : IImportHistoryRepository
{
    private readonly ILiteCollection<ImportHistoryEntity> _collection;

    public ImportHistoryRepository(DatabaseService databaseService)
    {
        _collection = databaseService.Database.GetCollection<ImportHistoryEntity>("import_history");
        
        // Create indexes
        _collection.EnsureIndex(x => x.ImportedAt);
        _collection.EnsureIndex(x => x.ImportType);
        _collection.EnsureIndex(x => x.IsSuccessful);
        _collection.EnsureIndex(x => x.ImportId);
        _collection.EnsureIndex(x => x.PackageId);
        _collection.EnsureIndex(x => x.SourcePath);
    }

    /// <inheritdoc />
    public void Add(ImportHistory importHistory)
    {
        var entity = ImportHistoryEntity.FromModel(importHistory);
        _collection.Insert(entity);
    }

    /// <inheritdoc />
    public ImportHistory? GetById(string id)
    {
        var entity = _collection.FindOne(x => x.ImportId == id);
        return entity?.ToModel();
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetAll()
    {
        return _collection.FindAll()
            .OrderByDescending(x => x.ImportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        return _collection.Find(x => x.ImportedAt >= startDate && x.ImportedAt <= endDate)
            .OrderByDescending(x => x.ImportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetRecent(int count)
    {
        return _collection.FindAll()
            .OrderByDescending(x => x.ImportedAt)
            .Take(count)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetByType(ImportType importType)
    {
        return _collection.Find(x => x.ImportType == importType)
            .OrderByDescending(x => x.ImportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public IEnumerable<ImportHistory> GetBySource(string sourcePath)
    {
        return _collection.Find(x => x.SourcePath != null && x.SourcePath.Contains(sourcePath))
            .OrderByDescending(x => x.ImportedAt)
            .Select(x => x.ToModel());
    }

    /// <inheritdoc />
    public bool Delete(string id)
    {
        var entity = _collection.FindOne(x => x.ImportId == id);
        if (entity == null)
            return false;
        
        return _collection.Delete(entity.Id);
    }

    /// <inheritdoc />
    public int DeleteOlderThan(DateTime date)
    {
        var idsToDelete = _collection.Find(x => x.ImportedAt < date)
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
    public ImportStatistics GetStatistics(DateTime startDate, DateTime endDate)
    {
        var imports = _collection.Find(x => x.ImportedAt >= startDate && x.ImportedAt <= endDate).ToList();
        
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
        var averageDurationMs = successful.Any() ? successful.Average(x => x.DurationMs) : 0;

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
            AverageDuration = TimeSpan.FromMilliseconds(averageDurationMs)
        };
    }

    /// <inheritdoc />
    public ImportHistory? GetMostRecentByPackageId(string packageId)
    {
        return _collection.Find(x => x.PackageId == packageId)
            .OrderByDescending(x => x.ImportedAt)
            .FirstOrDefault()
            ?.ToModel();
    }
}
