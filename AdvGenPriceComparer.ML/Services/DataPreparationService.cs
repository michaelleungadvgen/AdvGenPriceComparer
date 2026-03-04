using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.ML.Models;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace AdvGenPriceComparer.ML.Services;

/// <summary>
/// Service for preparing and exporting training data
/// </summary>
public class DataPreparationService
{
    /// <summary>
    /// Exports categorized items to a CSV file for training
    /// </summary>
    public async Task<DataExportResult> ExportTrainingDataAsync(
        IEnumerable<Item> items,
        string outputCsvPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var categorizedItems = items
                    .Where(i => !string.IsNullOrEmpty(i.Category))
                    .ToList();

                if (categorizedItems.Count == 0)
                {
                    return new DataExportResult
                    {
                        Success = false,
                        Message = "No categorized items found to export",
                        ExportedCount = 0
                    };
                }

                Directory.CreateDirectory(Path.GetDirectoryName(outputCsvPath)!);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using var writer = new StreamWriter(outputCsvPath);
                using var csv = new CsvWriter(writer, config);

                // Write header manually to ensure correct order
                csv.WriteField("ProductName");
                csv.WriteField("Brand");
                csv.WriteField("Description");
                csv.WriteField("Store");
                csv.WriteField("Category");
                csv.NextRecord();

                // Write data
                foreach (var item in categorizedItems)
                {
                    csv.WriteField(item.Name ?? "");
                    csv.WriteField(item.Brand ?? "");
                    csv.WriteField(item.Description ?? "");
                    csv.WriteField(""); // Store field
                    csv.WriteField(item.Category ?? "Uncategorized");
                    csv.NextRecord();
                }

                return new DataExportResult
                {
                    Success = true,
                    Message = $"Exported {categorizedItems.Count} items to {outputCsvPath}",
                    ExportedCount = categorizedItems.Count,
                    FilePath = outputCsvPath
                };
            }
            catch (Exception ex)
            {
                return new DataExportResult
                {
                    Success = false,
                    Message = $"Failed to export training data: {ex.Message}",
                    ExportedCount = 0
                };
            }
        });
    }

    /// <summary>
    /// Imports training data from a CSV file
    /// </summary>
    public async Task<DataImportResult> ImportTrainingDataAsync(string csvPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                if (!File.Exists(csvPath))
                {
                    return new DataImportResult
                    {
                        Success = false,
                        Message = $"CSV file not found: {csvPath}",
                        ImportedCount = 0
                    };
                }

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using var reader = new StreamReader(csvPath);
                using var csv = new CsvReader(reader, config);

                var records = csv.GetRecords<ProductData>().ToList();

                // Validate records
                var validRecords = records
                    .Where(r => !string.IsNullOrEmpty(r.ProductName) && !string.IsNullOrEmpty(r.Category))
                    .ToList();

                return new DataImportResult
                {
                    Success = true,
                    Message = $"Imported {validRecords.Count} valid training records from {csvPath}",
                    ImportedCount = validRecords.Count,
                    Data = validRecords
                };
            }
            catch (Exception ex)
            {
                return new DataImportResult
                {
                    Success = false,
                    Message = $"Failed to import training data: {ex.Message}",
                    ImportedCount = 0
                };
            }
        });
    }

    /// <summary>
    /// Generates sample training data for initial model training
    /// </summary>
    public async Task<DataExportResult> GenerateSampleTrainingDataAsync(string outputCsvPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var sampleData = GenerateSampleData();

                Directory.CreateDirectory(Path.GetDirectoryName(outputCsvPath)!);

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                };

                using var writer = new StreamWriter(outputCsvPath);
                using var csv = new CsvWriter(writer, config);

                // Write header
                csv.WriteField("ProductName");
                csv.WriteField("Brand");
                csv.WriteField("Description");
                csv.WriteField("Store");
                csv.WriteField("Category");
                csv.NextRecord();

                // Write sample data
                foreach (var item in sampleData)
                {
                    csv.WriteField(item.ProductName);
                    csv.WriteField(item.Brand);
                    csv.WriteField(item.Description);
                    csv.WriteField(item.Store);
                    csv.WriteField(item.Category);
                    csv.NextRecord();
                }

                return new DataExportResult
                {
                    Success = true,
                    Message = $"Generated {sampleData.Count} sample training records",
                    ExportedCount = sampleData.Count,
                    FilePath = outputCsvPath
                };
            }
            catch (Exception ex)
            {
                return new DataExportResult
                {
                    Success = false,
                    Message = $"Failed to generate sample data: {ex.Message}",
                    ExportedCount = 0
                };
            }
        });
    }

    /// <summary>
    /// Analyzes the category distribution in training data
    /// </summary>
    public CategoryDistribution AnalyzeCategoryDistribution(IEnumerable<Item> items)
    {
        var categorizedItems = items.Where(i => !string.IsNullOrEmpty(i.Category)).ToList();
        
        var distribution = categorizedItems
            .GroupBy(i => i.Category)
            .Select(g => new CategoryCount
            {
                Category = g.Key ?? "Unknown",
                Count = g.Count(),
                Percentage = categorizedItems.Count > 0 ? (double)g.Count() / categorizedItems.Count * 100 : 0
            })
            .OrderByDescending(c => c.Count)
            .ToList();

        return new CategoryDistribution
        {
            TotalItems = categorizedItems.Count,
            CategoryCounts = distribution,
            CategoriesWithMinItems = distribution.Count(c => c.Count >= ModelTrainingService.MinimumItemsPerCategory),
            IsValidForTraining = categorizedItems.Count >= ModelTrainingService.MinimumTrainingItems
        };
    }

    private List<ProductData> GenerateSampleData()
    {
        return new List<ProductData>
        {
            // Meat & Seafood
            new() { ProductName = "Beef Mince", Brand = "Woolworths", Category = ProductCategories.Meat },
            new() { ProductName = "Chicken Breast", Brand = "Coles", Category = ProductCategories.Meat },
            new() { ProductName = "Pork Chops", Brand = "Woolworths", Category = ProductCategories.Meat },
            new() { ProductName = "Lamb Leg", Brand = "Coles", Category = ProductCategories.Meat },
            new() { ProductName = "Salmon Fillets", Brand = "Tassal", Category = ProductCategories.Meat },
            new() { ProductName = "Prawns", Brand = "Woolworths", Category = ProductCategories.Meat },
            
            // Dairy & Eggs
            new() { ProductName = "Full Cream Milk", Brand = "Dairy Farmers", Category = ProductCategories.Dairy },
            new() { ProductName = "Greek Yogurt", Brand = "Chobani", Category = ProductCategories.Dairy },
            new() { ProductName = "Cheddar Cheese", Brand = "Bega", Category = ProductCategories.Dairy },
            new() { ProductName = "Free Range Eggs", Brand = "Pace Farm", Category = ProductCategories.Dairy },
            new() { ProductName = "Butter", Brand = "Western Star", Category = ProductCategories.Dairy },
            
            // Fruits & Vegetables
            new() { ProductName = "Bananas", Brand = "", Category = ProductCategories.FruitsVegetables },
            new() { ProductName = "Gala Apples", Brand = "", Category = ProductCategories.FruitsVegetables },
            new() { ProductName = "Carrots", Brand = "", Category = ProductCategories.FruitsVegetables },
            new() { ProductName = "Broccoli", Brand = "", Category = ProductCategories.FruitsVegetables },
            new() { ProductName = "Spinach", Brand = "", Category = ProductCategories.FruitsVegetables },
            
            // Pantry
            new() { ProductName = "White Rice", Brand = "Sunrice", Category = ProductCategories.Pantry },
            new() { ProductName = "Olive Oil", Brand = "Cobram Estate", Category = ProductCategories.Pantry },
            new() { ProductName = "Tomato Sauce", Brand = "Heinz", Category = ProductCategories.Pantry },
            new() { ProductName = "Pasta", Brand = "Barilla", Category = ProductCategories.Pantry },
            new() { ProductName = "Flour", Brand = "White Wings", Category = ProductCategories.Pantry },
            
            // Snacks
            new() { ProductName = "Chocolate Bar", Brand = "Cadbury", Category = ProductCategories.Snacks },
            new() { ProductName = "Potato Chips", Brand = "Smiths", Category = ProductCategories.Snacks },
            new() { ProductName = "Muesli Bars", Brand = "Uncle Tobys", Category = ProductCategories.Snacks },
            
            // Beverages
            new() { ProductName = "Orange Juice", Brand = "Daily Juice", Category = ProductCategories.Beverages },
            new() { ProductName = "Sparkling Water", Brand = "San Pellegrino", Category = ProductCategories.Beverages },
            new() { ProductName = "Coffee Beans", Brand = "Vittoria", Category = ProductCategories.Beverages },
            
            // Frozen
            new() { ProductName = "Frozen Peas", Brand = "Birds Eye", Category = ProductCategories.Frozen },
            new() { ProductName = "Ice Cream", Brand = "Connoisseur", Category = ProductCategories.Frozen },
            
            // Household
            new() { ProductName = "Dishwashing Liquid", Brand = "Finish", Category = ProductCategories.Household },
            new() { ProductName = "Laundry Powder", Brand = "Omo", Category = ProductCategories.Household },
            
            // Personal Care
            new() { ProductName = "Shampoo", Brand = "Head & Shoulders", Category = ProductCategories.PersonalCare },
            new() { ProductName = "Toothpaste", Brand = "Colgate", Category = ProductCategories.PersonalCare },
        };
    }
}

/// <summary>
/// Result of a data export operation
/// </summary>
public class DataExportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ExportedCount { get; set; }
    public string FilePath { get; set; } = string.Empty;
}

/// <summary>
/// Result of a data import operation
/// </summary>
public class DataImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int ImportedCount { get; set; }
    public List<ProductData> Data { get; set; } = new();
}

/// <summary>
/// Category distribution analysis result
/// </summary>
public class CategoryDistribution
{
    public int TotalItems { get; set; }
    public List<CategoryCount> CategoryCounts { get; set; } = new();
    public int CategoriesWithMinItems { get; set; }
    public bool IsValidForTraining { get; set; }
}

/// <summary>
/// Count information for a single category
/// </summary>
public class CategoryCount
{
    public string Category { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}
