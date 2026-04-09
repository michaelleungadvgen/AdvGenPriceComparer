using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.Data.LiteDB.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Repositories;

public class ExportHistoryRepositoryTests : IDisposable
{
    private readonly string _testDbPath;
    private readonly DatabaseService _databaseService;
    private readonly ExportHistoryRepository _repository;

    public ExportHistoryRepositoryTests()
    {
        _testDbPath = Path.Combine(Path.GetTempPath(), $"test_export_history_{Guid.NewGuid()}.db");
        _databaseService = new DatabaseService(_testDbPath);
        _repository = new ExportHistoryRepository(_databaseService);
    }

    public void Dispose()
    {
        _databaseService?.Dispose();
        if (File.Exists(_testDbPath))
        {
            try
            {
                File.Delete(_testDbPath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Fact]
    public void Add_ValidExportHistory_AddsToRepository()
    {
        // Arrange
        var exportHistory = CreateTestExportHistory();

        // Act
        _repository.Add(exportHistory);
        var result = _repository.GetById(exportHistory.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exportHistory.Id, result.Id);
        Assert.Equal(exportHistory.PackageId, result.PackageId);
    }

    [Fact]
    public void GetById_ExistingId_ReturnsExportHistory()
    {
        // Arrange
        var exportHistory = CreateTestExportHistory();
        _repository.Add(exportHistory);

        // Act
        var result = _repository.GetById(exportHistory.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(exportHistory.Id, result.Id);
        Assert.Equal(10, result.StoresExported);
        Assert.Equal(100, result.ProductsExported);
        Assert.Equal(500, result.PricesExported);
    }

    [Fact]
    public void GetById_NonExistingId_ReturnsNull()
    {
        // Act
        var result = _repository.GetById("non-existing-id");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetAll_ReturnsAllExportHistoriesOrderedByDate()
    {
        // Arrange
        var history1 = CreateTestExportHistory(id: "1", exportedAt: DateTime.UtcNow.AddDays(-1));
        var history2 = CreateTestExportHistory(id: "2", exportedAt: DateTime.UtcNow);
        var history3 = CreateTestExportHistory(id: "3", exportedAt: DateTime.UtcNow.AddDays(-2));
        
        _repository.Add(history1);
        _repository.Add(history2);
        _repository.Add(history3);

        // Act
        var results = _repository.GetAll().ToList();

        // Assert
        Assert.Equal(3, results.Count);
        Assert.Equal("2", results[0].Id); // Most recent first
        Assert.Equal("1", results[1].Id);
        Assert.Equal("3", results[2].Id);
    }

    [Fact]
    public void GetByDateRange_ReturnsExportsInRange()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var history1 = CreateTestExportHistory(id: "1", exportedAt: now.AddDays(-5));
        var history2 = CreateTestExportHistory(id: "2", exportedAt: now.AddDays(-3));
        var history3 = CreateTestExportHistory(id: "3", exportedAt: now.AddDays(-1));
        
        _repository.Add(history1);
        _repository.Add(history2);
        _repository.Add(history3);

        // Act
        var results = _repository.GetByDateRange(now.AddDays(-4), now).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, h => h.Id == "2");
        Assert.Contains(results, h => h.Id == "3");
    }

    [Fact]
    public void GetRecent_ReturnsSpecifiedCount()
    {
        // Arrange
        for (int i = 0; i < 10; i++)
        {
            _repository.Add(CreateTestExportHistory(id: $"{i}", exportedAt: DateTime.UtcNow.AddMinutes(-i)));
        }

        // Act
        var results = _repository.GetRecent(5).ToList();

        // Assert
        Assert.Equal(5, results.Count);
        Assert.Equal("0", results[0].Id); // Most recent
    }

    [Fact]
    public void GetByType_ReturnsMatchingType()
    {
        // Arrange
        var history1 = CreateTestExportHistory(id: "1", exportType: ExportType.StaticPackage);
        var history2 = CreateTestExportHistory(id: "2", exportType: ExportType.Json);
        var history3 = CreateTestExportHistory(id: "3", exportType: ExportType.StaticPackage);
        
        _repository.Add(history1);
        _repository.Add(history2);
        _repository.Add(history3);

        // Act
        var results = _repository.GetByType(ExportType.StaticPackage).ToList();

        // Assert
        Assert.Equal(2, results.Count);
        Assert.All(results, h => Assert.Equal(ExportType.StaticPackage, h.ExportType));
    }

    [Fact]
    public void Delete_ExistingId_RemovesAndReturnsTrue()
    {
        // Arrange
        var exportHistory = CreateTestExportHistory();
        _repository.Add(exportHistory);

        // Act
        var result = _repository.Delete(exportHistory.Id);
        var deleted = _repository.GetById(exportHistory.Id);

        // Assert
        Assert.True(result);
        Assert.Null(deleted);
    }

    [Fact]
    public void Delete_NonExistingId_ReturnsFalse()
    {
        // Act
        var result = _repository.Delete("non-existing-id");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void DeleteOlderThan_RemovesOldExports()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _repository.Add(CreateTestExportHistory(id: "1", exportedAt: now.AddDays(-10)));
        _repository.Add(CreateTestExportHistory(id: "2", exportedAt: now.AddDays(-5)));
        _repository.Add(CreateTestExportHistory(id: "3", exportedAt: now.AddDays(-1)));

        // Act
        var deletedCount = _repository.DeleteOlderThan(now.AddDays(-3));
        var remaining = _repository.GetAll().ToList();

        // Assert
        Assert.Equal(2, deletedCount);
        Assert.Single(remaining);
        Assert.Equal("3", remaining[0].Id);
    }

    [Fact]
    public void Count_ReturnsTotalNumber()
    {
        // Arrange
        for (int i = 0; i < 5; i++)
        {
            _repository.Add(CreateTestExportHistory(id: $"{i}"));
        }

        // Act
        var count = _repository.Count();

        // Assert
        Assert.Equal(5, count);
    }

    [Fact]
    public void GetStatistics_WithExports_ReturnsCorrectStats()
    {
        // Arrange
        var now = DateTime.UtcNow;
        _repository.Add(new ExportHistory
        {
            Id = "1",
            ExportedAt = now.AddDays(-1),
            IsSuccessful = true,
            StoresExported = 10,
            ProductsExported = 100,
            PricesExported = 500,
            TotalSizeBytes = 1024 * 1024,
            Duration = TimeSpan.FromSeconds(30)
        });
        _repository.Add(new ExportHistory
        {
            Id = "2",
            ExportedAt = now.AddDays(-1),
            IsSuccessful = true,
            StoresExported = 5,
            ProductsExported = 50,
            PricesExported = 250,
            TotalSizeBytes = 512 * 1024,
            Duration = TimeSpan.FromSeconds(15)
        });
        _repository.Add(new ExportHistory
        {
            Id = "3",
            ExportedAt = now.AddDays(-1),
            IsSuccessful = false,
            Duration = TimeSpan.FromSeconds(5)
        });

        // Act
        var stats = _repository.GetStatistics(now.AddDays(-2), now);

        // Assert
        Assert.Equal(3, stats.TotalExports);
        Assert.Equal(2, stats.SuccessfulExports);
        Assert.Equal(1, stats.FailedExports);
        Assert.Equal(15, stats.TotalStoresExported);
        Assert.Equal(150, stats.TotalProductsExported);
        Assert.Equal(750, stats.TotalPricesExported);
        Assert.Equal((1024 + 512) * 1024, stats.TotalSizeBytes);
        Assert.Equal(TimeSpan.FromSeconds(22.5), stats.AverageDuration);
    }

    [Fact]
    public void GetStatistics_NoExports_ReturnsEmptyStats()
    {
        // Act
        var stats = _repository.GetStatistics(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

        // Assert
        Assert.Equal(0, stats.TotalExports);
        Assert.Equal(0, stats.TotalSizeBytes);
        Assert.Equal(TimeSpan.Zero, stats.AverageDuration);
    }

    [Fact]
    public void ExportHistory_FormattedSize_ReturnsCorrectFormat()
    {
        // Arrange & Act & Assert
        Assert.Equal("100 B", new ExportHistory { TotalSizeBytes = 100 }.FormattedSize);
        Assert.Equal("1.00 KB", new ExportHistory { TotalSizeBytes = 1024 }.FormattedSize);
        Assert.Equal("1.50 KB", new ExportHistory { TotalSizeBytes = 1536 }.FormattedSize);
        Assert.Equal("1.00 MB", new ExportHistory { TotalSizeBytes = 1024 * 1024 }.FormattedSize);
        Assert.Equal("1.50 MB", new ExportHistory { TotalSizeBytes = (long)(1.5 * 1024 * 1024) }.FormattedSize);
        Assert.Equal("1.00 GB", new ExportHistory { TotalSizeBytes = 1024L * 1024 * 1024 }.FormattedSize);
    }

    private static ExportHistory CreateTestExportHistory(
        string id = "test-id",
        DateTime? exportedAt = null,
        ExportType exportType = ExportType.StaticPackage)
    {
        return new ExportHistory
        {
            Id = id,
            ExportedAt = exportedAt ?? DateTime.UtcNow,
            ExportType = exportType,
            StoresExported = 10,
            ProductsExported = 100,
            PricesExported = 500,
            TotalSizeBytes = 1024 * 1024,
            OutputPath = "/test/output",
            PackageId = "package-123",
            Description = "Test export",
            IsSuccessful = true,
            Duration = TimeSpan.FromSeconds(30)
        };
    }
}
