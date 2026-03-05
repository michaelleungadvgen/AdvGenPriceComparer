using AdvGenPriceComparer.ML.Models;
using AdvGenPriceComparer.ML.Services;

namespace TestMLModelTraining;

/// <summary>
/// CLI tool for training and testing the initial ML categorization model
/// </summary>
class Program
{
    private const string SampleDataPath = @"..\AdvGenPriceComparer.ML\Data\sample_training_data.csv";
    private const string OutputModelPath = @"..\AdvGenPriceComparer.ML\MLModels\category_model.zip";

    static async Task<int> Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  ML Model Training Test Tool");
        Console.WriteLine("  AdvGenPriceComparer");
        Console.WriteLine("========================================");
        Console.WriteLine();

        try
        {
            // Ensure directories exist
            var modelDir = Path.GetDirectoryName(OutputModelPath);
            if (!string.IsNullOrEmpty(modelDir) && !Directory.Exists(modelDir))
            {
                Directory.CreateDirectory(modelDir);
                Console.WriteLine($"Created directory: {modelDir}");
            }

            // Check if sample data exists
            if (!File.Exists(SampleDataPath))
            {
                Console.WriteLine($"ERROR: Sample data file not found: {SampleDataPath}");
                return 1;
            }

            Console.WriteLine($"Sample data path: {Path.GetFullPath(SampleDataPath)}");
            Console.WriteLine($"Output model path: {Path.GetFullPath(OutputModelPath)}");
            Console.WriteLine();

            // Step 1: Load and analyze training data
            Console.WriteLine("Step 1: Loading and analyzing training data...");
            var dataPrepService = new DataPreparationService();
            var importResult = await dataPrepService.ImportTrainingDataAsync(SampleDataPath);

            if (!importResult.Success)
            {
                Console.WriteLine($"ERROR: Failed to import training data: {importResult.Message}");
                return 1;
            }

            Console.WriteLine($"  Imported {importResult.ImportedCount} training records");
            Console.WriteLine();

            // Analyze category distribution
            var categoryGroups = importResult.Data
                .GroupBy(d => d.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .ToList();

            Console.WriteLine("  Category Distribution:");
            foreach (var cat in categoryGroups)
            {
                Console.WriteLine($"    - {cat.Category}: {cat.Count} items");
            }
            Console.WriteLine();

            // Step 2: Train the model
            Console.WriteLine("Step 2: Training ML model...");
            Console.WriteLine($"  Minimum required items: {ModelTrainingService.MinimumTrainingItems}");
            Console.WriteLine($"  Minimum items per category: {ModelTrainingService.MinimumItemsPerCategory}");
            Console.WriteLine();

            var trainingService = new ModelTrainingService(
                logInfo: msg => Console.WriteLine($"    [INFO] {msg}"),
                logError: (msg, ex) => Console.WriteLine($"    [ERROR] {msg}: {ex?.Message}"),
                logWarning: msg => Console.WriteLine($"    [WARN] {msg}")
            );

            var trainingResult = trainingService.TrainModelFromCsv(SampleDataPath, OutputModelPath);

            Console.WriteLine();
            Console.WriteLine("  Training Result:");
            Console.WriteLine($"    Success: {trainingResult.Success}");
            Console.WriteLine($"    Message: {trainingResult.Message}");
            Console.WriteLine($"    Training Items: {trainingResult.TrainingItemCount}");
            Console.WriteLine($"    Macro Accuracy: {trainingResult.Accuracy:P2}");
            Console.WriteLine($"    Micro Accuracy: {trainingResult.MicroAccuracy:P2}");
            Console.WriteLine($"    Duration: {trainingResult.Duration.TotalSeconds:F2} seconds");
            Console.WriteLine($"    Model Path: {trainingResult.ModelPath}");
            Console.WriteLine();

            if (!trainingResult.Success)
            {
                Console.WriteLine("ERROR: Model training failed!");
                return 1;
            }

            // Step 3: Verify model file exists
            Console.WriteLine("Step 3: Verifying model file...");
            if (!File.Exists(OutputModelPath))
            {
                Console.WriteLine($"ERROR: Model file was not created: {OutputModelPath}");
                return 1;
            }

            var modelFileInfo = new FileInfo(OutputModelPath);
            Console.WriteLine($"  Model file created successfully!");
            Console.WriteLine($"    Size: {modelFileInfo.Length:N0} bytes ({modelFileInfo.Length / 1024.0:F2} KB)");
            Console.WriteLine($"    Created: {modelFileInfo.LastWriteTime}");
            Console.WriteLine();

            // Step 4: Validate model can be loaded
            Console.WriteLine("Step 4: Validating model can be loaded...");
            bool isValid = trainingService.ValidateModel(OutputModelPath);
            Console.WriteLine($"  Model validation: {(isValid ? "PASSED" : "FAILED")}");
            Console.WriteLine();

            if (!isValid)
            {
                Console.WriteLine("ERROR: Model validation failed!");
                return 1;
            }

            // Step 5: Test predictions
            Console.WriteLine("Step 5: Testing model predictions...");
            var predictionService = new CategoryPredictionService(
                OutputModelPath,
                logInfo: msg => Console.WriteLine($"    [INFO] {msg}"),
                logWarning: msg => Console.WriteLine($"    [WARN] {msg}")
            );

            // Test items for prediction
            var testItems = new[]
            {
                new TestItem { Name = "Fresh Salmon Fillet", Brand = "Tassal", Description = "Atlantic salmon" },
                new TestItem { Name = "Whole Milk 2L", Brand = "Dairy Farmers", Description = "Full cream milk" },
                new TestItem { Name = "Red Apples 1kg", Brand = "", Description = "Fresh apples" },
                new TestItem { Name = "White Bread", Brand = "Wonder White", Description = "Sandwich bread" },
                new TestItem { Name = "Dark Chocolate", Brand = "Cadbury", Description = "Dairy milk chocolate" },
                new TestItem { Name = "Orange Juice 2L", Brand = "Daily Juice", Description = "100% juice" },
                new TestItem { Name = "Vanilla Ice Cream", Brand = "Connoisseur", Description = "Gourmet ice cream" },
                new TestItem { Name = "Dishwashing Liquid", Brand = "Finish", Description = "Lemon scent" },
                new TestItem { Name = "Shampoo", Brand = "Head & Shoulders", Description = "Anti-dandruff" },
                new TestItem { Name = "Baby Nappies", Brand = "Huggies", Description = "Ultra dry" },
                new TestItem { Name = "Dog Food", Brand = "Pedigree", Description = "Adult dog food" },
                new TestItem { Name = "Vitamin C", Brand = "Blackmores", Description = "Immune support" }
            };

            Console.WriteLine("  Prediction Tests:");
            int correctPredictions = 0;
            foreach (var testItem in testItems)
            {
                var prediction = predictionService.PredictCategoryFromText(
                    testItem.Name,
                    testItem.Brand,
                    testItem.Description
                );

                var expectedCategory = GetExpectedCategory(testItem.Name);
                bool isCorrect = prediction.PredictedCategory == expectedCategory;
                if (isCorrect) correctPredictions++;

                var status = isCorrect ? "✓" : "○";
                Console.WriteLine($"    {status} '{testItem.Name}' => {prediction.PredictedCategory} (confidence: {prediction.Confidence:P0})");
                if (!isCorrect && !string.IsNullOrEmpty(expectedCategory))
                {
                    Console.WriteLine($"      (Expected: {expectedCategory})");
                }
            }

            double accuracy = (double)correctPredictions / testItems.Length;
            Console.WriteLine();
            Console.WriteLine($"  Prediction Accuracy on test items: {accuracy:P0} ({correctPredictions}/{testItems.Length})");
            Console.WriteLine();

            // Step 6: Get model info
            Console.WriteLine("Step 6: Model Information");
            var modelInfo = trainingService.GetModelInfo(OutputModelPath);
            if (modelInfo != null)
            {
                Console.WriteLine($"    Path: {modelInfo.Path}");
                Console.WriteLine($"    Last Trained: {modelInfo.LastTrained}");
                Console.WriteLine($"    File Size: {modelInfo.FileSizeBytes:N0} bytes");
                Console.WriteLine($"    Is Valid: {modelInfo.IsValid}");
            }
            Console.WriteLine();

            // Summary
            Console.WriteLine("========================================");
            Console.WriteLine("  Training Complete!");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine($"Model saved to: {Path.GetFullPath(OutputModelPath)}");
            Console.WriteLine();
            Console.WriteLine("The model is ready to use for auto-categorization!");
            Console.WriteLine();

            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return 1;
        }
    }

    private static string? GetExpectedCategory(string productName)
    {
        return productName.ToLower() switch
        {
            var s when s.Contains("salmon") => ProductCategories.Meat,
            var s when s.Contains("milk") => ProductCategories.Dairy,
            var s when s.Contains("apples") => ProductCategories.FruitsVegetables,
            var s when s.Contains("bread") => ProductCategories.Bakery,
            var s when s.Contains("chocolate") => ProductCategories.Snacks,
            var s when s.Contains("juice") => ProductCategories.Beverages,
            var s when s.Contains("ice cream") => ProductCategories.Frozen,
            var s when s.Contains("dishwashing") => ProductCategories.Household,
            var s when s.Contains("shampoo") => ProductCategories.PersonalCare,
            var s when s.Contains("nappies") || s.Contains("baby") => ProductCategories.BabyProducts,
            var s when s.Contains("dog food") => ProductCategories.PetCare,
            var s when s.Contains("vitamin") => ProductCategories.Health,
            _ => null
        };
    }
}

/// <summary>
/// Test item for prediction validation
/// </summary>
public class TestItem
{
    public string Name { get; set; } = "";
    public string Brand { get; set; } = "";
    public string Description { get; set; } = "";
}
