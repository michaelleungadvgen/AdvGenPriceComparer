using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;


namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for importing weekly specials from supermarket catalogues
/// </summary>
public class WeeklySpecialsImportService : IWeeklySpecialsImportService
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly Application.Interfaces.ICategoryPredictionService? _categoryPredictionService;
    private readonly ILoggerService _logger;

    public WeeklySpecialsImportService(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILoggerService logger,
        Application.Interfaces.ICategoryPredictionService? categoryPredictionService = null)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _placeRepository = placeRepository ?? throw new ArgumentNullException(nameof(placeRepository));
        _priceRecordRepository = priceRecordRepository ?? throw new ArgumentNullException(nameof(priceRecordRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _categoryPredictionService = categoryPredictionService;
    }

    /// <inheritdoc />
    public async Task<WeeklySpecialsImportResult> ImportFromFileAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default)
    {
        _logger.LogInfo($"Starting weekly specials import from {options.FilePath} for {options.Chain}");

        var extension = Path.GetExtension(options.FilePath).ToLowerInvariant();
        
        return extension switch
        {
            ".json" => await ImportFromJsonAsync(options, cancellationToken),
            ".md" or ".markdown" or ".txt" => await ImportFromMarkdownAsync(options, cancellationToken),
            _ => new WeeklySpecialsImportResult
            {
                Success = false,
                Message = $"Unsupported file format: {extension}. Supported formats: .json, .md, .markdown, .txt",
                Chain = options.Chain
            }
        };
    }

    /// <inheritdoc />
    public async Task<WeeklySpecialsImportResult> ImportFromJsonAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default)
    {
        var result = new WeeklySpecialsImportResult { Chain = options.Chain };
        
        try
        {
            var json = await File.ReadAllTextAsync(options.FilePath, cancellationToken);
            var products = JsonSerializer.Deserialize<List<ColesProduct>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (products == null || !products.Any())
            {
                result.Success = false;
                result.Message = "No products found in JSON file";
                return result;
            }

            result.TotalItems = products.Count;
            var place = await GetOrCreatePlaceAsync(options.Chain);
            
            var validFrom = options.ValidFrom ?? DateTime.Today;
            var validTo = options.ValidTo ?? validFrom.AddDays(7);

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                cancellationToken.ThrowIfCancellationRequested();

                options.Progress?.Report(new WeeklySpecialsImportProgress
                {
                    CurrentItem = i + 1,
                    TotalItems = products.Count,
                    CurrentProductName = product.ProductName ?? "Unknown",
                    StatusMessage = $"Importing {product.ProductName}..."
                });

                try
                {
                    await ImportProductAsync(product, place.Id!, options, validFrom, validTo, cancellationToken);
                    result.ImportedItems++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to import product: {product.ProductName}", ex);
                    result.FailedItems++;
                    result.Errors.Add($"{product.ProductName}: {ex.Message}");
                }
            }

            result.Success = result.FailedItems == 0 || result.ImportedItems > 0;
            result.Message = $"Imported {result.ImportedItems} of {result.TotalItems} items. Failed: {result.FailedItems}";
            result.ValidFrom = validFrom;
            result.ValidTo = validTo;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to import from JSON", ex);
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<WeeklySpecialsImportResult> ImportFromMarkdownAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default)
    {
        var result = new WeeklySpecialsImportResult { Chain = options.Chain };
        
        try
        {
            var markdown = await File.ReadAllTextAsync(options.FilePath, cancellationToken);
            var products = options.Chain == SupermarketChain.Aldi 
                ? ParseAldiMarkdown(markdown)
                : ParseDrakesMarkdown(markdown);

            if (!products.Any())
            {
                result.Success = false;
                result.Message = "No products found in Markdown file";
                return result;
            }

            result.TotalItems = products.Count;
            var place = await GetOrCreatePlaceAsync(options.Chain);
            
            // Extract date range from markdown if not provided
            var (extractedFrom, extractedTo) = await ExtractDateRangeAsync(options.FilePath, options.Chain, cancellationToken);
            var validFrom = options.ValidFrom ?? extractedFrom ?? DateTime.Today;
            var validTo = options.ValidTo ?? extractedTo ?? validFrom.AddDays(7);

            for (int i = 0; i < products.Count; i++)
            {
                var product = products[i];
                cancellationToken.ThrowIfCancellationRequested();

                options.Progress?.Report(new WeeklySpecialsImportProgress
                {
                    CurrentItem = i + 1,
                    TotalItems = products.Count,
                    CurrentProductName = product.ProductName ?? "Unknown",
                    StatusMessage = $"Importing {product.ProductName}..."
                });

                try
                {
                    await ImportWeeklySpecialItemAsync(product, place.Id!, options, validFrom, validTo, cancellationToken);
                    result.ImportedItems++;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to import product: {product.ProductName}", ex);
                    result.FailedItems++;
                    result.Errors.Add($"{product.ProductName}: {ex.Message}");
                }
            }

            result.Success = result.FailedItems == 0 || result.ImportedItems > 0;
            result.Message = $"Imported {result.ImportedItems} of {result.TotalItems} items. Failed: {result.FailedItems}";
            result.ValidFrom = validFrom;
            result.ValidTo = validTo;
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to import from Markdown", ex);
            result.Success = false;
            result.Message = $"Import failed: {ex.Message}";
        }

        return result;
    }

    /// <inheritdoc />
    public Task<List<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem>> PreviewImportAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(options.FilePath).ToLowerInvariant();
        
        if (extension == ".json")
        {
            return PreviewJsonImportAsync(options, cancellationToken);
        }
        
        return PreviewMarkdownImportAsync(options, cancellationToken);
    }

    private async Task<List<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem>> PreviewJsonImportAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync(options.FilePath, cancellationToken);
        var products = JsonSerializer.Deserialize<List<ColesProduct>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (products == null) return new List<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem>();

        return products.Select(p => new AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem
        {
            ProductId = p.ProductID ?? "",
            ProductName = p.ProductName ?? "",
            Brand = p.Brand ?? "",
            Category = p.Category ?? "",
            Description = p.Description ?? "",
            Price = ParsePrice(p.Price),
            OriginalPrice = ParseNullablePrice(p.OriginalPrice),
            SpecialType = p.SpecialType ?? "",
            Chain = options.Chain
        }).ToList();
    }

    private async Task<List<AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem>> PreviewMarkdownImportAsync(WeeklySpecialsImportOptions options, CancellationToken cancellationToken)
    {
        var markdown = await File.ReadAllTextAsync(options.FilePath, cancellationToken);
        var products = options.Chain == SupermarketChain.Aldi 
            ? ParseAldiMarkdown(markdown)
            : ParseDrakesMarkdown(markdown);

        return products.Select(p => new AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Brand = p.Brand,
            Category = p.Category,
            Description = p.Description,
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            SpecialType = p.SpecialType,
            Chain = options.Chain
        }).ToList();
    }

    /// <inheritdoc />
    public string[] GetSupportedExtensions(SupermarketChain chain)
    {
        return chain switch
        {
            SupermarketChain.Coles or SupermarketChain.Woolworths => new[] { ".json" },
            SupermarketChain.Aldi or SupermarketChain.Drakes => new[] { ".md", ".markdown", ".txt" },
            _ => new[] { ".json", ".md", ".markdown", ".txt" }
        };
    }

    /// <inheritdoc />
    public async Task<SupermarketChain?> DetectChainAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        var fileName = Path.GetFileName(filePath).ToLowerInvariant();
        
        // Try filename detection first
        if (fileName.Contains("coles")) return SupermarketChain.Coles;
        if (fileName.Contains("woolworths")) return SupermarketChain.Woolworths;
        if (fileName.Contains("aldi")) return SupermarketChain.Aldi;
        if (fileName.Contains("drakes")) return SupermarketChain.Drakes;

        // Try content detection
        try
        {
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            if (extension == ".json")
            {
                // Try to detect from JSON structure
                if (content.Contains("\"productID\":\"COL")) return SupermarketChain.Coles;
                if (content.Contains("\"productID\":\"WOL")) return SupermarketChain.Woolworths;
            }
            else
            {
                // Try to detect from Markdown structure
                if (content.Contains("ADVENTURIDGE") || content.Contains("CROFTON")) return SupermarketChain.Aldi;
                if (content.Contains("Drakes") || content.Contains("Half Price Highlights")) return SupermarketChain.Drakes;
            }
        }
        catch
        {
            // Ignore errors in detection
        }

        return null;
    }

    /// <inheritdoc />
    public async Task<(DateTime? ValidFrom, DateTime? ValidTo)> ExtractDateRangeAsync(string filePath, SupermarketChain chain, CancellationToken cancellationToken = default)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, cancellationToken);
            
            if (chain == SupermarketChain.Drakes)
            {
                // Drakes format: **Sale: Wednesday 11/3/2026 to Tuesday 17/3/2026**
                var match = Regex.Match(content, @"Sale:\s*\w+\s+(\d{1,2}/\d{1,2}/\d{4})\s+to\s+\w+\s+(\d{1,2}/\d{1,2}/\d{4})");
                if (match.Success)
                {
                    if (DateTime.TryParseExact(match.Groups[1].Value, "d/M/yyyy", null, System.Globalization.DateTimeStyles.None, out var from))
                    {
                        if (DateTime.TryParseExact(match.Groups[2].Value, "d/M/yyyy", null, System.Globalization.DateTimeStyles.None, out var to))
                        {
                            return (from, to);
                        }
                    }
                }
            }
            else if (chain == SupermarketChain.Aldi)
            {
                // ALDI format: ## Available from Sat 21st March
                var match = Regex.Match(content, @"Available from\s+\w+\s+(\d{1,2})(?:st|nd|rd|th)\s+(\w+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var day = match.Groups[1].Value;
                    var month = match.Groups[2].Value;
                    var year = DateTime.Now.Year;
                    
                    if (DateTime.TryParseExact($"{day} {month} {year}", "d MMMM yyyy", null, System.Globalization.DateTimeStyles.None, out var from))
                    {
                        return (from, from.AddDays(7));
                    }
                }
            }
        }
        catch
        {
            // Ignore extraction errors
        }

        return (null, null);
    }

    private async Task<Place> GetOrCreatePlaceAsync(SupermarketChain chain)
    {
        var chainName = chain.ToString();
        var places = _placeRepository.GetAll().ToList();
        var place = places.FirstOrDefault(p => p.Name?.Contains(chainName) == true);

        if (place == null)
        {
            place = new Place
            {
                Name = $"{chainName} Supermarket",
                Chain = chainName,
                Address = "Unknown",
                Suburb = "Unknown",
                State = "QLD",
                Postcode = "4000"
            };
            var id = _placeRepository.Add(place);
            place.Id = id;
            _logger.LogInfo($"Created new place for {chainName}");
        }

        return place;
    }

    private async Task ImportProductAsync(ColesProduct product, string placeId, WeeklySpecialsImportOptions options, DateTime validFrom, DateTime validTo, CancellationToken cancellationToken)
    {
        // Check if item exists
        var items = _itemRepository.GetAll().ToList();
        var existingItem = items.FirstOrDefault(i => 
            i.Name?.Equals(product.ProductName, StringComparison.OrdinalIgnoreCase) == true &&
            i.Brand?.Equals(product.Brand, StringComparison.OrdinalIgnoreCase) == true);

        string itemId;
        if (existingItem == null || !options.SkipDuplicates)
        {
            // Create new item
            var category = options.AutoCategorize && _categoryPredictionService != null && string.IsNullOrEmpty(product.Category)
                ? PredictCategory(product)
                : (product.Category ?? "Uncategorized");

            var item = new Item
            {
                Name = product.ProductName ?? "Unknown Product",
                Brand = product.Brand,
                Category = category,
                Description = product.Description,
                DateAdded = DateTime.Now
            };
            itemId = _itemRepository.Add(item);
        }
        else
        {
            itemId = existingItem.Id!;
        }

        // Record the price
        var price = ParsePrice(product.Price);
        var originalPrice = ParseNullablePrice(product.OriginalPrice);

        _priceRecordRepository.Add(new PriceRecord
        {
            ItemId = itemId,
            PlaceId = placeId,
            Price = price,
            OriginalPrice = originalPrice,
            DateRecorded = DateTime.Now,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Source = $"{options.Chain} Catalogue",
            Notes = product.SpecialType
        });
    }

    private async Task ImportWeeklySpecialItemAsync(WeeklySpecialItem product, string placeId, WeeklySpecialsImportOptions options, DateTime validFrom, DateTime validTo, CancellationToken cancellationToken)
    {
        // Check if item exists
        var items = _itemRepository.GetAll().ToList();
        var existingItem = items.FirstOrDefault(i => 
            i.Name?.Equals(product.ProductName, StringComparison.OrdinalIgnoreCase) == true &&
            i.Brand?.Equals(product.Brand, StringComparison.OrdinalIgnoreCase) == true);

        string itemId;
        if (existingItem == null || !options.SkipDuplicates)
        {
            // Create new item
            var category = options.AutoCategorize && _categoryPredictionService != null && string.IsNullOrEmpty(product.Category)
                ? PredictCategory(product.ProductName, product.Brand)
                : (product.Category ?? "Uncategorized");

            var item = new Item
            {
                Name = product.ProductName,
                Brand = product.Brand,
                Category = category,
                Description = product.Description,
                DateAdded = DateTime.Now
            };
            itemId = _itemRepository.Add(item);
        }
        else
        {
            itemId = existingItem.Id!;
        }

        // Record the price
        _priceRecordRepository.Add(new PriceRecord
        {
            ItemId = itemId,
            PlaceId = placeId,
            Price = product.Price,
            OriginalPrice = product.OriginalPrice,
            DateRecorded = DateTime.Now,
            ValidFrom = validFrom,
            ValidTo = validTo,
            Source = $"{options.Chain} Catalogue",
            Notes = product.SpecialType
        });
    }

    private string PredictCategory(ColesProduct product)
    {
        if (_categoryPredictionService == null) return "Uncategorized";

        var tempItem = new Item
        {
            Name = product.ProductName ?? "",
            Brand = product.Brand ?? "",
            Description = product.Description ?? ""
        };

        var result = _categoryPredictionService.PredictCategory(tempItem);
        return result.PredictedCategory;
    }

    private string PredictCategory(string productName, string brand)
    {
        if (_categoryPredictionService == null) return "Uncategorized";

        var tempItem = new Item
        {
            Name = productName,
            Brand = brand
        };

        var result = _categoryPredictionService.PredictCategory(tempItem);
        return result.PredictedCategory;
    }

    private decimal ParsePrice(string? priceString)
    {
        if (string.IsNullOrEmpty(priceString)) return 0;
        
        var match = Regex.Match(priceString, @"[\$\u20AC\u00A3]?([\d,]+\.?\d*)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var price))
        {
            return price;
        }
        return 0;
    }

    private decimal? ParseNullablePrice(string? priceString)
    {
        if (string.IsNullOrEmpty(priceString)) return null;
        
        var match = Regex.Match(priceString, @"[\$\u20AC\u00A3]?([\d,]+\.?\d*)");
        if (match.Success && decimal.TryParse(match.Groups[1].Value.Replace(",", ""), out var price))
        {
            return price;
        }
        return null;
    }

    private List<WeeklySpecialItem> ParseAldiMarkdown(string markdown)
    {
        var items = new List<WeeklySpecialItem>();
        var lines = markdown.Split('\n');
        string currentCategory = "General";
        string? currentDateSection = null;

        foreach (var line in lines)
        {
            // Check for date section
            if (line.StartsWith("## Available from"))
            {
                currentDateSection = line.Replace("## Available from", "").Trim();
                continue;
            }

            // Check for category
            if (line.StartsWith("### "))
            {
                currentCategory = line.Replace("### ", "").Trim();
                continue;
            }

            // Parse product line: - **Product Name** - BRAND - $price
            var match = Regex.Match(line, @"-\s*\*\*(.+?)\*\*\s*-\s*(.+?)\s*-\s*\$([\d\.]+)");
            if (match.Success)
            {
                var productName = match.Groups[1].Value.Trim();
                var brand = match.Groups[2].Value.Trim();
                var price = decimal.Parse(match.Groups[3].Value);

                // Check for unit price in parentheses
                var unitPriceMatch = Regex.Match(line, @"\(\$([\d\.]+)/(.+?)\)");
                var description = unitPriceMatch.Success ? $"${unitPriceMatch.Groups[1].Value}/{unitPriceMatch.Groups[2].Value}" : "";

                items.Add(new AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem
                {
                    ProductId = $"ALDI_{items.Count:000}",
                    ProductName = productName,
                    Brand = brand,
                    Category = currentCategory,
                    Description = description,
                    Price = price,
                    Chain = SupermarketChain.Aldi
                });
            }
        }

        return items;
    }

    private List<WeeklySpecialItem> ParseDrakesMarkdown(string markdown)
    {
        var items = new List<WeeklySpecialItem>();
        var lines = markdown.Split('\n');
        string currentCategory = "Weekly Deals";
        bool inTable = false;

        foreach (var line in lines)
        {
            // Check for category headers
            if (line.StartsWith("## ") && !line.Contains("Sale:"))
            {
                currentCategory = line.Replace("## ", "").Trim();
                continue;
            }

            // Skip table header and separator lines
            if (line.StartsWith("|") && (line.Contains("Product") || line.Contains("----")))
            {
                inTable = true;
                continue;
            }

            // Parse table row
            if (line.StartsWith("|") && inTable)
            {
                var parts = line.Split('|').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).ToArray();
                if (parts.Length >= 2)
                {
                    var productName = parts[0];
                    var pricePart = parts[1];
                    var savePart = parts.Length > 2 ? parts[2] : "";

                    // Parse price
                    var priceMatch = Regex.Match(pricePart, @"\$([\d\.]+)");
                    if (priceMatch.Success && decimal.TryParse(priceMatch.Groups[1].Value, out var price))
                    {
                        // Parse unit price if available
                        var unitPriceMatch = Regex.Match(pricePart, @"\(\$([\d\.]+)/(.+?)\)");
                        var description = unitPriceMatch.Success ? $"${unitPriceMatch.Groups[1].Value}/{unitPriceMatch.Groups[2].Value}" : "";

                        // Determine special type from save column
                        var specialType = savePart.Contains("Half Price", StringComparison.OrdinalIgnoreCase) ? "Half Price" :
                                         savePart.Contains("$") ? $"Save {savePart}" : savePart;

                        items.Add(new AdvGenPriceComparer.Core.Interfaces.WeeklySpecialItem
                        {
                            ProductId = $"DRAKES_{items.Count:000}",
                            ProductName = productName,
                            Brand = "",
                            Category = currentCategory,
                            Description = description,
                            Price = price,
                            SpecialType = specialType,
                            Chain = SupermarketChain.Drakes
                        });
                    }
                }
            }
        }

        return items;
    }

    // DTO for JSON deserialization
    private class ColesProduct
    {
        public string? ProductID { get; set; }
        public string? ProductName { get; set; }
        public string? Category { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? Price { get; set; }
        public string? OriginalPrice { get; set; }
        public string? Savings { get; set; }
        public string? SpecialType { get; set; }
    }

}
