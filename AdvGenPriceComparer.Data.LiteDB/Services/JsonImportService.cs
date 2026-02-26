using System.Text.Json;
using System.Text.RegularExpressions;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;
using AdvGenPriceComparer.Data.LiteDB.Entities;
using AdvGenPriceComparer.Data.LiteDB.Repositories;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

// Note: ImportPreviewItem is defined in WPF.Models, not here to avoid duplication
// The service uses ColesProduct directly for data transfer

/// <summary>
/// Exception types for import errors
/// </summary>
public enum ImportErrorType
{
    ValidationError,
    FileNotFound,
    InvalidJson,
    InvalidData,
    DatabaseError,
    ParsingError,
    UnknownError
}

/// <summary>
/// Detailed import error information
/// </summary>
public class ImportError
{
    public ImportErrorType ErrorType { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? ProductName { get; set; }
    public int? LineNumber { get; set; }
    public string? RawData { get; set; }
}

/// <summary>
/// Validation result for input data
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ImportError> Errors { get; set; } = new();
}

/// <summary>
/// Service for importing grocery data from JSON files
/// </summary>
public class JsonImportService
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly Action<string>? _logInfo;
    private readonly Action<string, Exception>? _logError;
    private readonly Action<string>? _logWarning;

    public JsonImportService(IItemRepository itemRepository, 
        IPlaceRepository placeRepository, 
        IPriceRecordRepository priceRecordRepository,
        Action<string>? logInfo = null, 
        Action<string, Exception>? logError = null,
        Action<string>? logWarning = null)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;
    }

    private void LogInfo(string message) => _logInfo?.Invoke(message);
    private void LogError(string message, Exception? ex = null) => _logError?.Invoke(message, ex ?? new Exception(message));
    private void LogWarning(string message) => _logWarning?.Invoke(message);

    /// <summary>
    /// Validates a file path for import
    /// </summary>
    private ValidationResult ValidateFilePath(string filePath)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(filePath))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.ValidationError,
                Message = "File path cannot be null or empty"
            });
            return result;
        }

        if (!File.Exists(filePath))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.FileNotFound,
                Message = $"File not found: {filePath}"
            });
            return result;
        }

        var fileInfo = new FileInfo(filePath);
        if (fileInfo.Length == 0)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.ValidationError,
                Message = "File is empty"
            });
            return result;
        }

        // Check file size (max 50MB)
        const long maxFileSize = 50 * 1024 * 1024;
        if (fileInfo.Length > maxFileSize)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.ValidationError,
                Message = $"File size ({fileInfo.Length / 1024 / 1024}MB) exceeds maximum allowed (50MB)"
            });
            return result;
        }

        return result;
    }

    /// <summary>
    /// Validates JSON content structure
    /// </summary>
    private ValidationResult ValidateJsonContent(string jsonContent)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(jsonContent))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidJson,
                Message = "JSON content is empty"
            });
            return result;
        }

        // Check if it's valid JSON
        try
        {
            using var doc = JsonDocument.Parse(jsonContent);
            
            // Check if it's an array or object
            if (doc.RootElement.ValueKind != JsonValueKind.Array && 
                doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                result.IsValid = false;
                result.Errors.Add(new ImportError
                {
                    ErrorType = ImportErrorType.InvalidJson,
                    Message = "JSON must be an array or object"
                });
            }
        }
        catch (JsonException ex)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidJson,
                Message = $"Invalid JSON format: {ex.Message}"
            });
        }

        return result;
    }

    /// <summary>
    /// Validates a single product data
    /// </summary>
    private ValidationResult ValidateProduct(ColesProduct product, int index)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate product name
        if (string.IsNullOrWhiteSpace(product.ProductName))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Product name is required",
                FieldName = "ProductName",
                LineNumber = index
            });
        }
        else if (product.ProductName.Length > 500)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Product name exceeds maximum length of 500 characters",
                FieldName = "ProductName",
                ProductName = product.ProductName,
                LineNumber = index
            });
        }

        // Validate price
        if (string.IsNullOrWhiteSpace(product.Price))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Price is required",
                FieldName = "Price",
                ProductName = product.ProductName,
                LineNumber = index
            });
        }
        else
        {
            var priceValidation = ValidatePriceFormat(product.Price, "Price", index, product.ProductName);
            if (!priceValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(priceValidation.Errors);
            }
        }

        // Validate original price if provided
        if (!string.IsNullOrWhiteSpace(product.OriginalPrice))
        {
            var origPriceValidation = ValidatePriceFormat(product.OriginalPrice, "OriginalPrice", index, product.ProductName);
            if (!origPriceValidation.IsValid)
            {
                result.IsValid = false;
                result.Errors.AddRange(origPriceValidation.Errors);
            }
        }

        // Validate category if provided
        if (!string.IsNullOrWhiteSpace(product.Category) && product.Category.Length > 200)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Category exceeds maximum length of 200 characters",
                FieldName = "Category",
                ProductName = product.ProductName,
                LineNumber = index
            });
        }

        // Validate brand if provided
        if (!string.IsNullOrWhiteSpace(product.Brand) && product.Brand.Length > 200)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Brand exceeds maximum length of 200 characters",
                FieldName = "Brand",
                ProductName = product.ProductName,
                LineNumber = index
            });
        }

        return result;
    }

    /// <summary>
    /// Validates price format
    /// </summary>
    private ValidationResult ValidatePriceFormat(string priceString, string fieldName, int index, string? productName)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(priceString))
        {
            return result; // Empty is valid (will be treated as 0)
        }

        // Remove $ and whitespace
        var cleanPrice = priceString.Replace("$", "").Trim();
        
        // Check for valid decimal format
        if (!decimal.TryParse(cleanPrice, out var price))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = $"Invalid price format: '{priceString}'. Expected format: $X.XX",
                FieldName = fieldName,
                ProductName = productName,
                LineNumber = index,
                RawData = priceString
            });
            return result;
        }

        // Check for negative prices
        if (price < 0)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Price cannot be negative",
                FieldName = fieldName,
                ProductName = productName,
                LineNumber = index,
                RawData = priceString
            });
        }

        // Check for unreasonably high prices (over $10,000)
        if (price > 10000)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.InvalidData,
                Message = "Price exceeds maximum allowed value ($10,000)",
                FieldName = fieldName,
                ProductName = productName,
                LineNumber = index,
                RawData = priceString
            });
        }

        return result;
    }

    /// <summary>
    /// Validates store ID
    /// </summary>
    private ValidationResult ValidateStoreId(string storeId)
    {
        var result = new ValidationResult { IsValid = true };

        if (string.IsNullOrWhiteSpace(storeId))
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.ValidationError,
                Message = "Store ID is required"
            });
            return result;
        }

        var store = _placeRepository.GetById(storeId);
        if (store == null)
        {
            result.IsValid = false;
            result.Errors.Add(new ImportError
            {
                ErrorType = ImportErrorType.ValidationError,
                Message = $"Store with ID '{storeId}' not found in database"
            });
        }

        return result;
    }

    /// <summary>
    /// Preview import from JSON file without saving to database
    /// Returns ColesProduct list that can be converted to preview items by the ViewModel
    /// </summary>
    public async Task<(List<ColesProduct> Products, List<ImportError> Errors)> PreviewImportAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var errors = new List<ImportError>();
            
            try
            {
                // Validate file path
                var pathValidation = ValidateFilePath(filePath);
                if (!pathValidation.IsValid)
                {
                    errors.AddRange(pathValidation.Errors);
                    LogWarning($"File validation failed for {filePath}: {string.Join(", ", pathValidation.Errors.Select(e => e.Message))}");
                    return (new List<ColesProduct>(), errors);
                }

                string jsonContent;
                try
                {
                    jsonContent = File.ReadAllText(filePath);
                }
                catch (IOException ex)
                {
                    errors.Add(new ImportError
                    {
                        ErrorType = ImportErrorType.FileNotFound,
                        Message = $"Failed to read file: {ex.Message}"
                    });
                    LogError($"Failed to read file {filePath}", ex);
                    return (new List<ColesProduct>(), errors);
                }

                // Validate JSON content
                var jsonValidation = ValidateJsonContent(jsonContent);
                if (!jsonValidation.IsValid)
                {
                    errors.AddRange(jsonValidation.Errors);
                    LogWarning($"JSON validation failed for {filePath}");
                    return (new List<ColesProduct>(), errors);
                }

                // Deserialize
                List<ColesProduct>? colesData;
                try
                {
                    colesData = JsonSerializer.Deserialize<List<ColesProduct>>(jsonContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                }
                catch (JsonException ex)
                {
                    errors.Add(new ImportError
                    {
                        ErrorType = ImportErrorType.InvalidJson,
                        Message = $"Failed to parse JSON: {ex.Message}"
                    });
                    LogError($"JSON deserialization failed for {filePath}", ex);
                    return (new List<ColesProduct>(), errors);
                }

                if (colesData == null)
                {
                    errors.Add(new ImportError
                    {
                        ErrorType = ImportErrorType.InvalidJson,
                        Message = "JSON deserialized to null"
                    });
                    return (new List<ColesProduct>(), errors);
                }

                // Validate each product
                var validProducts = new List<ColesProduct>();
                for (int i = 0; i < colesData.Count; i++)
                {
                    var productValidation = ValidateProduct(colesData[i], i);
                    if (productValidation.IsValid)
                    {
                        validProducts.Add(colesData[i]);
                    }
                    else
                    {
                        errors.AddRange(productValidation.Errors);
                    }
                }

                LogInfo($"Preview import completed for {filePath}: {validProducts.Count} valid products, {errors.Count} errors");
                return (validProducts, errors);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error during preview import of {filePath}", ex);
                errors.Add(new ImportError
                {
                    ErrorType = ImportErrorType.UnknownError,
                    Message = $"Unexpected error: {ex.Message}"
                });
                return (new List<ColesProduct>(), errors);
            }
        }, cancellationToken);
    }

    /// <summary>
    /// Import Coles products with specified store and date
    /// </summary>
    public ImportResult ImportColesProducts(List<ColesProduct> products, string storeId, DateTime catalogueDate, 
        Dictionary<string, string>? existingItemMappings = null, IProgress<ImportProgress>? progress = null, DateTime? expiryDate = null)
    {
        var result = new ImportResult();
        var detailedErrors = new List<ImportError>();
        existingItemMappings ??= new Dictionary<string, string>();
        
        // Validate store
        var storeValidation = ValidateStoreId(storeId);
        if (!storeValidation.IsValid)
        {
            detailedErrors.AddRange(storeValidation.Errors);
            result.Success = false;
            result.ErrorMessage = "Store validation failed";
            result.Errors = detailedErrors.Select(e => e.Message).ToList();
            LogError($"Import failed: Store validation failed for {storeId}");
            return result;
        }

        // Validate products list
        if (products == null || products.Count == 0)
        {
            result.Success = false;
            result.ErrorMessage = "No products to import";
            LogWarning("Import failed: Empty product list");
            return result;
        }

        // Validate catalogue date
        if (catalogueDate > DateTime.Now.AddYears(1))
        {
            result.Success = false;
            result.ErrorMessage = "Catalogue date is too far in the future";
            LogWarning($"Import failed: Invalid catalogue date {catalogueDate}");
            return result;
        }

        if (catalogueDate < DateTime.Now.AddYears(-5))
        {
            result.Success = false;
            result.ErrorMessage = "Catalogue date is too old (more than 5 years)";
            LogWarning($"Import failed: Catalogue date too old {catalogueDate}");
            return result;
        }

        var store = _placeRepository.GetById(storeId);
        int totalItems = products.Count;
        int processedCount = 0;
        int skippedCount = 0;

        LogInfo($"Starting import of {totalItems} products for store {store.Name}");

        foreach (var product in products)
        {
            try
            {
                // Validate product before processing
                var productValidation = ValidateProduct(product, processedCount);
                if (!productValidation.IsValid)
                {
                    detailedErrors.AddRange(productValidation.Errors);
                    skippedCount++;
                    processedCount++;
                    continue;
                }

                string itemId;
                bool addPriceRecordOnly = existingItemMappings.TryGetValue(product.GetProductId(), out var existingId) 
                    && !string.IsNullOrEmpty(existingId);

                if (addPriceRecordOnly)
                {
                    // Verify the existing item still exists
                    var existingItem = _itemRepository.GetById(existingId);
                    if (existingItem == null)
                    {
                        detailedErrors.Add(new ImportError
                        {
                            ErrorType = ImportErrorType.DatabaseError,
                            Message = $"Referenced item no longer exists: {existingId}",
                            ProductName = product.ProductName
                        });
                        skippedCount++;
                        processedCount++;
                        continue;
                    }
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
                        detailedErrors.Add(new ImportError
                        {
                            ErrorType = ImportErrorType.DatabaseError,
                            Message = "Failed to create item in database",
                            ProductName = product.ProductName
                        });
                        skippedCount++;
                        processedCount++;
                        continue;
                    }
                }

                // Create price record
                var priceRecord = CreatePriceRecord(product, itemId, storeId, catalogueDate, expiryDate);
                _priceRecordRepository.Add(priceRecord);
                result.PriceRecordsCreated++;
            }
            catch (Exception ex)
            {
                detailedErrors.Add(new ImportError
                {
                    ErrorType = ImportErrorType.UnknownError,
                    Message = $"Error processing product: {ex.Message}",
                    ProductName = product.ProductName
                });
                LogError($"Error importing product {product.ProductName}", ex);
            }

            processedCount++;
            progress?.Report(new ImportProgress
            {
                TotalItems = totalItems,
                ProcessedItems = processedCount,
                CurrentItem = product.ProductName
            });
        }

        result.Success = detailedErrors.Count == 0 || result.ItemsProcessed > 0;
        result.Errors = detailedErrors.Select(e => $"[{e.ErrorType}] {e.Message}" + 
            (e.ProductName != null ? $" (Product: {e.ProductName})" : "")).ToList();
        
        if (result.Success)
        {
            result.Message = $"Imported {result.ItemsProcessed} new items and {result.PriceRecordsCreated} price records" +
                (detailedErrors.Count > 0 ? $" with {detailedErrors.Count} warnings" : "");
            LogInfo($"Import completed: {result.ItemsProcessed} items, {result.PriceRecordsCreated} price records, {detailedErrors.Count} errors");
        }
        else
        {
            result.ErrorMessage = "Import failed - see errors for details";
            LogError($"Import failed with {detailedErrors.Count} errors");
        }
        
        return result;
    }

    /// <summary>
    /// Import data from JSON file path
    /// </summary>
    public ImportResult ImportFromFile(string filePath, DateTime? validDate = null)
    {
        LogInfo($"Starting import from file: {filePath}");
        
        // Validate file path
        var pathValidation = ValidateFilePath(filePath);
        if (!pathValidation.IsValid)
        {
            LogError($"File validation failed for {filePath}");
            return new ImportResult
            {
                Success = false,
                ErrorMessage = pathValidation.Errors.First().Message,
                Errors = pathValidation.Errors.Select(e => e.Message).ToList()
            };
        }

        string jsonContent;
        try
        {
            jsonContent = File.ReadAllText(filePath);
        }
        catch (IOException ex)
        {
            LogError($"Failed to read file {filePath}", ex);
            return new ImportResult
            {
                Success = false,
                ErrorMessage = $"Failed to read file: {ex.Message}"
            };
        }

        // Validate JSON content
        var jsonValidation = ValidateJsonContent(jsonContent);
        if (!jsonValidation.IsValid)
        {
            LogError($"JSON validation failed for {filePath}");
            return new ImportResult
            {
                Success = false,
                ErrorMessage = jsonValidation.Errors.First().Message,
                Errors = jsonValidation.Errors.Select(e => e.Message).ToList()
            };
        }

        var fileName = Path.GetFileName(filePath);

        // Try to detect the format (Coles or Woolworths)
        if (jsonContent.Contains("\"ProductID\"") || jsonContent.Contains("\"productID\"") ||
            jsonContent.Contains("\"productName\"") || jsonContent.Contains("\"ProductName\""))
        {
            return ImportColesJsonContent(jsonContent, fileName, validDate);
        }
        else
        {
            LogWarning($"Unsupported JSON format in file {filePath}");
            return new ImportResult
            {
                Success = false,
                ErrorMessage = "Unsupported JSON format. Expected Coles or Woolworths format with ProductID or productName fields."
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
                    result.Errors.Add($"Error processing product '{product.ProductName}': {ex.Message}");
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
                    result.Errors.Add($"Error processing product '{product.ProductName}': {ex.Message}");
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
        // Try to find existing item by exact name match and brand
        // Use exact matching to avoid false positives (e.g., "Coca-Cola 1.25L" vs "Coca-Cola 2L")
        var existingItems = _itemRepository.GetAll()
            .Where(i => i.Name.Equals(product.ProductName, StringComparison.OrdinalIgnoreCase) 
                && i.Brand == product.Brand
                && i.IsActive);

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
                    ["ProductID"] = product.GetProductId(),
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

    private PriceRecord CreatePriceRecord(ColesProduct product, string itemId, string placeId, DateTime recordDate, DateTime? expiryDate = null)
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
            ValidTo = expiryDate ?? recordDate.AddDays(7), // Use provided expiry date or default to 7 days
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

    /// <summary>
    /// Import from Drakes markdown catalogue file
    /// </summary>
    public ImportResult ImportFromDrakesMarkdown(string filePath, string storeName = "Drakes", DateTime? validDate = null)
    {
        var result = new ImportResult();
        var parser = new DrakesMarkdownParser();
        var parseResult = parser.ParseFile(filePath);

        if (!parseResult.Success)
        {
            result.Success = false;
            result.ErrorMessage = parseResult.ErrorMessage ?? "Failed to parse markdown file";
            return result;
        }

        // Get or create store
        var store = GetOrCreateStore(storeName, "Drakes");
        var importDate = validDate ?? parseResult.ValidFrom;

        // Import products
        return ImportColesProducts(parseResult.Products, store.Id!, importDate);
    }

    /// <summary>
    /// Preview import from Drakes markdown file without saving
    /// </summary>
    public async Task<(List<ColesProduct> Products, List<ImportError> Errors)> PreviewDrakesMarkdownAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return await Task.Run(() =>
        {
            var errors = new List<ImportError>();
            
            try
            {
                // Validate file path
                var pathValidation = ValidateFilePath(filePath);
                if (!pathValidation.IsValid)
                {
                    errors.AddRange(pathValidation.Errors);
                    LogWarning($"File validation failed for markdown {filePath}");
                    return (new List<ColesProduct>(), errors);
                }

                var parser = new DrakesMarkdownParser(LogInfo, msg => LogError(msg), LogWarning);
                var result = parser.ParseFile(filePath);
                
                if (!result.Success)
                {
                    errors.Add(new ImportError
                    {
                        ErrorType = ImportErrorType.ParsingError,
                        Message = result.ErrorMessage ?? "Failed to parse markdown file"
                    });
                    LogWarning($"Markdown parsing failed for {filePath}: {result.ErrorMessage}");
                    return (new List<ColesProduct>(), errors);
                }

                // Validate each parsed product
                var validProducts = new List<ColesProduct>();
                for (int i = 0; i < result.Products.Count; i++)
                {
                    var productValidation = ValidateProduct(result.Products[i], i);
                    if (productValidation.IsValid)
                    {
                        validProducts.Add(result.Products[i]);
                    }
                    else
                    {
                        errors.AddRange(productValidation.Errors);
                    }
                }

                LogInfo($"Markdown preview completed for {filePath}: {validProducts.Count} valid products, {errors.Count} errors");
                return (validProducts, errors);
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error during markdown preview of {filePath}", ex);
                errors.Add(new ImportError
                {
                    ErrorType = ImportErrorType.UnknownError,
                    Message = $"Unexpected error: {ex.Message}"
                });
                return (new List<ColesProduct>(), errors);
            }
        }, cancellationToken);
    }
}

/// <summary>
/// Parser for Drakes markdown catalogue format
/// </summary>
public class DrakesMarkdownParser
{
    private readonly Action<string>? _logInfo;
    private readonly Action<string>? _logError;
    private readonly Action<string>? _logWarning;

    public DrakesMarkdownParser(Action<string>? logInfo = null, Action<string>? logError = null, Action<string>? logWarning = null)
    {
        _logInfo = logInfo;
        _logError = logError;
        _logWarning = logWarning;
    }

    private void LogInfo(string message) => _logInfo?.Invoke(message);
    private void LogError(string message) => _logError?.Invoke(message);
    private void LogWarning(string message) => _logWarning?.Invoke(message);

    public ParseResult ParseFile(string filePath)
    {
        try
        {
            LogInfo($"Parsing markdown file: {filePath}");
            
            if (!File.Exists(filePath))
            {
                LogError($"Markdown file not found: {filePath}");
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = $"File not found: {filePath}"
                };
            }

            var content = File.ReadAllText(filePath);
            
            if (string.IsNullOrWhiteSpace(content))
            {
                LogWarning($"Markdown file is empty: {filePath}");
                return new ParseResult
                {
                    Success = false,
                    ErrorMessage = "File is empty"
                };
            }

            var products = new List<ColesProduct>();
            var categories = new HashSet<string>();

            // Extract date range
            var (validFrom, validTo) = ExtractDateRange(content);

            // Extract categories and products from markdown tables
            var sections = ExtractSections(content);

            foreach (var section in sections)
            {
                categories.Add(section.Category);
                var sectionProducts = ParseTableProducts(section.Content, section.Category);
                products.AddRange(sectionProducts);
            }

            return new ParseResult
            {
                Success = products.Count > 0,
                Products = products,
                Categories = categories.ToList(),
                ValidFrom = validFrom,
                ValidTo = validTo,
                FilePath = filePath
            };
        }
        catch (Exception ex)
        {
            return new ParseResult
            {
                Success = false,
                ErrorMessage = ex.Message,
                Products = new List<ColesProduct>(),
                Categories = new List<string>()
            };
        }
    }

    private (DateTime fromDate, DateTime toDate) ExtractDateRange(string content)
    {
        // Pattern: **Sale Period:** Wednesday 4/2/2026 to Tuesday 10/2/2026
        var pattern = @"(?:Sale Period|Valid):?\s*(?:\*\*)?\s*(?:\w+\s+)?(\d{1,2}[\/\.]\d{1,2}[\/\.]\d{2,4})\s*(?:to|-)\s*(?:\w+\s+)?(\d{1,2}[\/\.]\d{1,2}[\/\.]\d{2,4})";
        var match = Regex.Match(content, pattern, RegexOptions.IgnoreCase);

        DateTime fromDate = DateTime.Today;
        DateTime toDate = DateTime.Today.AddDays(7);

        if (match.Success)
        {
            DateTime.TryParse(match.Groups[1].Value, out fromDate);
            DateTime.TryParse(match.Groups[2].Value, out toDate);
        }

        return (fromDate, toDate);
    }

    private List<SectionInfo> ExtractSections(string content)
    {
        var sections = new List<SectionInfo>();

        // Split by ## headers (category sections)
        var sectionPattern = @"##\s+(.+?)\n\n*(?:\|[^\n]*\|\n\|[-:\s|]*\|\n)?([^#]*?)(?=\n##\s+|\z)";
        var matches = Regex.Matches(content, sectionPattern, RegexOptions.Singleline);

        foreach (Match match in matches)
        {
            var category = match.Groups[1].Value.Trim();
            var sectionContent = match.Groups[2].Value;
            
            sections.Add(new SectionInfo
            {
                Category = category,
                Content = sectionContent
            });
        }

        return sections;
    }

    private List<ColesProduct> ParseTableProducts(string tableContent, string category)
    {
        var products = new List<ColesProduct>();

        // Parse markdown table rows
        var rowPattern = @"\|\s*([^|\n]+)\|\s*([^|\n]+)\|\s*([^|\n]+)\|\s*";
        var rows = Regex.Matches(tableContent, rowPattern);

        int rowIndex = 0;
        foreach (Match row in rows)
        {
            // Skip header row and separator row
            if (rowIndex++ < 2) continue;

            var productName = row.Groups[1].Value.Trim();
            var priceStr = row.Groups[2].Value.Trim();
            var savingsStr = row.Groups[3].Value.Trim();

            // Skip empty rows or header-like rows
            if (string.IsNullOrWhiteSpace(productName) || 
                productName.Contains("Product", StringComparison.OrdinalIgnoreCase))
                continue;

            var product = CreateProduct(productName, priceStr, savingsStr, category);
            if (product != null)
            {
                products.Add(product);
            }
        }

        return products;
    }

    private ColesProduct? CreateProduct(string productName, string priceStr, string savingsStr, string category)
    {
        // Extract price - handle formats like "$24.75", "$7.90/kg"
        var priceMatch = Regex.Match(priceStr, @"\$([\d.,]+)");
        if (!priceMatch.Success) return null;

        var price = "$" + priceMatch.Groups[1].Value;

        // Determine special type from savings
        string? specialType = null;
        string? originalPrice = null;

        if (savingsStr.Contains("1/2 Price", StringComparison.OrdinalIgnoreCase))
        {
            specialType = "Half Price";
            // Calculate original price
            if (decimal.TryParse(priceMatch.Groups[1].Value, out var p))
            {
                originalPrice = "$" + (p * 2).ToString("F2");
            }
        }
        else if (savingsStr.Contains("SAVE", StringComparison.OrdinalIgnoreCase))
        {
            var saveMatch = Regex.Match(savingsStr, @"SAVE\s*\$?([\d.,]+)");
            if (saveMatch.Success)
            {
                specialType = savingsStr.Trim();
                if (decimal.TryParse(priceMatch.Groups[1].Value, out var p) &&
                    decimal.TryParse(saveMatch.Groups[1].Value, out var s))
                {
                    originalPrice = "$" + (p + s).ToString("F2");
                }
            }
        }
        else if (!string.IsNullOrWhiteSpace(savingsStr) && !savingsStr.Contains("Value", StringComparison.OrdinalIgnoreCase))
        {
            specialType = savingsStr.Trim();
        }

        // Extract size from product name if present
        var sizeMatch = Regex.Match(productName, @"(\d+(?:\.\d+)?\s*(?:g|kg|ml|L|lt|pack))", RegexOptions.IgnoreCase);
        var description = sizeMatch.Success ? sizeMatch.Value : "";

        // Extract brand from product name
        var brand = ExtractBrand(productName);

        return new ColesProduct
        {
            ProductID = $"DRK_{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            ProductName = productName,
            Category = MapCategory(category),
            Brand = brand,
            Description = description,
            Price = price,
            OriginalPrice = originalPrice,
            Savings = savingsStr.Contains("SAVE") ? savingsStr : null,
            SpecialType = specialType
        };
    }

    private string MapCategory(string category)
    {
        // Map Drakes categories to standard categories
        var categoryLower = category.ToLower();

        if (categoryLower.Contains("meat") || categoryLower.Contains("seafood"))
            return "Meat & Seafood";
        if (categoryLower.Contains("fruit") || categoryLower.Contains("vegetable") || categoryLower.Contains("fresh produce"))
            return "Fruits & Vegetables";
        if (categoryLower.Contains("dairy") || categoryLower.Contains("egg"))
            return "Dairy & Eggs";
        if (categoryLower.Contains("bakery") || categoryLower.Contains("bread"))
            return "Bakery";
        if (categoryLower.Contains("frozen"))
            return "Frozen Foods";
        if (categoryLower.Contains("drink") || categoryLower.Contains("beverage"))
            return "Beverages";
        if (categoryLower.Contains("snack") || categoryLower.Contains("confectionery"))
            return "Snacks & Confectionery";
        if (categoryLower.Contains("deli"))
            return "Deli";
        if (categoryLower.Contains("half price") || categoryLower.Contains("special"))
            return "Specials";

        return "Grocery";
    }

    private string? ExtractBrand(string productName)
    {
        // Common brands to detect
        var brands = new[]
        {
            "Streets", "Arnott's", "Connoisseur", "Chobani", "McCain",
            "Coca-Cola", "Nice & Natural", "Golden Circle", "Pepsi", "Schweppes",
            "Smith's", "Cadbury", "Nestle", "Wonder", "Joojoos", "Fini",
            "Kellogg's", "Latina Fresh", "Cottee's", "Leggo's", "Ingham's",
            "I & J", "Nescafe", "Uncle Tobys", "Sanitarium", "Moccona"
        };

        foreach (var brand in brands)
        {
            if (productName.Contains(brand, StringComparison.OrdinalIgnoreCase))
                return brand;
        }

        // Try to extract first word as brand
        var firstWord = productName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        if (!string.IsNullOrEmpty(firstWord) && firstWord.Length > 2)
            return firstWord;

        return null;
    }
}

