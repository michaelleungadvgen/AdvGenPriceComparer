using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Repositories;

public class ImportHistoryRepositoryTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _database;
    private readonly ImportHistoryRepository _repository;

    public ImportHistoryRepositoryTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_import_history_{Guid.NewGuid():N}.db");
        _database = new DatabaseService(_testDbPath);
        _repository = new ImportHistoryRepository(_database);
    }

    public void Dispose()
    {
        _database?.Dispose();
        if (File.Exists(_testDbPath))
        {
            try { File.Delete(_testDbPath); } catch { }
        }
    }

    [Fact]
    public void Add_ShouldAddImportHistory()
    {
        // Arrange
        var import = CreateSampleImportHistory();

        // Act
        _repository.Add(import);
        var retrieved = _repository.GetById(import.Id);

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(import.Id, retrieved.Id);
        Assert.Equal(import.StoresImported, retrieved.StoresImported);
        Assert.Equal(import.ProductsImported, retrieved.ProductsImported);
        Assert.Equal(import.PricesImported, retrieved.PricesImported);
    }

    [Fact]
    public void GetById_WithNonExistentId_ShouldReturnNull()
    {
        // Act
        var result = _repository.GetById("non-existent-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ShouldReturnAllImportsOrderedByDateDescending()
    {
        // Arrange
        var import1 = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-1));
        var import2 = CreateSampleImportHistory(importedAt: DateTime.UtcNow);
        var import3 = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-2));
        
        _repository.Add(import1);
        _repository.Add(import2);
        _repository.Add(import3);

        // Act
        var results = _repository.GetAll().ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal(import2.Id, results[0].Id); // Most recent first
        Assert.Equal(import1.Id, results[1].Id);
        Assert.Equal(import3.Id, results[2].Id);
    }

    [Fact]
    public void GetByDateRange_ShouldReturnImportsInRange()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-5);
        var endDate = DateTime.UtcNow.AddDays(-1);
        
        var importInRange = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-3));
        var importBeforeRange = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-10));
        var importAfterRange = CreateSampleImportHistory(importedAt: DateTime.UtcNow);
        
        _repository.Add(importInRange);
        _repository.Add(importBeforeRange);
        _repository.Add(importAfterRange);

        // Act
        var results = _repository.GetByDateRange(startDate, endDate).ToList();

        // Assert
        Assert.Single(results);
        Assert.Equal(importInRange.Id, results[0].Id);
    }

    [Fact]
    public void GetRecent_ShouldReturnMostRecentImports()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _repository.Add(CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-i)));
        }

        // Act
        var results = _repository.GetRecent(5).ToList();

        // Assert
        Assert.Equal(5, results.Count);
        Assert.True(results[0].ImportedAt > results[1].ImportedAt);
    }

    [Fact]
    public void GetByType_ShouldReturnImportsOfSpecifiedType()
    {
        // Arrange
        var staticImport = CreateSampleImportHistory(importType: ImportType.StaticPackage);
        var jsonImport = CreateSampleImportHistory(importType: ImportType.Json);
        
        _repository.Add(staticImport);
        _repository.Add(jsonImport);

        // Act
        var staticResults = _repository.GetByType(ImportType.StaticPackage).ToList();
        var jsonResults = _repository.GetByType(ImportType.Json).ToList();

        // Assert
        Assert.Single(staticResults);
        Assert.Single(jsonResults);
        Assert.Equal(staticImport.Id, staticResults[0].Id);
        Assert.Equal(jsonImport.Id, jsonResults[0].Id);
    }

    [Fact]
    public void GetBySource_ShouldReturnImportsMatchingSourcePath()
    {
        // Arrange
        var import1 = CreateSampleImportHistory(sourcePath: "C:\\imports\\package1.zip");
        var import2 = CreateSampleImportHistory(sourcePath: "C:\\imports\\package2.zip");
        var import3 = CreateSampleImportHistory(sourcePath: "http://example.com/data.zip");
        
        _repository.Add(import1);
        _repository.Add(import2);
        _repository.Add(import3);

        // Act
        var results = _repository.GetBySource("imports").ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, r => r.Id == import1.Id);
        Assert.Contains(results, r => r.Id == import2.Id);
    }

    [Fact]
    public void Delete_ShouldRemoveImportHistory()
    {
        // Arrange
        var import = CreateSampleImportHistory();
        _repository.Add(import);

        // Act
        var deleted = _repository.Delete(import.Id);
        var retrieved = _repository.GetById(import.Id);

        // Assert
        Assert.True(deleted);
        Assert.Null(retrieved);
    }

    [Fact]
    public void Delete_WithNonExistentId_ShouldReturnFalse()
    {
        // Act
        var result = _repository.Delete("non-existent-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DeleteOlderThan_ShouldRemoveOldImports()
    {
        // Arrange
        var cutoffDate = DateTime.UtcNow.AddDays(-30);
        var oldImport = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-60));
        var recentImport = CreateSampleImportHistory(importedAt: DateTime.UtcNow.AddDays(-10));
        
        _repository.Add(oldImport);
        _repository.Add(recentImport);

        // Act
        var deletedCount = _repository.DeleteOlderThan(cutoffDate);
        var remaining = _repository.GetAll().ToList();

        // Assert
        Assert.Equal(1, deletedCount);
        Assert.Single(remaining);
        Assert.Equal(recentImport.Id, remaining[0].Id);
    }

    [Fact]
    public void Count_ShouldReturnTotalNumberOfImports()
    {
        // Arrange
        _repository.Add(CreateSampleImportHistory());
        _repository.Add(CreateSampleImportHistory());
        _repository.Add(CreateSampleImportHistory());

        // Act
        var count = _repository.Count();

        // Assert
        Assert.Equal(3, count);
    }

    [Fact]
    public void GetStatistics_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow;
        
        var successfulImport = CreateSampleImportHistory(
            importedAt: DateTime.UtcNow.AddDays(-5),
            isSuccessful: true,
            storesImported: 5,
            productsImported: 100,
            pricesImported: 500);
        
        var failedImport = CreateSampleImportHistory(
            importedAt: DateTime.UtcNow.AddDays(-3),
            isSuccessful: false,
            storesImported: 0,
            productsImported: 0,
            pricesImported: 0);
        
        _repository.Add(successfulImport);
        _repository.Add(failedImport);

        // Act
        var stats = _repository.GetStatistics(startDate, endDate);

        // Assert
        Assert.Equal(2, stats.TotalImports);
        Assert.Equal(1, stats.SuccessfulImports);
        Assert.Equal(1, stats.FailedImports);
        Assert.Equal(5, stats.TotalStoresImported);
        Assert.Equal(100, stats.TotalProductsImported);
        Assert.Equal(500, stats.TotalPricesImported);
    }

    [Fact]
    public void GetStatistics_WithNoImports_ShouldReturnZeroedStatistics()
    {
        // Act
        var stats = _repository.GetStatistics(DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

        // Assert
        Assert.Equal(0, stats.TotalImports);
        Assert.Equal(0, stats.SuccessfulImports);
        Assert.Equal(0, stats.FailedImports);
        Assert.Equal(TimeSpan.Zero, stats.AverageDuration);
    }

    [Fact]
    public void GetMostRecentByPackageId_ShouldReturnMostRecentImportForPackage()
    {
        // Arrange
        var packageId = "test-package-123";
        var olderImport = CreateSampleImportHistory(
            packageId: packageId,
            importedAt: DateTime.UtcNow.AddDays(-5));
        var newerImport = CreateSampleImportHistory(
            packageId: packageId,
            importedAt: DateTime.UtcNow.AddDays(-1));
        var differentPackage = CreateSampleImportHistory(
            packageId: "different-package",
            importedAt: DateTime.UtcNow);
        
        _repository.Add(olderImport);
        _repository.Add(newerImport);
        _repository.Add(differentPackage);

        // Act
        var result = _repository.GetMostRecentByPackageId(packageId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(newerImport.Id, result.Id);
    }

    [Fact]
    public void GetMostRecentByPackageId_WithNonExistentPackage_ShouldReturnNull()
    {
        // Act
        var result = _repository.GetMostRecentByPackageId("non-existent-package");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ImportHistory_FormattedSize_ShouldFormatCorrectly()
    {
        // Arrange & Act & Assert
        var importBytes = new ImportHistory { TotalSizeBytes = 500 };
        Assert.Equal("500 B", importBytes.FormattedSize);

        var importKb = new ImportHistory { TotalSizeBytes = 1536 };
        Assert.Equal("1.50 KB", importKb.FormattedSize);

        var importMb = new ImportHistory { TotalSizeBytes = 2 * 1024 * 1024 };
        Assert.Equal("2.00 MB", importMb.FormattedSize);

        var importGb = new ImportHistory { TotalSizeBytes = 3L * 1024 * 1024 * 1024 };
        Assert.Equal("3.00 GB", importGb.FormattedSize);
    }

    [Fact]
    public void ImportHistory_TotalEntitiesProcessed_ShouldCalculateCorrectly()
    {
        // Arrange
        var import = new ImportHistory
        {
            StoresImported = 5,
            StoresSkipped = 2,
            ProductsImported = 100,
            ProductsSkipped = 10,
            PricesImported = 500,
            PricesSkipped = 50
        };

        // Act
        var total = import.TotalEntitiesProcessed;

        // Assert
        Assert.Equal(667, total); // 5+2+100+10+500+50
    }

    private static ImportHistory CreateSampleImportHistory(
        DateTime? importedAt = null,
        ImportType importType = ImportType.StaticPackage,
        string? sourcePath = null,
        string? packageId = null,
        bool isSuccessful = true,
        int storesImported = 5,
        int productsImported = 100,
        int pricesImported = 500)
    {
        return new ImportHistory
        {
            Id = Guid.NewGuid().ToString(),
            ImportedAt = importedAt ?? DateTime.UtcNow,
            ImportType = importType,
            SourcePath = sourcePath ?? $"C:\\imports\\package_{Guid.NewGuid():N}.zip",
            PackageId = packageId ?? $"package-{Guid.NewGuid():N}",
            IsSuccessful = isSuccessful,
            StoresImported = storesImported,
            StoresSkipped = 1,
            ProductsImported = productsImported,
            ProductsSkipped = 5,
            PricesImported = pricesImported,
            PricesSkipped = 10,
            TotalSizeBytes = 1024 * 1024,
            Duration = TimeSpan.FromSeconds(30),
            DuplicateStrategy = "Update",
            ErrorCount = isSuccessful ? 0 : 3
        };
    }
}
