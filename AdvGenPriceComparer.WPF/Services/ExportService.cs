using System.IO;
using System.IO.Compression;
using System.Text.Json;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for exporting grocery price data to JSON format
/// </summary>
public class ExportService
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly ILoggerService _logger;

    public ExportService(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        ILoggerService logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _logger = logger;
    }

    /// <summary>
    /// Export items to standardized JSON format
    /// </summary>
    public async Task<ExportResult> ExportToJsonAsync(ExportOptions options, string outputPath, IProgress<ExportProgress>? progress = null)
    {
        var result = new ExportResult();
        
        try
        {
            _logger.LogInfo($"Starting export to {outputPath}");
            progress?.Report(new ExportProgress { Percentage = 0, Status = "Fetching items..." });

            // Get all items
            var allItems = _itemRepository.GetAll().ToList();
            var filteredItems = FilterItems(allItems, options).ToList();
            
            _logger.LogInfo($"Found {filteredItems.Count} items to export (from {allItems.Count} total)");
            progress?.Report(new ExportProgress { Percentage = 10, Status = $"Found {filteredItems.Count} items..." });

            // Build export data
            var exportData = await BuildExportDataAsync(filteredItems, options, progress);
            
            progress?.Report(new ExportProgress { Percentage = 80, Status = "Writing to file..." });

            // Serialize to JSON
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(exportData, jsonOptions);
            await File.WriteAllTextAsync(outputPath, json);

            result.Success = true;
            result.ItemsExported = exportData.Items.Count;
            result.FilePath = outputPath;
            result.FileSizeBytes = new FileInfo(outputPath).Length;
            result.Message = $"Successfully exported {result.ItemsExported} items to {outputPath}";
            
            _logger.LogInfo(result.Message);
            progress?.Report(new ExportProgress { Percentage = 100, Status = "Export complete!" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Export failed: {ex.Message}";
            _logger.LogError(result.ErrorMessage, ex);
            progress?.Report(new ExportProgress { Percentage = 0, Status = $"Error: {ex.Message}" });
        }

        return result;
    }

    /// <summary>
    /// Export to JSON with GZip compression
    /// </summary>
    public async Task<ExportResult> ExportToJsonGzAsync(ExportOptions options, string outputPath, IProgress<ExportProgress>? progress = null)
    {
        var result = new ExportResult();
        
        try
        {
            _logger.LogInfo($"Starting compressed export to {outputPath}");
            progress?.Report(new ExportProgress { Percentage = 0, Status = "Fetching items..." });

            // Get and filter items
            var allItems = _itemRepository.GetAll().ToList();
            var filteredItems = FilterItems(allItems, options).ToList();
            
            _logger.LogInfo($"Found {filteredItems.Count} items to export");
            progress?.Report(new ExportProgress { Percentage = 10, Status = $"Found {filteredItems.Count} items..." });

            // Build export data
            var exportData = await BuildExportDataAsync(filteredItems, options, progress);
            
            progress?.Report(new ExportProgress { Percentage = 80, Status = "Compressing..." });

            // Serialize and compress
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false, // No indentation for compressed output
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };

            var json = JsonSerializer.Serialize(exportData, jsonOptions);
            var jsonBytes = System.Text.Encoding.UTF8.GetBytes(json);

            // Compress
            using (var fileStream = File.Create(outputPath))
            using (var gzipStream = new GZipStream(fileStream, CompressionLevel.Optimal))
            {
                await gzipStream.WriteAsync(jsonBytes);
            }

            result.Success = true;
            result.ItemsExported = exportData.Items.Count;
            result.FilePath = outputPath;
            result.FileSizeBytes = new FileInfo(outputPath).Length;
            result.CompressionRatio = (double)result.FileSizeBytes / jsonBytes.Length;
            result.Message = $"Successfully exported {result.ItemsExported} items to {outputPath} (compressed)";
            
            _logger.LogInfo(result.Message);
            progress?.Report(new ExportProgress { Percentage = 100, Status = "Export complete!" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Export failed: {ex.Message}";
            _logger.LogError(result.ErrorMessage, ex);
            progress?.Report(new ExportProgress { Percentage = 0, Status = $"Error: {ex.Message}" });
        }

        return result;
    }

    /// <summary>
    /// Incremental export - only items changed since last export
    /// </summary>
    public async Task<ExportResult> IncrementalExportAsync(DateTime lastExportDate, string outputPath, IProgress<ExportProgress>? progress = null)
    {
        var options = new ExportOptions
        {
            LastUpdatedAfter = lastExportDate
        };

        return await ExportToJsonAsync(options, outputPath, progress);
    }

    /// <summary>
    /// Filter items based on export options
    /// </summary>
    private IEnumerable<Item> FilterItems(IEnumerable<Item> items, ExportOptions options)
    {
        var query = items.AsEnumerable();

        // Category filter
        if (!string.IsNullOrEmpty(options.Category))
        {
            query = query.Where(i => i.Category?.Equals(options.Category, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Brand filter
        if (!string.IsNullOrEmpty(options.Brand))
        {
            query = query.Where(i => i.Brand?.Equals(options.Brand, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Date range filter (based on LastUpdated)
        if (options.LastUpdatedAfter.HasValue)
        {
            query = query.Where(i => i.LastUpdated >= options.LastUpdatedAfter.Value);
        }

        if (options.LastUpdatedBefore.HasValue)
        {
            query = query.Where(i => i.LastUpdated <= options.LastUpdatedBefore.Value);
        }

        // Active items only (unless specified)
        if (options.ActiveOnly)
        {
            query = query.Where(i => i.IsActive);
        }

        return query;
    }

    /// <summary>
    /// Build export data structure
    /// </summary>
    private async Task<ExportData> BuildExportDataAsync(List<Item> items, ExportOptions options, IProgress<ExportProgress>? progress)
    {
        var exportItems = new List<ExportItem>();
        var places = _placeRepository.GetAll().ToDictionary(p => p.Id, p => p);
        
        int processedCount = 0;
        int totalCount = items.Count;

        await Task.Run(() =>
        {
            foreach (var item in items)
            {
                // Get latest price records for this item
                var priceRecords = _priceRecordRepository.GetByItem(item.Id!).ToList();
                
                // Filter by store if specified
                if (options.StoreIds?.Any() == true)
                {
                    priceRecords = priceRecords.Where(pr => options.StoreIds.Contains(pr.PlaceId)).ToList();
                }

                // Filter by date range
                if (options.ValidFrom.HasValue)
                {
                    priceRecords = priceRecords.Where(pr => pr.ValidFrom >= options.ValidFrom || pr.DateRecorded >= options.ValidFrom).ToList();
                }

                if (options.ValidTo.HasValue)
                {
                    priceRecords = priceRecords.Where(pr => pr.ValidTo <= options.ValidTo || pr.DateRecorded <= options.ValidTo).ToList();
                }

                // Filter by price range
                if (options.MinPrice.HasValue)
                {
                    priceRecords = priceRecords.Where(pr => pr.Price >= options.MinPrice.Value).ToList();
                }

                if (options.MaxPrice.HasValue)
                {
                    priceRecords = priceRecords.Where(pr => pr.Price <= options.MaxPrice.Value).ToList();
                }

                // Only on sale items
                if (options.OnlyOnSale)
                {
                    priceRecords = priceRecords.Where(pr => pr.IsOnSale).ToList();
                }

                // Create export items for each price record
                foreach (var priceRecord in priceRecords)
                {
                    places.TryGetValue(priceRecord.PlaceId, out var place);
                    
                    var exportItem = new ExportItem
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Brand = item.Brand,
                        Category = item.Category,
                        SubCategory = item.SubCategory,
                        Barcode = item.Barcode,
                        PackageSize = item.PackageSize,
                        Unit = item.Unit,
                        Price = priceRecord.Price,
                        OriginalPrice = priceRecord.OriginalPrice,
                        PriceUnit = item.Unit ?? "ea",
                        IsOnSale = priceRecord.IsOnSale,
                        SaleDescription = priceRecord.SaleDescription,
                        ValidFrom = priceRecord.ValidFrom,
                        ValidTo = priceRecord.ValidTo,
                        Store = place?.Name ?? "Unknown",
                        StoreId = priceRecord.PlaceId,
                        StoreChain = place?.Chain,
                        DateRecorded = priceRecord.DateRecorded,
                        Source = priceRecord.Source,
                        Notes = priceRecord.Notes
                    };

                    exportItems.Add(exportItem);
                }

                processedCount++;
                if (processedCount % 10 == 0)
                {
                    var percentage = 10 + (int)((double)processedCount / totalCount * 70);
                    progress?.Report(new ExportProgress { Percentage = percentage, Status = $"Processing {processedCount}/{totalCount}..." });
                }
            }
        });

        return new ExportData
        {
            ExportVersion = "1.0",
            ExportDate = DateTime.UtcNow,
            Source = "AdvGenPriceComparer",
            Location = new ExportLocation
            {
                Suburb = options.LocationSuburb ?? "Unknown",
                State = options.LocationState ?? "Unknown",
                Country = options.LocationCountry ?? "Australia"
            },
            ExportOptions = new ExportOptionsSummary
            {
                Category = options.Category,
                StoreIds = options.StoreIds,
                DateRange = options.ValidFrom.HasValue || options.ValidTo.HasValue 
                    ? $"{options.ValidFrom:yyyy-MM-dd} to {options.ValidTo:yyyy-MM-dd}" 
                    : "All dates",
                OnlyOnSale = options.OnlyOnSale
            },
            Statistics = new ExportStatistics
            {
                TotalItems = items.Count,
                TotalPriceRecords = exportItems.Count,
                UniqueStores = exportItems.Select(e => e.StoreId).Distinct().Count(),
                Categories = exportItems.Where(e => !string.IsNullOrEmpty(e.Category)).Select(e => e.Category!).Distinct().ToList(),
                DateRange = exportItems.Any() 
                    ? $"{exportItems.Min(e => e.DateRecorded):yyyy-MM-dd} to {exportItems.Max(e => e.DateRecorded):yyyy-MM-dd}"
                    : "N/A"
            },
            Items = exportItems.OrderBy(e => e.Category).ThenBy(e => e.Name).ToList()
        };
    }
}

/// <summary>
/// Options for exporting data
/// </summary>
public class ExportOptions
{
    // Filters
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public List<string>? StoreIds { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime? LastUpdatedAfter { get; set; }
    public DateTime? LastUpdatedBefore { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool OnlyOnSale { get; set; } = false;
    public bool ActiveOnly { get; set; } = true;

    // Location info for export metadata
    public string? LocationSuburb { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; } = "Australia";
}

/// <summary>
/// Result of export operation
/// </summary>
public class ExportResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public int ItemsExported { get; set; }
    public string? FilePath { get; set; }
    public long FileSizeBytes { get; set; }
    public double CompressionRatio { get; set; } = 1.0;
}

/// <summary>
/// Progress information for export operations
/// </summary>
public class ExportProgress
{
    public int Percentage { get; set; }
    public string? Status { get; set; }
}

/// <summary>
/// Root export data structure
/// </summary>
public class ExportData
{
    public string ExportVersion { get; set; } = "1.0";
    public DateTime ExportDate { get; set; }
    public string Source { get; set; } = "AdvGenPriceComparer";
    public ExportLocation Location { get; set; } = new();
    public ExportOptionsSummary ExportOptions { get; set; } = new();
    public ExportStatistics Statistics { get; set; } = new();
    public List<ExportItem> Items { get; set; } = new();
}

/// <summary>
/// Location information for export
/// </summary>
public class ExportLocation
{
    public string Suburb { get; set; } = "Unknown";
    public string State { get; set; } = "Unknown";
    public string Country { get; set; } = "Australia";
}

/// <summary>
/// Summary of export options used
/// </summary>
public class ExportOptionsSummary
{
    public string? Category { get; set; }
    public List<string>? StoreIds { get; set; }
    public string? DateRange { get; set; }
    public bool OnlyOnSale { get; set; }
}

/// <summary>
/// Statistics about the exported data
/// </summary>
public class ExportStatistics
{
    public int TotalItems { get; set; }
    public int TotalPriceRecords { get; set; }
    public int UniqueStores { get; set; }
    public List<string> Categories { get; set; } = new();
    public string? DateRange { get; set; }
}

/// <summary>
/// Individual item in export
/// </summary>
public class ExportItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Barcode { get; set; }
    public string? PackageSize { get; set; }
    public string? Unit { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string PriceUnit { get; set; } = "ea";
    public bool IsOnSale { get; set; }
    public string? SaleDescription { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Store { get; set; }
    public string? StoreId { get; set; }
    public string? StoreChain { get; set; }
    public DateTime DateRecorded { get; set; }
    public string? Source { get; set; }
    public string? Notes { get; set; }
}