public class SectionInfo
{
    public string Category { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

public class ParseResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<ColesProduct> Products { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
    public string FilePath { get; set; } = string.Empty;
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
    /// <summary>
    /// Product ID from source (optional - will be auto-generated if not provided)
    /// </summary>
    public string? ProductID { get; set; }
    
    /// <summary>
    /// Product name (required)
    /// </summary>
    public string ProductName { get; set; } = string.Empty;
    
    /// <summary>
    /// Product category (optional)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Brand name (optional)
    /// </summary>
    public string? Brand { get; set; }
    
    /// <summary>
    /// Product description (optional)
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Current price (required)
    /// </summary>
    public string Price { get; set; } = string.Empty;
    
    /// <summary>
    /// Original price before discount (optional)
    /// </summary>
    public string? OriginalPrice { get; set; }
    
    /// <summary>
    /// Savings amount (optional)
    /// </summary>
    public string? Savings { get; set; }
    
    /// <summary>
    /// Unit price (e.g., per 100g, per kg) (optional)
    /// </summary>
    public string? UnitPrice { get; set; }
    
    /// <summary>
    /// Type of special offer (optional)
    /// </summary>
    public string? SpecialType { get; set; }
    
    /// <summary>
    /// Get or generate a product ID for this product
    /// </summary>
    public string GetProductId()
    {
        if (!string.IsNullOrEmpty(ProductID))
            return ProductID;
        
        // Generate a stable ID based on product name and brand
        // This allows the same product to be matched across different files
        var idSource = $"{Brand?.ToLowerInvariant() ?? "unknown"}_{ProductName.ToLowerInvariant().Replace(" ", "_")}";
        var hash = System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes(idSource));
        return "GEN_" + BitConverter.ToString(hash).Replace("-", "")[..12];
    }
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
