using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Core.Interfaces;

namespace AdvGenPriceComparer.Desktop.WinUI.Services;

public class JsonImportService
{
    private readonly IGroceryDataService _groceryDataService;

    public JsonImportService(IGroceryDataService groceryDataService)
    {
        _groceryDataService = groceryDataService;
    }

    public async Task<StorageFile?> SelectJsonFileAsync(Window window)
    {
        var picker = new FileOpenPicker();
        
        // Get the window handle for WinUI 3
        var hWnd = WindowNative.GetWindowHandle(window);
        InitializeWithWindow.Initialize(picker, hWnd);
        
        picker.ViewMode = PickerViewMode.List;
        picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        picker.FileTypeFilter.Add(".json");
        
        return await picker.PickSingleFileAsync();
    }

    public async Task<ImportResult> ImportJsonDataAsync(StorageFile file)
    {
        try
        {
            var json = await FileIO.ReadTextAsync(file);
            
            // Try to parse as different JSON structures
            var importResult = await TryImportAsItemArray(json);
            if (!importResult.Success)
            {
                importResult = await TryImportAsWrappedData(json);
            }
            
            return importResult;
        }
        catch (Exception ex)
        {
            return new ImportResult 
            { 
                Success = false, 
                ErrorMessage = $"Failed to read file: {ex.Message}" 
            };
        }
    }

    private Task<ImportResult> TryImportAsItemArray(string json)
    {
        try
        {
            var jsonItems = JsonSerializer.Deserialize<List<JsonItem>>(json);
            if (jsonItems == null || jsonItems.Count == 0)
            {
                return Task.FromResult(new ImportResult 
                { 
                    Success = false, 
                    ErrorMessage = "No items found in JSON file" 
                });
            }

            var importedItems = new List<Item>();
            var importedPlaces = new List<Place>();

            foreach (var jsonItem in jsonItems)
            {
                var item = ConvertToItem(jsonItem);
                _groceryDataService.Items.Add(item);
                importedItems.Add(item);
            }

            return Task.FromResult(new ImportResult
            {
                Success = true,
                ImportedItemsCount = importedItems.Count,
                ImportedPlacesCount = 0,
                Message = $"Successfully imported {importedItems.Count} items"
            });
        }
        catch (JsonException ex)
        {
            return Task.FromResult(new ImportResult 
            { 
                Success = false, 
                ErrorMessage = $"JSON parsing error: {ex.Message}" 
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new ImportResult 
            { 
                Success = false, 
                ErrorMessage = $"Import error: {ex.Message}" 
            });
        }
    }

    private async Task<ImportResult> TryImportAsWrappedData(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Look for common wrapper properties
            JsonElement itemsElement;
            if (root.TryGetProperty("items", out itemsElement) ||
                root.TryGetProperty("products", out itemsElement) ||
                root.TryGetProperty("data", out itemsElement))
            {
                var itemsJson = itemsElement.GetRawText();
                return await TryImportAsItemArray(itemsJson);
            }

            return new ImportResult 
            { 
                Success = false, 
                ErrorMessage = "Unsupported JSON structure. Expected array of items or object with 'items'/'products'/'data' property." 
            };
        }
        catch (Exception ex)
        {
            return new ImportResult 
            { 
                Success = false, 
                ErrorMessage = $"JSON structure analysis error: {ex.Message}" 
            };
        }
    }

    private Item ConvertToItem(JsonItem jsonItem)
    {
        var item = new Item
        {
            Name = jsonItem.ProductName ?? jsonItem.Name ?? "Unknown Product",
            Description = jsonItem.Description,
            Brand = jsonItem.Brand,
            Category = jsonItem.Category,
            Barcode = jsonItem.ProductID,
            PackageSize = ExtractPackageSize(jsonItem.ProductName ?? jsonItem.Name ?? ""),
            DateAdded = DateTime.UtcNow,
            LastUpdated = DateTime.UtcNow
        };

        // Parse price if available
        if (!string.IsNullOrEmpty(jsonItem.Price))
        {
            var priceStr = jsonItem.Price.Replace("$", "").Replace(",", "");
            if (decimal.TryParse(priceStr, out decimal price))
            {
                item.ExtraInformation["ImportedPrice"] = price.ToString();
            }
        }

        // Store original price and savings if available
        if (!string.IsNullOrEmpty(jsonItem.OriginalPrice))
        {
            item.ExtraInformation["ImportedOriginalPrice"] = jsonItem.OriginalPrice;
        }
        
        if (!string.IsNullOrEmpty(jsonItem.Savings))
        {
            item.ExtraInformation["ImportedSavings"] = jsonItem.Savings;
        }

        return item;
    }

    private string ExtractPackageSize(string productName)
    {
        // Extract common package size patterns (e.g., "500g", "2L", "6x250mL")
        var sizePatterns = new[]
        {
            @"\d+(?:\.\d+)?(?:kg|g|l|ml|oz|lb)\b",
            @"\d+x\d+(?:kg|g|l|ml|oz|lb)\b",
            @"\d+(?:\.\d+)?\s*(?:pack|pk)\b"
        };

        foreach (var pattern in sizePatterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                productName, 
                pattern, 
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (match.Success)
            {
                return match.Value;
            }
        }

        return string.Empty;
    }
}

public class JsonItem
{
    public string? ProductID { get; set; }
    public string? ProductName { get; set; }
    public string? Name { get; set; }
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string? Price { get; set; }
    public string? OriginalPrice { get; set; }
    public string? Savings { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? Message { get; set; }
    public int ImportedItemsCount { get; set; }
    public int ImportedPlacesCount { get; set; }
}