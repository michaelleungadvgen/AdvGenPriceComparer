using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for training and managing ML.NET category prediction models
/// </summary>
public class ModelTrainingService
{
    private readonly MLContext _mlContext;
    private readonly Action<string>? _logInfo;
    private readonly Action<string, Exception>? _logError;
    private readonly Action<string>? _logWarning;

    /// <summary>
    /// Minimum number of categorized items required for training
    /// </summary>
    public const int MinimumTrainingItems = 100;

    /// <summary>
    /// Minimum items per category for effective training
    /// </summary>
    public const int MinimumItemsPerCategory = 10;

    /// <summary>
    /// Creates a new instance of ModelTrainingService
    /// </summary>
    public ModelTrainingService(
        Action<string>? logInfo = null,
        Action<string, Exception>? logError = null,
        Action<string>? logWarning = null)
    {
        _mlContext = new MLContext(seed: 0);
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;
    }

    /// <summary>
    /// Trains a category prediction model from items in the database
    /// </summary>
    public async Task<TrainingResult> TrainModelFromDatabaseAsync(
        IEnumerable<Item> items,
        string outputModelPath)
    {
        var startTime = DateTime.Now;
        _logInfo?.Invoke("Starting model training from database");

        // Filter items with categories
        var categorizedItems = items
            .Where(i => !string.IsNullOrEmpty(i.Category))
            .ToList();

        if (categorizedItems.Count < MinimumTrainingItems)
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Insufficient training data. Need at least {MinimumTrainingItems} categorized items, found {categorizedItems.Count}"
            };
        }

        // Check category distribution
        var categoryDistribution = categorizedItems
            .GroupBy(i => i.Category)
            .Select(g => new { Category = g.Key, Count = g.Count() })
            .ToList();

        var underRepresentedCategories = categoryDistribution
            .Where(c => c.Count < MinimumItemsPerCategory)
            .Select(c => c.Category)
            .ToList();

        if (underRepresentedCategories.Any())
        {
            _logWarning?.Invoke($"Categories with fewer than {MinimumItemsPerCategory} items: {string.Join(", ", underRepresentedCategories)}");
        }

        // Convert to training data
        var trainingData = categorizedItems.Select(item => new ProductData
        {
            ProductName = item.Name ?? "",
            Brand = item.Brand ?? "",
            Description = item.Description ?? "",
            Store = "", // Store info can be added via ExtraInformation if needed
            Category = item.Category ?? "Uncategorized"
        }).ToList();

        // Create and train model
        var result = await TrainModelInternalAsync(trainingData, outputModelPath);
        result.TrainingItemCount = categorizedItems.Count;
        result.Duration = DateTime.Now - startTime;

        return result;
    }

    /// <summary>
    /// Trains a category prediction model from a CSV file
    /// </summary>
    public TrainingResult TrainModelFromCsv(string csvPath, string outputModelPath)
    {
        var startTime = DateTime.Now;
        _logInfo?.Invoke($"Training model from CSV: {csvPath}");

        if (!File.Exists(csvPath))
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"CSV file not found: {csvPath}"
            };
        }

        // Load data from CSV
        IDataView dataView = _mlContext.Data.LoadFromTextFile<ProductData>(
            csvPath,
            hasHeader: true,
            separatorChar: ',');

        // Get row count
        var rowCount = _mlContext.Data
            .CreateEnumerable<ProductData>(dataView, reuseRowObject: false)
            .Count();

        if (rowCount < MinimumTrainingItems)
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Insufficient training data. Need at least {MinimumTrainingItems} rows, found {rowCount}"
            };
        }

        // Split data into train/test sets (80/20)
        var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

        // Build and train pipeline
        var pipeline = BuildTrainingPipeline();
        var model = pipeline.Fit(split.TrainSet);

        // Evaluate model
        var predictions = model.Transform(split.TestSet);
        var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

        // Save model
        Directory.CreateDirectory(Path.GetDirectoryName(outputModelPath)!);
        _mlContext.Model.Save(model, dataView.Schema, outputModelPath);

        _logInfo?.Invoke($"Model trained - Macro Accuracy: {metrics.MacroAccuracy:P2}, Micro Accuracy: {metrics.MicroAccuracy:P2}");

        return new TrainingResult
        {
            Success = true,
            Message = "Model trained successfully from CSV",
            Accuracy = metrics.MacroAccuracy,
            MicroAccuracy = metrics.MicroAccuracy,
            TrainingItemCount = rowCount,
            ModelPath = outputModelPath,
            Duration = DateTime.Now - startTime
        };
    }

    /// <summary>
    /// Retrains an existing model with additional training data
    /// </summary>
    public async Task<TrainingResult> RetrainModelAsync(
        string existingModelPath,
        List<ProductData> newTrainingData,
        string outputModelPath)
    {
        var startTime = DateTime.Now;
        _logInfo?.Invoke($"Retraining model with {newTrainingData.Count} new samples");

        if (!File.Exists(existingModelPath))
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Existing model not found: {existingModelPath}"
            };
        }

        if (newTrainingData.Count < MinimumItemsPerCategory)
        {
            return new TrainingResult
            {
                Success = false,
                Message = $"Insufficient new data. Need at least {MinimumItemsPerCategory} new samples, found {newTrainingData.Count}"
            };
        }

        // Load new data
        var newDataView = _mlContext.Data.LoadFromEnumerable(newTrainingData);

        // Build and train pipeline on new data
        var pipeline = BuildTrainingPipeline();
        var retrainedModel = pipeline.Fit(newDataView);

        // Save retrained model
        Directory.CreateDirectory(Path.GetDirectoryName(outputModelPath)!);
        _mlContext.Model.Save(retrainedModel, newDataView.Schema, outputModelPath);

        _logInfo?.Invoke($"Model retrained successfully with {newTrainingData.Count} new samples");

        return new TrainingResult
        {
            Success = true,
            Message = $"Model retrained with {newTrainingData.Count} new samples",
            TrainingItemCount = newTrainingData.Count,
            ModelPath = outputModelPath,
            Duration = DateTime.Now - startTime
        };
    }

    /// <summary>
    /// Validates that a model file exists and can be loaded
    /// </summary>
    public bool ValidateModel(string modelPath)
    {
        try
        {
            if (!File.Exists(modelPath))
                return false;

            using var stream = File.OpenRead(modelPath);
            var model = _mlContext.Model.Load(stream, out var _);
            return model != null;
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Model validation failed for {modelPath}", ex);
            return false;
        }
    }

    /// <summary>
    /// Gets information about a trained model
    /// </summary>
    public ModelInfo? GetModelInfo(string modelPath)
    {
        try
        {
            if (!File.Exists(modelPath))
                return null;

            var fileInfo = new FileInfo(modelPath);
            
            return new ModelInfo
            {
                Path = modelPath,
                LastTrained = fileInfo.LastWriteTime,
                FileSizeBytes = fileInfo.Length,
                IsValid = ValidateModel(modelPath)
            };
        }
        catch (Exception ex)
        {
            _logError?.Invoke($"Failed to get model info for {modelPath}", ex);
            return null;
        }
    }

    private async Task<TrainingResult> TrainModelInternalAsync(
        List<ProductData> trainingData,
        string outputModelPath)
    {
        return await Task.Run(() =>
        {
            // Create data view
            IDataView dataView = _mlContext.Data.LoadFromEnumerable(trainingData);

            // Split into train/test sets (80/20)
            var split = _mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

            // Build training pipeline
            var pipeline = BuildTrainingPipeline();

            // Train model
            _logInfo?.Invoke("Training model...");
            var model = pipeline.Fit(split.TrainSet);

            // Evaluate model
            var predictions = model.Transform(split.TestSet);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions);

            _logInfo?.Invoke($"Model trained - Macro Accuracy: {metrics.MacroAccuracy:P2}, Micro Accuracy: {metrics.MicroAccuracy:P2}");

            // Save model
            Directory.CreateDirectory(Path.GetDirectoryName(outputModelPath)!);
            _mlContext.Model.Save(model, dataView.Schema, outputModelPath);

            return new TrainingResult
            {
                Success = true,
                Message = "Model trained successfully from database",
                Accuracy = metrics.MacroAccuracy,
                MicroAccuracy = metrics.MicroAccuracy,
                ModelPath = outputModelPath
            };
        });
    }

    private IEstimator<ITransformer> BuildTrainingPipeline()
    {
        // Build a text featurization pipeline for product categorization
        // Note: Store field is excluded from the pipeline as it's often empty and can cause schema issues
        return _mlContext.Transforms.Conversion
            .MapValueToKey("Label", "Label")
            .Append(_mlContext.Transforms.Text.FeaturizeText("ProductNameFeatures", "ProductName"))
            .Append(_mlContext.Transforms.Text.FeaturizeText("BrandFeatures", "Brand"))
            .Append(_mlContext.Transforms.Text.FeaturizeText("DescriptionFeatures", "Description"))
            .Append(_mlContext.Transforms.Concatenate("Features",
                "ProductNameFeatures", "BrandFeatures", "DescriptionFeatures"))
            .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy(
                new SdcaMaximumEntropyMulticlassTrainer.Options
                {
                    LabelColumnName = "Label",
                    FeatureColumnName = "Features",
                    MaximumNumberOfIterations = 100
                }))
            .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));
    }
}

/// <summary>
/// Information about a trained ML model
/// </summary>
public class ModelInfo
{
    /// <summary>
    /// Path to the model file
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// When the model was last trained
    /// </summary>
    public DateTime LastTrained { get; set; }

    /// <summary>
    /// Size of the model file in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Whether the model is valid and can be loaded
    /// </summary>
    public bool IsValid { get; set; }
}
