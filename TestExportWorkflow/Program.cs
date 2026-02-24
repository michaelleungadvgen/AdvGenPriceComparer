using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Services;
using AdvGenPriceComparer.Data.LiteDB.Repositories;
using AdvGenPriceComparer.WPF.Services;

namespace TestExportWorkflow;

/// <summary>
/// CLI test program for end-to-end export workflow testing
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     AdvGenPriceComparer - Export Workflow Test Suite        ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Setup test database path with unique timestamp to avoid conflicts
        var testRunId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var testDbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "AdvGenPriceComparer", 
            $"TestExportWorkflow_{testRunId}.db");
        var testExportDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "TestExports",
            testRunId);

        // Clean up previous test data
        CleanupTestData(testDbPath, testExportDir);
        Directory.CreateDirectory(Path.GetDirectoryName(testDbPath)!);
        Directory.CreateDirectory(testExportDir);

        Console.WriteLine($"Test Database: {testDbPath}");
        Console.WriteLine($"Test Export Directory: {testExportDir}");
        Console.WriteLine();

        try
        {
            // Initialize services
            var dbService = new DatabaseService(testDbPath);
            var itemRepository = new ItemRepository(dbService);
            var placeRepository = new PlaceRepository(dbService);
            var priceRecordRepository = new PriceRecordRepository(dbService);

            // Create mock logger
            var logger = new TestLogger();

            var exportService = new ExportService(
                itemRepository,
                placeRepository,
                priceRecordRepository,
                logger);

            int passedTests = 0;
            int failedTests = 0;

            // Test 1: Empty Database Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 1: Export Empty Database");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var emptyExportPath = Path.Combine(testExportDir, "test1_empty_export.json");
                var options = new ExportOptions();
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, emptyExportPath, progress);

                if (result.Success && result.ItemsExported == 0 && File.Exists(emptyExportPath))
                {
                    Console.WriteLine("✅ PASSED: Empty database exported successfully");
                    Console.WriteLine($"   File created: {emptyExportPath}");
                    Console.WriteLine($"   File size: {result.FileSizeBytes} bytes");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Empty database export failed");
                    Console.WriteLine($"   Success: {result.Success}, Items: {result.ItemsExported}");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Seed test data
            Console.WriteLine("Seeding test data...");
            var (seededItems, seededPlaces, seededRecords) = SeedTestData(dbService);
            Console.WriteLine($"  ✓ Created {seededItems} items");
            Console.WriteLine($"  ✓ Created {seededPlaces} places");
            Console.WriteLine($"  ✓ Created {seededRecords} price records");
            Console.WriteLine();

            // Test 2: Full Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 2: Full Export (All Data)");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var fullExportPath = Path.Combine(testExportDir, "test2_full_export.json");
                var options = new ExportOptions();
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, fullExportPath, progress);

                if (result.Success && result.ItemsExported > 0 && File.Exists(fullExportPath))
                {
                    Console.WriteLine("✅ PASSED: Full export completed successfully");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    Console.WriteLine($"   File size: {result.FileSizeBytes / 1024.0:F2} KB");
                    Console.WriteLine($"   File path: {result.FilePath}");
                    
                    // Verify JSON structure
                    var jsonContent = await File.ReadAllTextAsync(fullExportPath);
                    if (jsonContent.Contains("\"exportVersion\"") && 
                        jsonContent.Contains("\"items\"") &&
                        jsonContent.Contains("\"statistics\""))
                    {
                        Console.WriteLine("✅ PASSED: Export JSON structure is valid");
                        passedTests++;
                    }
                    else
                    {
                        Console.WriteLine("❌ FAILED: Export JSON structure is invalid");
                        failedTests++;
                    }
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Full export failed");
                    Console.WriteLine($"   Success: {result.Success}, Items: {result.ItemsExported}");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 3: Category Filter Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 3: Export with Category Filter (Beverages)");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var categoryExportPath = Path.Combine(testExportDir, "test3_category_export.json");
                var options = new ExportOptions
                {
                    Category = "Beverages"
                };
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, categoryExportPath, progress);

                if (result.Success && File.Exists(categoryExportPath))
                {
                    Console.WriteLine("✅ PASSED: Category filter export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Category filter export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 4: Date Range Filter Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 4: Export with Date Range Filter");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var dateExportPath = Path.Combine(testExportDir, "test4_date_export.json");
                var options = new ExportOptions
                {
                    ValidFrom = DateTime.Now.AddDays(-7),
                    ValidTo = DateTime.Now.AddDays(7)
                };
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, dateExportPath, progress);

                if (result.Success && File.Exists(dateExportPath))
                {
                    Console.WriteLine("✅ PASSED: Date range filter export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Date range filter export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 5: Price Range Filter Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 5: Export with Price Range Filter ($2 - $10)");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var priceExportPath = Path.Combine(testExportDir, "test5_price_export.json");
                var options = new ExportOptions
                {
                    MinPrice = 2.0m,
                    MaxPrice = 10.0m
                };
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, priceExportPath, progress);

                if (result.Success && File.Exists(priceExportPath))
                {
                    Console.WriteLine("✅ PASSED: Price range filter export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Price range filter export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 6: Sale Items Only Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 6: Export Sale Items Only");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var saleExportPath = Path.Combine(testExportDir, "test6_sale_export.json");
                var options = new ExportOptions
                {
                    OnlyOnSale = true
                };
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, saleExportPath, progress);

                if (result.Success && File.Exists(saleExportPath))
                {
                    Console.WriteLine("✅ PASSED: Sale items export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Sale items export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 7: Compressed Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 7: Compressed Export (.json.gz)");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var compressedExportPath = Path.Combine(testExportDir, "test7_compressed_export.json.gz");
                var options = new ExportOptions();
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonGzAsync(options, compressedExportPath, progress);

                if (result.Success && File.Exists(compressedExportPath) && result.CompressionRatio < 1.0)
                {
                    Console.WriteLine("✅ PASSED: Compressed export completed successfully");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    Console.WriteLine($"   File size: {result.FileSizeBytes / 1024.0:F2} KB");
                    Console.WriteLine($"   Compression ratio: {result.CompressionRatio:P2}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Compressed export failed");
                    Console.WriteLine($"   Success: {result.Success}, File exists: {File.Exists(compressedExportPath)}");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 8: Incremental Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 8: Incremental Export (Last 24 hours)");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var incrementalExportPath = Path.Combine(testExportDir, "test8_incremental_export.json");
                var lastExportDate = DateTime.Now.AddHours(-24);
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.IncrementalExportAsync(lastExportDate, incrementalExportPath, progress);

                if (result.Success && File.Exists(incrementalExportPath))
                {
                    Console.WriteLine("✅ PASSED: Incremental export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Incremental export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 9: Combined Filters Export
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 9: Export with Combined Filters");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var combinedExportPath = Path.Combine(testExportDir, "test9_combined_export.json");
                var options = new ExportOptions
                {
                    Category = "Snacks",
                    MinPrice = 1.0m,
                    MaxPrice = 20.0m,
                    OnlyOnSale = true
                };
                var progress = new Progress<ExportProgress>(p => 
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}"));

                var result = await exportService.ExportToJsonAsync(options, combinedExportPath, progress);

                if (result.Success && File.Exists(combinedExportPath))
                {
                    Console.WriteLine("✅ PASSED: Combined filters export completed");
                    Console.WriteLine($"   Items exported: {result.ItemsExported}");
                    passedTests++;
                }
                else
                {
                    Console.WriteLine("❌ FAILED: Combined filters export failed");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Test 10: Progress Reporting
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            Console.WriteLine("TEST 10: Progress Reporting Verification");
            Console.WriteLine("────────────────────────────────────────────────────────────────");
            try
            {
                var progressReportPath = Path.Combine(testExportDir, "test10_progress.json");
                var options = new ExportOptions();
                var progressReports = new List<ExportProgress>();
                var progress = new Progress<ExportProgress>(p => 
                {
                    progressReports.Add(p);
                    Console.WriteLine($"  Progress: {p.Percentage}% - {p.Status}");
                });

                var result = await exportService.ExportToJsonAsync(options, progressReportPath, progress);

                // Wait a bit for progress events
                await Task.Delay(100);

                if (result.Success && progressReports.Count > 0)
                {
                    var firstProgress = progressReports.First();
                    var lastProgress = progressReports.Last();
                    
                    if (firstProgress.Percentage == 0 && lastProgress.Percentage == 100)
                    {
                        Console.WriteLine("✅ PASSED: Progress reporting works correctly");
                        Console.WriteLine($"   Total progress updates: {progressReports.Count}");
                        passedTests++;
                    }
                    else
                    {
                        Console.WriteLine("❌ FAILED: Progress reporting incomplete");
                        failedTests++;
                    }
                }
                else
                {
                    Console.WriteLine("❌ FAILED: No progress reports received");
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ FAILED: Exception - {ex.Message}");
                failedTests++;
            }
            Console.WriteLine();

            // Summary
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                     TEST SUMMARY                             ║");
            Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
            Console.WriteLine($"║  Total Tests: 10                                             ║");
            Console.WriteLine($"║  ✅ Passed:   {passedTests}                                            ║");
            Console.WriteLine($"║  ❌ Failed:   {failedTests}                                            ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine();

            if (failedTests == 0)
            {
                Console.WriteLine("🎉 ALL TESTS PASSED! Export workflow is working correctly.");
            }
            else
            {
                Console.WriteLine("⚠️  SOME TESTS FAILED. Please review the output above.");
                Environment.ExitCode = 1;
            }

            Console.WriteLine();
            Console.WriteLine($"Test export files location: {testExportDir}");
            Console.WriteLine();
            Console.WriteLine("Test completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║              CRITICAL ERROR DURING TESTING                   ║");
            Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            Environment.ExitCode = 2;
        }
    }

    /// <summary>
    /// Seeds test data into the database
    /// </summary>
    static (int items, int places, int records) SeedTestData(DatabaseService dbService)
    {
        var itemRepo = new ItemRepository(dbService);
        var placeRepo = new PlaceRepository(dbService);
        var priceRepo = new PriceRecordRepository(dbService);

        int itemsCreated = 0;
        int placesCreated = 0;
        int recordsCreated = 0;

        // Create test places (stores)
        var stores = new[]
        {
            new Place { Name = "Coles Brisbane City", Chain = "Coles", Suburb = "Brisbane", State = "QLD", Postcode = "4000" },
            new Place { Name = "Woolworths Fortitude Valley", Chain = "Woolworths", Suburb = "Fortitude Valley", State = "QLD", Postcode = "4006" },
            new Place { Name = "Drakes Annerley", Chain = "Drakes", Suburb = "Annerley", State = "QLD", Postcode = "4103" },
            new Place { Name = "Aldi Moorooka", Chain = "ALDI", Suburb = "Moorooka", State = "QLD", Postcode = "4105" }
        };

        foreach (var store in stores)
        {
            placeRepo.Add(store);
            placesCreated++;
        }

        // Create test items with various categories
        var items = new[]
        {
            // Beverages
            new Item { Name = "Coca-Cola Soft Drink", Brand = "Coca-Cola", Category = "Beverages", PackageSize = "2L", Unit = "bottle" },
            new Item { Name = "Pepsi Max", Brand = "Pepsi", Category = "Beverages", PackageSize = "2L", Unit = "bottle" },
            new Item { Name = "Orange Juice", Brand = "Daily Juice", Category = "Beverages", PackageSize = "2L", Unit = "bottle" },
            new Item { Name = "Spring Water", Brand = "Mount Franklin", Category = "Beverages", PackageSize = "600ml", Unit = "bottle" },
            
            // Snacks
            new Item { Name = "Potato Chips Original", Brand = "Smith's", Category = "Snacks", PackageSize = "170g", Unit = "bag" },
            new Item { Name = "Tim Tam Original", Brand = "Arnott's", Category = "Snacks", PackageSize = "200g", Unit = "pack" },
            new Item { Name = "Chocolate Block", Brand = "Cadbury", Category = "Snacks", PackageSize = "180g", Unit = "block" },
            
            // Dairy
            new Item { Name = "Full Cream Milk", Brand = "Dairy Farmers", Category = "Dairy", PackageSize = "2L", Unit = "bottle" },
            new Item { Name = "Greek Yogurt", Brand = "Chobani", Category = "Dairy", PackageSize = "500g", Unit = "tub" },
            
            // Meat
            new Item { Name = "Beef Mince", Brand = "Coles", Category = "Meat", PackageSize = "500g", Unit = "pack" },
            new Item { Name = "Chicken Breast", Brand = "Ingham's", Category = "Meat", PackageSize = "1kg", Unit = "pack" }
        };

        foreach (var item in items)
        {
            itemRepo.Add(item);
            itemsCreated++;
        }

        // Create price records for items
        var random = new Random(42); // Fixed seed for reproducibility
        var allItems = itemRepo.GetAll().ToList();
        var allPlaces = placeRepo.GetAll().ToList();

        foreach (var item in allItems)
        {
            // Create 1-3 price records per item at different stores
            int recordCount = random.Next(1, 4);
            for (int i = 0; i < recordCount; i++)
            {
                var place = allPlaces[random.Next(allPlaces.Count)];
                var basePrice = random.Next(2, 20);
                var isOnSale = random.Next(0, 3) == 0; // 33% chance of being on sale
                
                var priceRecord = new PriceRecord
                {
                    ItemId = item.Id!,
                    PlaceId = place.Id!,
                    Price = basePrice + random.Next(0, 99) / 100m,
                    OriginalPrice = isOnSale ? basePrice + 2 + random.Next(0, 50) / 100m : null,
                    IsOnSale = isOnSale,
                    SaleDescription = isOnSale ? "Half Price" : null,
                    ValidFrom = DateTime.Now.AddDays(-random.Next(1, 7)),
                    ValidTo = DateTime.Now.AddDays(random.Next(1, 14)),
                    DateRecorded = DateTime.Now.AddDays(-random.Next(0, 3)),
                    Source = place.Chain,
                    Notes = $"Test price record for {item.Name}"
                };

                priceRepo.Add(priceRecord);
                recordsCreated++;
            }
        }

        return (itemsCreated, placesCreated, recordsCreated);
    }

    /// <summary>
    /// Cleans up test data from previous runs
    /// </summary>
    static void CleanupTestData(string dbPath, string exportDir)
    {
        try
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }

            if (Directory.Exists(exportDir))
            {
                foreach (var file in Directory.GetFiles(exportDir))
                {
                    File.Delete(file);
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

/// <summary>
/// Simple test logger implementation
/// </summary>
public class TestLogger : AdvGenPriceComparer.WPF.Services.ILoggerService
{
    public void LogDebug(string message)
    {
        Console.WriteLine($"  [DEBUG] {message}");
    }

    public void LogInfo(string message)
    {
        Console.WriteLine($"  [INFO] {message}");
    }

    public void LogWarning(string message)
    {
        Console.WriteLine($"  [WARN] {message}");
    }

    public void LogError(string message, Exception? ex = null)
    {
        Console.WriteLine($"  [ERROR] {message}");
        if (ex != null)
        {
            Console.WriteLine($"    Exception: {ex.Message}");
        }
    }

    public void LogCritical(string message, Exception? ex = null)
    {
        Console.WriteLine($"  [CRITICAL] {message}");
        if (ex != null)
        {
            Console.WriteLine($"    Exception: {ex.Message}");
        }
    }

    public string GetLogFilePath()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AdvGenPriceComparer",
            "Logs",
            "test_export_workflow.log");
    }
}
