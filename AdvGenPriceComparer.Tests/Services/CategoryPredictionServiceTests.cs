using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;
using Xunit;

namespace AdvGenPriceComparer.Tests.Services;

/// <summary>
/// Unit tests for CategoryPredictionService
/// Tests service behavior, error handling, and business logic without requiring trained models
/// </summary>
public class CategoryPredictionServiceTests : IDisposable
{
    private readonly string _testModelPath;
    private readonly List<string> _logMessages;
    private readonly List<(string Message, Exception? Exception)> _errorMessages;

    public CategoryPredictionServiceTests()
    {
        _testModelPath = Path.Combine(Path.GetTempPath(), $"test_cat_model_{Guid.NewGuid():N}.zip");
        _logMessages = new List<string>();
        _errorMessages = new List<(string, Exception?)>();
    }

    public void Dispose()
    {
        // Cleanup test files
        if (File.Exists(_testModelPath))
            File.Delete(_testModelPath);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithNonExistentModel_SetsIsModelLoadedFalse()
    {
        // Arrange
        var nonExistentPath = "/nonexistent/path/model.zip";
        
        // Act
        var service = new CategoryPredictionService(
            nonExistentPath,
            logInfo: msg => _logMessages.Add(msg),
            logWarning: msg => _logMessages.Add($"[WARN] {msg}")
        );
        
        // Assert
        Assert.False(service.IsModelLoaded);
        Assert.Contains(_logMessages, m => m.Contains("not found") || m.Contains("WARN"));
    }

    [Fact]
    public void Constructor_WithValidPath_SetsModelPath()
    {
        // Act
        var service = new CategoryPredictionService(_testModelPath);
        
        // Assert
        Assert.Equal(_testModelPath, service.ModelPath);
    }

    [Fact]
    public void Constructor_LogsInfoWhenModelNotFound()
    {
        // Arrange
        var nonExistentPath = "/nonexistent/path/model.zip";
        string? loggedMessage = null;
        
        // Act
        var service = new CategoryPredictionService(
            nonExistentPath,
            logWarning: msg => loggedMessage = msg
        );
        
        // Assert
        Assert.NotNull(loggedMessage);
        Assert.Contains("not found", loggedMessage);
    }

    #endregion

    #region Prediction Without Model Tests

    [Fact]
    public void PredictCategory_WithoutModel_ReturnsUncategorized()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var prediction = service.PredictCategory(item);
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
        Assert.Empty(prediction.CategoryScores);
    }

    [Fact]
    public void PredictCategory_WithoutModel_ReturnsZeroConfidence()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var prediction = service.PredictCategory(item);
        
