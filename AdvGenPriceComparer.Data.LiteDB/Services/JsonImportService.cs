using System.Text.Json;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Repositories;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

// Note: ImportPreviewItem is defined in WPF.Models, not here to avoid duplication
// The service uses ColesProduct directly for data transfer

/// <summary>
/// Service for importing grocery data from JSON files into LiteDB
/// </summary>
public class JsonImportService
{
    private readonly DatabaseService _dbService;
    private readonly ItemRepository _itemRepository;
    private readonly PlaceRepository _placeRepository;
    private readonly PriceRecordRepository _priceRecordRepository;

    public JsonImportService(DatabaseService dbService)
    {
        _dbService = dbService;
        _itemRepository = new ItemRepository(dbService);
        _placeRepository = new PlaceRepository(dbService);
        _priceRecordRepository = new PriceRecordRepository(dbService);
    }

    /// <summary>
    /// Preview import from JSON file without saving to database
    /// Returns ColesProduct list that can be converted to preview items by the ViewModel
    /// </summary>
    public async Task<List<ColesProduct>> PreviewImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var jsonContent = File.ReadAllText(filePath);
                var colesData = JsonSerializer.Deserialize<List<ColesProduct>>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return colesData ?? new List<ColesProduct>();
            }
            catch (Exception)
            {
                // Return empty list on error - let the caller handle errors
                return new List<ColesProduct>();
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Import Coles products with specified store and date
    /// </summary>
    public ImportResult ImportColesProducts(List<ColesProduct> products, string storeId, DateTime catalogueDate, 
        Dictionary<string, string>? existingItemMappings = null, IProgress<ImportProgress>? progress = null)
    {
        var result = new ImportResult();
        existingItemMappings ??= new Dictionary<string, string>();
        
        // Get the store
        var store = _placeRepository.GetById(storeId);
        if (store == null)
        {
            result.Success = false;
            result.ErrorMessage = "Store not found";
            return result;
        }

        int totalItems = products.Count;
        int processedCount = 0;

        foreach (var product in products)
        {
            try
            {
                string itemId;
                bool addPriceRecordOnly = existingItemMappings.TryGetValue(product.ProductID, out var existingId) 
                    && !string.IsNullOrEmpty(existingId);

                if (addPriceRecordOnly)
                {
                    // Add price record to existing item
                    itemId = existingId;
                }
                else
                {
                    // Create new item
                    var item = CreateOrUpdateItem(product, store.Chain ?? "Unknown");
                    if (item != null)
                    {
                        itemId = item.Id!;
                        result.ItemsProcessed++;
                    }
                    else
                    {
                        result.Errors.Add($"Failed to create item for {product.ProductName}");
                        continue;
                    }
                }

                // Create price record
                var priceRecord = CreatePriceRecord(product, itemId, storeId, catalogueDate);
                _priceRecordRepository.Add(priceRecord);
                result.PriceRecordsCreated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error processing {product.ProductName}: {ex.Message}");
            }

            processedCount++;
            progress?.Report(new ImportProgress
            {
                TotalItems = totalItems,
                ProcessedItems = processedCount,
                CurrentItem = product.ProductName
            });
        }

        result.Success = result.Errors.Count == 0;
        result.Message = $"Successfully imported {result.ItemsProcessed} new items and {result.PriceRecordsCreated} price records";
        
        return result;
    }

    /// <summary>
    /// Import data from JSON file path
    /// </summary>
    public ImportResult ImportFromFile(string filePath, DateTime? validDate = null)
    {
        try
        {
            var jsonContent = File.ReadAllText(filePath);
            var fileName = Path.GetFileName(filePath);

            // Try to detect the format (Coles or Woolworths)
            if (jsonContent.Contains("\"ProductID\"") || jsonContent.Contains("\"productID\"") ||
                jsonContent.Contains("\"productName\"") || jsonContent.Contains("\"ProductName\""))
            {
                return ImportColesJsonContent(jsonContent, fileName, validDate);
            }
            else
            {
                return new ImportResult
                {
                    Success = false,
                    ErrorMessage = "Unsupported JSON format. Expected Coles or Woolworths format."
                };
            }
        }
        catch (Exception ex)
        {
            return new ImportResult
            {
                Success = false,
                ErrorMessage = $"Error reading file: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Import Coles products from JSON content
    /// </summary>
    private ImportResult ImportColesJsonContent(string jsonContent, string fileName, DateTime? validDate = null)
    {
        var result = new ImportResult();
        var importDate = validDate ?? DateTime.UtcNow;

        try
        {
            var colesData = JsonSerializer.Deserialize<List<ColesProduct>>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (colesData == null || colesData.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No products found in JSON file";
                return result;
            }

            // Extract store name from filename if possible
            var storeName = ExtractStoreNameFromFileName(fileName);
            var chain = DetermineChain(fileName, jsonContent);

            // Get or create store location
            var store = GetOrCreateStore(storeName, chain);

            // Process each product
            foreach (var product in colesData)
            {
                try
                {
                    // Create or update item
                    var item = CreateOrUpdateItem(product, chain);
                    result.ItemsProcessed++;

                    // Create price record
                    if (item != null)
                    {
                        var priceRecord = CreatePriceRecord(product, item.Id!, store.Id!, importDate);
                        _priceRecordRepository.Add(priceRecord);
                        result.PriceRecordsCreated++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing product {product.ProductID}: {ex.Message}");
                }
            }

            result.Success = true;
            result.Message = $"Successfully imported {result.ItemsProcessed} items and {result.PriceRecordsCreated} price records from {chain}";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Import failed: {ex.Message}";
        }

        return result;
    }

    private string ExtractStoreNameFromFileName(string fileName)
    {
        // Try to extract store name from filename (e.g., "coles_29102025.json" -> "Coles")
        if (fileName.ToLower().Contains("coles"))
            return "Coles";
        if (fileName.ToLower().Contains("woolworths") || fileName.ToLower().Contains("woolworth"))
            return "Woolworths";
        if (fileName.ToLower().Contains("aldi"))
            return "ALDI";
        if (fileName.ToLower().Contains("iga"))
            return "IGA";

        return "Unknown Store";
    }

    private string DetermineChain(string fileName, string jsonContent)
    {
        var lowerFileName = fileName.ToLower();
        var lowerContent = jsonContent.ToLower();

        if (lowerFileName.Contains("coles") || lowerContent.Contains("coles"))
            return "Coles";
        if (lowerFileName.Contains("woolworths") || lowerContent.Contains("woolworths"))
            return "Woolworths";
        if (lowerFileName.Contains("aldi"))
            return "ALDI";
        if (lowerFileName.Contains("iga"))
            return "IGA";

        return "Unknown";
    }

    /// <summary>
    /// Import Coles products from JSON file
    /// </summary>
    public ImportResult ImportColesJson(string jsonFilePath, string storeName = "Coles", DateTime? validDate = null)
    {
        var result = new ImportResult();
        var importDate = validDate ?? DateTime.UtcNow;

        try
        {
            // Read JSON file
            var jsonContent = File.ReadAllText(jsonFilePath);
            var colesData = JsonSerializer.Deserialize<ColesJsonData>(jsonContent, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (colesData?.Products == null || colesData.Products.Count == 0)
            {
                result.Success = false;
                result.ErrorMessage = "No products found in JSON file";
                return result;
            }

            // Get or create store location
            var store = GetOrCreateStore(storeName, "Coles");

            // Process each product
            foreach (var product in colesData.Products)
            {
                try
                {
                    // Create or update item
                    var item = CreateOrUpdateItem(product, "Coles");
                    result.ItemsProcessed++;

                    // Create price record
                    if (item != null)
                    {
                        var priceRecord = CreatePriceRecord(product, item.Id!, store.Id!, importDate);
                        _priceRecordRepository.Add(priceRecord);
                        result.PriceRecordsCreated++;
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error processing product {product.ProductID}: {ex.Message}");
                }
            }

            result.Success = true;
            result.Message = $"Successfully imported {result.ItemsProcessed} items and {result.PriceRecordsCreated} price records";
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Import failed: {ex.Message}";
        }

        return result;
    }

    private Item CreateOrUpdateItem(ColesProduct product, string chain)
    {
        // Try to find existing item by name and brand
        var existingItems = _itemRepository.SearchByName(product.ProductName)
            .Where(i => i.Brand == product.Brand);

        Item item;

        if (existingItems.Any())
        {
            // Update existing item
            item = existingItems.First();
            item.Name = product.ProductName;
            item.Description = product.Description;
            item.Brand = product.Brand;
            item.Category = product.Category;
            item.LastUpdated = DateTime.UtcNow;

            _itemRepository.Update(item);
        }
        else
        {
            // Create new item
            item = new Item
            {
                Name = product.ProductName,
                Description = product.Description,
                Brand = product.Brand,
                Category = product.Category,
                PackageSize = product.Description,
                IsActive = true,
                DateAdded = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                ExtraInformation = new Dictionary<string, string>
                {
                    ["ProductID"] = product.ProductID,
                    ["Store"] = chain
                }
            };

            // Add unit price if available
            if (!string.IsNullOrEmpty(product.UnitPrice))
            {
                item.ExtraInformation["UnitPrice"] = product.UnitPrice;
            }

            var id = _itemRepository.Add(item);
            item.Id = id;
        }

        return item;
    }

    private Place GetOrCreateStore(string storeName, string chain)
    {
        var existingStores = _placeRepository.SearchByName(storeName)
            .Where(p => p.Chain == chain);

        if (existingStores.Any())
        {
            return existingStores.First();
        }

        var store = new Place
        {
            Name = storeName,
            Chain = chain,
            IsActive = true,
            DateAdded = DateTime.UtcNow
        };

        var id = _placeRepository.Add(store);
        store.Id = id;
        return store;
    }

    private PriceRecord CreatePriceRecord(ColesProduct product, string itemId, string placeId, DateTime recordDate)
    {
        var price = ParsePrice(product.Price);
        var originalPrice = ParsePrice(product.OriginalPrice);
        var savings = ParsePrice(product.Savings);

        var saleDescription = savings > 0 ? $"Save ${savings:F2}" : null;
        if (!string.IsNullOrEmpty(product.SpecialType))
        {
            saleDescription = product.SpecialType;
        }

        return new PriceRecord
        {
            ItemId = itemId,
            PlaceId = placeId,
            Price = price,
            OriginalPrice = originalPrice > 0 ? originalPrice : null,
            IsOnSale = savings > 0,
            SaleDescription = saleDescription,
            DateRecorded = recordDate,
            ValidFrom = recordDate,
            ValidTo = recordDate.AddDays(7), // Assume weekly specials
            Source = "Catalogue",
            CatalogueDate = recordDate
        };
    }

    private decimal ParsePrice(string? priceString)
    {
        if (string.IsNullOrEmpty(priceString))
            return 0;

        // Remove $ and parse
        var cleanPrice = priceString.Replace("$", "").Trim();
        if (decimal.TryParse(cleanPrice, out var price))
            return price;

        return 0;
    }
}

/// <summary>
/// Model for Coles JSON data structure
/// </summary>
public class ColesJsonData
{
    public List<ColesProduct> Products { get; set; } = new();
}

/// <summary>
/// Model for individual Coles product
/// </summary>
public class ColesProduct
{
    public string ProductID { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string Price { get; set; } = string.Empty;
    public string? OriginalPrice { get; set; }
    public string? Savings { get; set; }
    public string? UnitPrice { get; set; }
    public string? SpecialType { get; set; }
}

/// <summary>
/// Result of import operation
/// </summary>
public class ImportResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public int ItemsProcessed { get; set; }
    public int PriceRecordsCreated { get; set; }
    public int ItemsSkipped { get; set; }
    public int ItemsFailed { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Progress report for import operations
/// </summary>
public class ImportProgress
{
    public int TotalItems { get; set; }
    public int ProcessedItems { get; set; }
    public string CurrentItem { get; set; } = string.Empty;
    public double PercentComplete => TotalItems > 0 ? (double)ProcessedItems / TotalItems * 100 : 0;
}
