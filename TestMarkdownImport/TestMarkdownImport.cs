using System;
using System.Collections.Generic;
using System.IO;
using AdvGenPriceComparer.Data.LiteDB.Services;

namespace TestMarkdownImport;

/// <summary>
/// Test CLI for verifying Drakes markdown import functionality
/// Agent-006: Testing markdown import with drakes.md
/// </summary>
class Program
{
    static int Main(string[] args)
    {
        Console.WriteLine("==============================================");
        Console.WriteLine("Drakes Markdown Import Test - Agent-006");
        Console.WriteLine("==============================================\n");

        // Get project root
        var projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", ".."));
        
        // Test files to process
        var testFiles = new[]
        {
            Path.Combine(projectRoot, "drakes_04_02_2026.md"),
            Path.Combine(projectRoot, "drakes.md"),
            Path.Combine(projectRoot, "drakes_12_02_2026.md")
        };

        int totalTests = 0;
        int passedTests = 0;

        // Test 1-3: Markdown Parsing Tests
        foreach (var file in testFiles)
        {
            if (!File.Exists(file))
            {
                Console.WriteLine($"⚠️  File not found: {file}");
                continue;
            }

            Console.WriteLine($"\n📄 Testing file: {Path.GetFileName(file)}");
            Console.WriteLine(new string('-', 50));

            try
            {
                var parser = new DrakesMarkdownParser();
                var result = parser.ParseFile(file);

                totalTests++;

                if (result.Success && result.Products.Count > 0)
                {
                    passedTests++;
                    Console.WriteLine($"✅ PASSED - Parsed {result.Products.Count} products");
                    Console.WriteLine($"   📅 Valid From: {result.ValidFrom:dd/MM/yyyy}");
                    Console.WriteLine($"   📅 Valid To: {result.ValidTo:dd/MM/yyyy}");
                    Console.WriteLine($"   📁 Categories: {result.Categories.Count}");

                    // Show sample products
                    Console.WriteLine("\n   Sample Products:");
                    for (int i = 0; i < Math.Min(3, result.Products.Count); i++)
                    {
                        var p = result.Products[i];
                        Console.WriteLine($"   - {p.ProductName}");
                        Console.WriteLine($"     Price: {p.Price} | Category: {p.Category}");
                    }
                }
                else
                {
                    Console.WriteLine($"❌ FAILED - {result.ErrorMessage ?? "No products found"}");
                }
            }
            catch (Exception ex)
            {
                totalTests++;
                Console.WriteLine($"❌ FAILED - Exception: {ex.Message}");
            }
        }

        // Test 4: Database Import via JsonImportService
        Console.WriteLine("\n\n==============================================");
        Console.WriteLine("Database Import Test via JsonImportService");
        Console.WriteLine("==============================================");

        totalTests++;
        var dbTest = TestDatabaseImport(projectRoot);
        if (dbTest)
        {
            passedTests++;
            Console.WriteLine("✅ PASSED - Database import via JsonImportService successful");
        }
        else
        {
            Console.WriteLine("❌ FAILED - Database import via JsonImportService failed");
        }

        // Summary
        Console.WriteLine("\n\n==============================================");
        Console.WriteLine("Test Summary");
        Console.WriteLine("==============================================");
        Console.WriteLine($"Total Tests: {totalTests}");
        Console.WriteLine($"Passed: {passedTests}");
        Console.WriteLine($"Failed: {totalTests - passedTests}");
        Console.WriteLine($"Success Rate: {(totalTests > 0 ? (passedTests * 100 / totalTests) : 0)}%");

        if (passedTests == totalTests && totalTests > 0)
        {
            Console.WriteLine("\n🎉 All tests passed! Markdown import is working correctly.");
            return 0;
        }
        else
        {
            Console.WriteLine("\n⚠️  Some tests failed. Review the output above.");
            return 1;
        }
    }

    static bool TestDatabaseImport(string projectRoot)
    {
        try
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "AdvGenPriceComparer",
                "TestMarkdownImport.db");

            Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

            // Clean up previous test database
            if (File.Exists(dbPath))
                File.Delete(dbPath);

            var dbService = new DatabaseService(dbPath);
            var importService = new JsonImportService(dbService);

            // Test with the markdown file
            var markdownFile = Path.Combine(projectRoot, "drakes_04_02_2026.md");
            if (!File.Exists(markdownFile))
            {
                Console.WriteLine($"   ⚠️  Test file not found: {markdownFile}");
                return false;
            }

            Console.WriteLine($"   📥 Importing from: {Path.GetFileName(markdownFile)}");
            
            // Use the new ImportFromDrakesMarkdown method
            var result = importService.ImportFromDrakesMarkdown(markdownFile, "Drakes Test Store");

            Console.WriteLine($"   ✅ Result: {result.Message}");
            Console.WriteLine($"   📊 Items: {result.ItemsProcessed}, Price Records: {result.PriceRecordsCreated}");
            
            if (result.Errors.Count > 0)
            {
                Console.WriteLine($"   ⚠️  Errors: {result.Errors.Count}");
                foreach (var error in result.Errors.Take(3))
                {
                    Console.WriteLine($"      - {error}");
                }
            }

            return result.Success && result.ItemsProcessed > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"   ❌ Error: {ex.Message}");
            Console.WriteLine($"   📍 Stack: {ex.StackTrace?.Split('\n').FirstOrDefault()}");
            return false;
        }
    }
}