        // Assert
        Assert.Equal(0f, prediction.Confidence);
    }

    [Fact]
    public void PredictCategoryFromText_WithoutModel_ReturnsUncategorized()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        
        // Act
        var prediction = service.PredictCategoryFromText("Fresh Milk", "Dairy Farmers", "Whole milk");
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
        Assert.Equal(0f, prediction.Confidence);
    }

    [Fact]
    public void PredictCategory_WithNullItem_ReturnsUncategorized()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        Item? nullItem = null;
        
        // Act
        var prediction = service.PredictCategory(nullItem!);
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
    }

    [Fact]
    public void PredictCategory_WithEmptyProductName_HandlesGracefully()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("", "Brand", "Description");
        
        // Act
        var prediction = service.PredictCategory(item);
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
    }

    #endregion

    #region TryAutoCategorize Tests

    [Fact]
    public void TryAutoCategorize_WithoutModel_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var success = service.TryAutoCategorize(item, out var category);
        
        // Assert
        Assert.False(success);
        Assert.Equal(string.Empty, category);
    }

    [Fact]
    public void TryAutoCategorize_WithoutModel_DoesNotSetCategory()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        service.TryAutoCategorize(item, out var category);
        
        // Assert
        Assert.True(string.IsNullOrEmpty(category));
    }

    [Fact]
    public void TryAutoCategorize_WithHighThreshold_WithoutModel_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var success = service.TryAutoCategorize(item, 0.99f, out var category);
        
        // Assert
        Assert.False(success);
    }

    [Fact]
    public void TryAutoCategorize_WithZeroThreshold_WithoutModel_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var success = service.TryAutoCategorize(item, 0f, out var category);
        
        // Assert
        Assert.False(success);
    }

    #endregion

    #region GetTopSuggestions Tests

    [Fact]
    public void GetTopSuggestions_WithoutModel_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var suggestions = service.GetTopSuggestions(item, 3);
        
        // Assert
        Assert.Empty(suggestions);
    }

    [Fact]
    public void GetTopSuggestions_FromText_WithoutModel_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var productData = new ProductData 
        { 
            ProductName = "Milk", 
            Brand = "Brand", 
            Description = "Description" 
        };
        
        // Act
        var suggestions = service.GetTopSuggestions(productData, 3);
        
        // Assert
        Assert.Empty(suggestions);
    }

    [Fact]
    public void GetTopSuggestions_WithTopNZero_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var suggestions = service.GetTopSuggestions(item, 0);
        
        // Assert
        Assert.Empty(suggestions);
    }

    [Fact]
    public void GetTopSuggestions_WithNegativeTopN_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var suggestions = service.GetTopSuggestions(item, -1);
        
        // Assert
        Assert.Empty(suggestions);
    }

    #endregion

    #region Batch Prediction Tests

    [Fact]
    public void PredictCategories_WithoutModel_ReturnsAllItemsWithUncategorized()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>
        {
            CreateItem("Milk", "Brand1", "Description1"),
            CreateItem("Apple", "Brand2", "Description2"),
            CreateItem("Bread", "Brand3", "Description3")
        };
        
        // Act
        var results = service.PredictCategories(items);
        
        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.Equal("Uncategorized", r.Prediction.PredictedCategory));
    }

    [Fact]
    public void PredictCategories_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>();
        
        // Act
        var results = service.PredictCategories(items);
        
        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void PredictCategories_PreservesItemOrder()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>
        {
            CreateItem("First", "Brand", "Description"),
            CreateItem("Second", "Brand", "Description"),
            CreateItem("Third", "Brand", "Description")
        };
        
        // Act
        var results = service.PredictCategories(items);
        
        // Assert
        Assert.Equal("First", results[0].Item.Name);
        Assert.Equal("Second", results[1].Item.Name);
        Assert.Equal("Third", results[2].Item.Name);
    }

    #endregion

    #region AutoCategorizeBatch Tests

    [Fact]
    public void AutoCategorizeBatch_WithoutModel_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>
        {
            CreateItem("Milk", "Brand", "Description"),
            CreateItem("Apple", "Brand", "Description")
        };
        
        // Act
        var results = service.AutoCategorizeBatch(items);
        
        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void AutoCategorizeBatch_WithEmptyList_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>();
        
        // Act
        var results = service.AutoCategorizeBatch(items);
        
        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public void AutoCategorizeBatch_WithHighThreshold_WithoutModel_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var items = new List<Item>
        {
            CreateItem("Milk", "Brand", "Description")
        };
        
        // Act
        var results = service.AutoCategorizeBatch(items, confidenceThreshold: 0.99f);
        
        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region ICategoryPredictionService Interface Tests

    [Fact]
    public void PredictCategory_AsInterface_WithoutModel_ReturnsResultWithUncategorized()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act - Call through interface
        var interfaceService = (AdvGenPriceComparer.Application.Interfaces.ICategoryPredictionService)service;
        var result = interfaceService.PredictCategory(item);
        
        // Assert
        Assert.Equal("Uncategorized", result.PredictedCategory);
        Assert.Equal(0f, result.Confidence);
        Assert.Empty(result.CategoryScores);
    }

    [Fact]
    public void Interface_ReturnsCategoryPredictionResult()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act
        var interfaceService = (AdvGenPriceComparer.Application.Interfaces.ICategoryPredictionService)service;
        var result = interfaceService.PredictCategory(item);
        
        // Assert - Verify it's the correct return type
        Assert.IsType<AdvGenPriceComparer.Application.Interfaces.CategoryPredictionResult>(result);
    }

    #endregion

    #region LoadModel Tests

    [Fact]
    public void LoadModel_WithNonExistentFile_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var nonExistentPath = "/nonexistent/model.zip";
        
        // Act
        var result = service.LoadModel(nonExistentPath);
        
        // Assert
        Assert.False(result);
        Assert.False(service.IsModelLoaded);
    }

    [Fact]
    public void LoadModel_WithNonExistentFile_LogsWarning()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var nonExistentPath = "/nonexistent/model.zip";
        string? loggedWarning = null;
        
        // Act - Create new service that logs warnings
        var serviceWithLogging = new CategoryPredictionService(
            _testModelPath,
            logWarning: msg => loggedWarning = msg
        );
        serviceWithLogging.LoadModel(nonExistentPath);
        
        // Assert
        Assert.NotNull(loggedWarning);
        Assert.Contains("not found", loggedWarning);
    }

    [Fact]
    public void ReloadModel_WithoutOriginalModel_ReturnsFalse()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        
        // Act
        var result = service.ReloadModel();
        
        // Assert
        Assert.False(result);
    }

    #endregion

    #region Default Confidence Threshold Tests

    [Fact]
    public void DefaultConfidenceThreshold_Is70Percent()
    {
        // Assert
        Assert.Equal(0.7f, CategoryPredictionService.DefaultConfidenceThreshold);
    }

    [Fact]
    public void TryAutoCategorize_UsesDefaultThreshold_WhenNotSpecified()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = CreateItem("Milk", "Brand", "Description");
        
        // Act - This overload should use DefaultConfidenceThreshold
        var success = service.TryAutoCategorize(item, out var category);
        
        // Assert - Without a model, it should return false regardless of threshold
        Assert.False(success);
    }

    #endregion

    #region Null Handling Tests

    [Fact]
    public void PredictCategoryFromText_WithNullValues_HandlesGracefully()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        
        // Act
        var prediction = service.PredictCategoryFromText(null!, null!, null!);
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
    }

    [Fact]
    public void PredictCategory_ItemWithNullName_HandlesGracefully()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        var item = new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = null,
            Brand = "Brand",
            Description = "Description",
            DateAdded = DateTime.UtcNow
        };
        
        // Act
        var prediction = service.PredictCategory(item);
        
        // Assert
        Assert.Equal("Uncategorized", prediction.PredictedCategory);
    }

    [Fact]
    public void GetTopSuggestions_WithNullItem_ReturnsEmptyList()
    {
        // Arrange
        var service = CreateServiceWithoutModel();
        Item? nullItem = null;
        
        // Act
        var suggestions = service.GetTopSuggestions(nullItem!, 3);
        
        // Assert
        Assert.Empty(suggestions);
    }

    #endregion

    #region Helper Methods

    private CategoryPredictionService CreateServiceWithoutModel()
    {
        return new CategoryPredictionService(
            "/nonexistent/path/model.zip",
            logInfo: msg => _logMessages.Add(msg),
            logError: (msg, ex) => _errorMessages.Add((msg, ex)),
            logWarning: msg => _logMessages.Add($"[WARN] {msg}")
        );
    }

    private Item CreateItem(string name, string brand, string description)
    {
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Brand = brand,
            Description = description,
            DateAdded = DateTime.UtcNow
        };
    }

    #endregion
}
