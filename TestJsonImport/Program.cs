using System;
using System.IO;
using System.Linq;
using AdvGenPriceComparer.Data.LiteDB.Services;

namespace TestJsonImport;

class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("========================================");
        Console.WriteLine("  AdvGenPriceComparer JSON Import Test");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Get the repository root directory
        var repoRoot = FindRepositoryRoot();
        if (string.IsNullOrEmpty(repoRoot))
        {
            Console.WriteLine("ERROR: Could not find repository root.");
            return 1;
        }

        Console.WriteLine($"Repository root: {repoRoot}");
        Console.WriteLine();

        // Test file paths
        var colesFile = Path.Combine(repoRoot, "data", "coles_28012026.json");
        var woolworthsFile = Path.Combine(repoRoot, "data", "woolworths_28012026.json");
        var testDbPath = Path.Combine(Path.GetTempPath(), $"test_import_{DateTime.Now:yyyyMMdd_HHmmss}.db");

        int totalErrors = 0;

        try
        {
            // Test 1: Preview Import (Coles)
            Console.WriteLine("TEST 1: Preview Import - Coles");
            Console.WriteLine("--------------------------------");
            totalErrors += TestPreviewImport(colesFile, "Coles");
            Console.WriteLine();

            // Test 2: Preview Import (Woolworths)
            Console.WriteLine("TEST 2: Preview Import - Woolworths");
            Console.WriteLine("-----------------------------------");
            totalErrors += TestPreviewImport(woolworthsFile, "Woolworths");
            Console.WriteLine();

            // Test 3: Full Import with Database
            Console.WriteLine("TEST 3: Full Import with Database");
            Console.WriteLine("---------------------------------");
            totalErrors += TestFullImport(colesFile, testDbPath);
            Console.WriteLine();

            // Test 4: Verify Data Integrity
            Console.WriteLine("TEST 4: Verify Data Integrity");
            Console.WriteLine("-----------------------------");
            totalErrors += TestDataIntegrity(testDbPath);
            Console.WriteLine();

            // Cleanup
            CleanupTestDatabase(testDbPath);

            // Summary
            Console.WriteLine("========================================");
            Console.WriteLine("  TEST SUMMARY");
            Console.WriteLine("========================================");
            if (totalErrors == 0)
            {
                Console.WriteLine("ALL TESTS PASSED!");
                Console.WriteLine();
                Console.WriteLine("The JsonImportService is working correctly:");
                Console.WriteLine("  - Coles JSON format: PARSED OK");
                Console.WriteLine("  - Woolworths JSON format: PARSED OK");
                Console.WriteLine("  - Database import: WORKING OK");
                Console.WriteLine("  - Data integrity: VERIFIED OK");
                return 0;
            }
            else
            {
                Console.WriteLine($"{totalErrors} TEST(S) FAILED");
                return 1;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"FATAL ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            CleanupTestDatabase(testDbPath);
            return 1;
        }
    }

    static string FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        // Try to find the repository root by looking for the .git folder or solution file
        while (!string.IsNullOrEmpty(currentDir))
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                File.Exists(Path.Combine(currentDir, "AdvGenPriceComparer.sln")))
            {
                return currentDir;
            }
            
            var parent = Directory.GetParent(currentDir);
            currentDir = parent?.FullName;
        }
        
        // Fallback to expected location
        var fallback = @"C:\Users\advgen10\source\repos\AdvGenPriceComparer";
        if (Directory.Exists(fallback))
            return fallback;
            
        return string.Empty;
    }

    static int TestPreviewImport(string filePath, string storeName)
    {
        int errors = 0;
        
        try
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"  [FAIL] File not found: {filePath}");
                return 1;
            }

            // Create a temporary database service just for preview
            var tempDbPath = Path.Combine(Path.GetTempPath(), $"preview_test_{Guid.NewGuid()}.db");
            
            using (var dbService = new DatabaseService(tempDbPath))
            {
                var importService = new JsonImportService(dbService);

                // Test preview import
                var previewTask = importService.PreviewImportAsync(filePath);
                previewTask.Wait();
                var products = previewTask.Result;

                if (products == null || products.Count == 0)
                {
                    Console.WriteLine($"  [FAIL] No products parsed from {storeName} JSON");
                    return 1;
                }

                Console.WriteLine($"  [PASS] Successfully parsed {products.Count} products from {storeName}");
                
                // Verify a sample product has required fields
                var sample = products.First();
                if (string.IsNullOrEmpty(sample.ProductID))
                {
                    Console.WriteLine($"  [FAIL] Sample product missing ProductID");
                    errors++;
                }
                if (string.IsNullOrEmpty(sample.ProductName))
                {
                    Console.WriteLine($"  [FAIL] Sample product missing ProductName");
                    errors++;
                }
                if (string.IsNullOrEmpty(sample.Price))
                {
                    Console.WriteLine($"  [FAIL] Sample product missing Price");
                    errors++;
                }

                if (errors == 0)
                {
                    Console.WriteLine($"  [PASS] Sample product validated: {sample.ProductName}");
                    Console.WriteLine($"         Price: {sample.Price}, Savings: {sample.Savings}");
                }
            }
            
            // Cleanup after disposing
            try { File.Delete(tempDbPath); } catch { }

            return errors;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] Error during preview: {ex.Message}");
            return 1;
        }
    }

    static int TestFullImport(string filePath, string dbPath)
    {
        int errors = 0;
        
        try
        {
            using (var dbService = new DatabaseService(dbPath))
            {
                var importService = new JsonImportService(dbService);

                // Test full import from file
                var result = importService.ImportFromFile(filePath, DateTime.Now);

                if (!result.Success)
                {
                    Console.WriteLine($"  [FAIL] Import failed: {result.ErrorMessage}");
                    return 1;
                }

                Console.WriteLine($"  [PASS] Import completed successfully");
                Console.WriteLine($"         Items processed: {result.ItemsProcessed}");
                Console.WriteLine($"         Price records created: {result.PriceRecordsCreated}");
                Console.WriteLine($"         Errors: {result.Errors.Count}");

                if (result.Errors.Count > 0)
                {
                    foreach (var error in result.Errors.Take(3))
                    {
                        Console.WriteLine($"         - {error}");
                    }
                    if (result.Errors.Count > 3)
                    {
                        Console.WriteLine($"         ... and {result.Errors.Count - 3} more");
                    }
                }

                return result.Errors.Count > 0 ? 1 : 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] Error during full import: {ex.Message}");
            return 1;
        }
    }

    static int TestDataIntegrity(string dbPath)
    {
        int errors = 0;
        
        try
        {
            using (var dbService = new DatabaseService(dbPath))
            {
                var items = dbService.Items.Query().ToList();
                var places = dbService.Places.Query().ToList();
                var priceRecords = dbService.PriceRecords.Query().ToList();

                Console.WriteLine($"  [INFO] Database contains:");
                Console.WriteLine($"         - {items.Count} items");
                Console.WriteLine($"         - {places.Count} stores");
                Console.WriteLine($"         - {priceRecords.Count} price records");

                if (items.Count == 0)
                {
                    Console.WriteLine($"  [FAIL] No items in database after import");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"  [PASS] Items imported: {items.Count}");
                }

                if (places.Count == 0)
                {
                    Console.WriteLine($"  [FAIL] No stores in database after import");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"  [PASS] Stores created: {places.Count}");
                }

                if (priceRecords.Count == 0)
                {
                    Console.WriteLine($"  [FAIL] No price records in database after import");
                    errors++;
                }
                else
                {
                    Console.WriteLine($"  [PASS] Price records created: {priceRecords.Count}");
                }

                // Verify price parsing
                var sampleRecord = priceRecords.FirstOrDefault();
                if (sampleRecord != null)
                {
                    Console.WriteLine($"  [PASS] Sample price record: ${sampleRecord.Price:F2}");
                    if (sampleRecord.OriginalPrice.HasValue)
                    {
                        Console.WriteLine($"         Original: ${sampleRecord.OriginalPrice.Value:F2}");
                    }
                }

                return errors;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [FAIL] Error during data integrity check: {ex.Message}");
            return 1;
        }
    }

    static void CleanupTestDatabase(string dbPath)
    {
        try
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Console.WriteLine($"  [INFO] Cleaned up test database");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  [WARN] Could not clean up test database: {ex.Message}");
        }
    }
}
