using AdvGenPriceComparer.Data.LiteDB.Services;

namespace AdvGenPriceComparer;

/// <summary>
/// Example of how to import Coles JSON data into LiteDB
/// </summary>
public class ImportExample
{
    public static void Main(string[] args)
    {
        // Path to the JSON file
        var jsonFilePath = @"C:\Users\advgen10\source\repos\AdvGenPriceComparer\data\coles_17092025.json";

        // Path to the database (can be relative or absolute)
        var databasePath = "GroceryPrices.db";

        // Create database service
        using var dbService = new DatabaseService(databasePath);

        // Create import service
        var importService = new JsonImportService(dbService);

        // Import the data
        Console.WriteLine("Starting import...");
        Console.WriteLine($"Reading from: {jsonFilePath}");
        Console.WriteLine();

        var result = importService.ImportColesJson(
            jsonFilePath,
            storeName: "Coles",
            validDate: new DateTime(2025, 9, 17));

        // Display results
        if (result.Success)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓ Import completed successfully!");
            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine($"Items processed: {result.ItemsProcessed}");
            Console.WriteLine($"Price records created: {result.PriceRecordsCreated}");
            Console.WriteLine($"Message: {result.Message}");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✗ Import failed!");
            Console.ResetColor();
            Console.WriteLine($"Error: {result.ErrorMessage}");
        }

        // Display any errors
        if (result.Errors.Any())
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warnings/Errors ({result.Errors.Count}):");
            Console.ResetColor();
            foreach (var error in result.Errors.Take(10)) // Show first 10 errors
            {
                Console.WriteLine($"  - {error}");
            }
            if (result.Errors.Count > 10)
            {
                Console.WriteLine($"  ... and {result.Errors.Count - 10} more");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Database location: " + Path.GetFullPath(databasePath));
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}
