using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using Microsoft.ML;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for predicting product categories using ML.NET
/// </summary>
public class CategoryPredictionService
{
    private readonly MLContext _mlContext;
    private ITransformer? _model;
    private PredictionEngine<ProductData, CategoryPrediction>? _predictionEngine;
    private readonly Action<string>? _logInfo;
    private readonly Action<string, Exception>? _logError;
    private readonly Action<string>? _logWarning;
    private readonly string _modelPath;

    /// <summary>
    /// Default confidence threshold for auto-categorization (0.7 = 70%)
    /// </summary>
    public const float DefaultConfidenceThreshold = 0.7f;

    /// <summary>
    /// Creates a new instance of CategoryPredictionService
    /// </summary>
    public CategoryPredictionService(
        string modelPath,
        Action<string>? logInfo = null,
        Action<string, Exception>? logError = null,
        Action<string>? logWarning = null)
    {
        _mlContext = new MLContext();
        _modelPath = modelPath;
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;

        // Try to load existing model
        if (File.Exists(modelPath))
        {
            LoadModel(modelPath);
        }
        else
        {
            _logWarning?.Invoke($"ML model not found at {modelPath}. Predictions will return 'Uncategorized' until a model is trained.");
        }
    }

    /// <summary>
    /// Gets whether a valid model is loaded and ready for predictions
    /// </summary>
    public bool IsModelLoaded => _predictionEngine != null;

    /// <summary>
    /// Gets the path to the current model
    /// </summary>
    public string ModelPath => _modelPath;

    /// <summary>
    /// Loads a model from the specified path
    /// </summary>
    public bool LoadModel(string modelPath)
    {
        try
        {
            if (!File.Exists(modelPath))
            {
                _logWarning?.Invoke($"Model file not found: {modelPath}");
                return false;
            }

            using var stream = File.OpenRead(modelPath);
            _model = _mlContext.Model.Load(stream, out var _);
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<ProductData, CategoryPrediction>(_model);
            
            _logInfo?.Invoke($"ML model loaded successfully from {modelPath}");
            return true;
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to load ML model from {modelPath}", ex);
            _predictionEngine = null;
            _model = null;
            return false;
        }
    }

    /// <summary>
    /// Reloads the model from the original model path
    /// </summary>
    public bool ReloadModel()
    {
        return LoadModel(_modelPath);
    }

    /// <summary>
    /// Predicts the category for a single product
    /// </summary>
    public CategoryPrediction PredictCategory(Item item)
    {
        if (_predictionEngine == null)
        {
            return new CategoryPrediction
            {
                PredictedCategory = "Uncategorized",
                CategoryScores = new Dictionary<string, float>()
            };
        }

        var productData = new ProductData
        {
            ProductName = item.Name ?? "",
            Brand = item.Brand ?? "",
            Description = item.Description ?? "",
            Store = ""
        };

        return PredictCategory(productData);
    }

    /// <summary>
    /// Predicts the category from text fields (name, brand, description)
    /// </summary>
    public CategoryPrediction PredictCategoryFromText(string name, string brand, string description)
    {
        var productData = new ProductData
        {
            ProductName = name ?? "",
            Brand = brand ?? "",
            Description = description ?? "",
            Store = ""
        };

        return PredictCategory(productData);
    }

    /// <summary>
    /// Predicts the category for product data
    /// </summary>
    public CategoryPrediction PredictCategory(ProductData productData)
    {
        if (_predictionEngine == null)
        {
            return new CategoryPrediction
            {
                PredictedCategory = "Uncategorized",
                CategoryScores = new Dictionary<string, float>()
            };
        }

        try
        {
            var prediction = _predictionEngine.Predict(productData);

            // Build category scores dictionary
            prediction.CategoryScores = new Dictionary<string, float>();
            var categories = ProductCategories.AllCategories;
            
            for (int i = 0; i < prediction.Score.Length && i < categories.Length; i++)
            {
                prediction.CategoryScores[categories[i]] = prediction.Score[i];
            }

            _logInfo?.Invoke($"Predicted category for '{productData.ProductName}': {prediction.PredictedCategory} (confidence: {prediction.Confidence:P2})");

            return prediction;
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to predict category for '{productData.ProductName}'", ex);
            return new CategoryPrediction
            {
                PredictedCategory = "Uncategorized",
                CategoryScores = new Dictionary<string, float>()
            };
        }
    }

    /// <summary>
    /// Predicts categories for multiple products (batch prediction)
    /// </summary>
    public List<(Item Item, CategoryPrediction Prediction)> PredictCategories(List<Item> items)
    {
        var results = new List<(Item, CategoryPrediction)>();

        foreach (var item in items)
        {
            var prediction = PredictCategory(item);
            results.Add((item, prediction));
        }

        return results;
    }

    /// <summary>
    /// Tries to auto-categorize an item if confidence is high enough
    /// </summary>
    public bool TryAutoCategorize(Item item, float confidenceThreshold, out string category)
    {
        var prediction = PredictCategory(item);

        if (prediction.Confidence >= confidenceThreshold && 
            !string.IsNullOrEmpty(prediction.PredictedCategory) &&
            prediction.PredictedCategory != "Uncategorized")
        {
            category = prediction.PredictedCategory;
            return true;
        }

        category = string.Empty;
        return false;
    }

    /// <summary>
    /// Tries to auto-categorize an item using the default confidence threshold
    /// </summary>
    public bool TryAutoCategorize(Item item, out string category)
    {
        return TryAutoCategorize(item, DefaultConfidenceThreshold, out category);
    }

    /// <summary>
    /// Gets top N category suggestions for an item
    /// </summary>
    public List<(string Category, float Confidence)> GetTopSuggestions(Item item, int topN = 3)
    {
        var prediction = PredictCategory(item);

        return prediction.CategoryScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }

    /// <summary>
    /// Gets top N category suggestions for product data
    /// </summary>
    public List<(string Category, float Confidence)> GetTopSuggestions(ProductData productData, int topN = 3)
    {
        var prediction = PredictCategory(productData);

        return prediction.CategoryScores
            .OrderByDescending(kvp => kvp.Value)
            .Take(topN)
            .Select(kvp => (kvp.Key, kvp.Value))
            .ToList();
    }

    /// <summary>
    /// Auto-categorizes a list of items, returning only those that meet the confidence threshold
    /// </summary>
    public List<(Item Item, string Category, float Confidence)> AutoCategorizeBatch(
        List<Item> items, 
        float confidenceThreshold = DefaultConfidenceThreshold)
    {
        var results = new List<(Item, string, float)>();

        foreach (var item in items)
        {
            var prediction = PredictCategory(item);
            
            if (prediction.Confidence >= confidenceThreshold &&
                !string.IsNullOrEmpty(prediction.PredictedCategory) &&
                prediction.PredictedCategory != "Uncategorized")
            {
                results.Add((item, prediction.PredictedCategory, prediction.Confidence));
            }
        }

        return results;
    }
}
