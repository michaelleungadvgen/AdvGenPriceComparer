using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using AdvGenPriceComparer.Core.Interfaces;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for exporting data to static formats for P2P price data sharing.
/// Generates a static data package that can be hosted on web servers or file shares
/// for other peers to discover and import.
/// </summary>
public class StaticDataExporter
{
    private readonly IItemRepository _itemRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IPriceRecordRepository _priceRecordRepository;
    private readonly IExportHistoryRepository _exportHistoryRepository;
    private readonly ILoggerService _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public StaticDataExporter(
        IItemRepository itemRepository,
        IPlaceRepository placeRepository,
        IPriceRecordRepository priceRecordRepository,
        IExportHistoryRepository exportHistoryRepository,
        ILoggerService logger)
    {
        _itemRepository = itemRepository;
        _placeRepository = placeRepository;
        _priceRecordRepository = priceRecordRepository;
        _exportHistoryRepository = exportHistoryRepository;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Export all data to a static package for P2P sharing
    /// </summary>
    public async Task<StaticExportResult> ExportStaticPackageAsync(
        StaticExportOptions options, 
        string outputDirectory,
        IProgress<StaticExportProgress>? progress = null)
    {
        var result = new StaticExportResult();
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInfo($"Starting static data export to {outputDirectory}");
            progress?.Report(new StaticExportProgress { Percentage = 0, Status = "Initializing..." });

            // Create output directory
            Directory.CreateDirectory(outputDirectory);

            // Generate package metadata
            var packageId = Guid.NewGuid().ToString("N")[..16];
            var timestamp = DateTime.UtcNow;
            var version = "1.0";

            progress?.Report(new StaticExportProgress { Percentage = 5, Status = "Exporting stores..." });

            // Export stores
            var storesPath = Path.Combine(outputDirectory, "stores.json");
            var storesCount = await ExportStoresAsync(storesPath);
            result.FilesCreated.Add(storesPath);

            progress?.Report(new StaticExportProgress { Percentage = 25, Status = $"Exported {storesCount} stores..." });

            // Export products
            var productsPath = Path.Combine(outputDirectory, "products.json");
            var productsCount = await ExportProductsAsync(productsPath, options);
            result.FilesCreated.Add(productsPath);

            progress?.Report(new StaticExportProgress { Percentage = 50, Status = $"Exported {productsCount} products..." });

            // Export prices
            var pricesPath = Path.Combine(outputDirectory, "prices.json");
            var pricesCount = await ExportPricesAsync(pricesPath, options);
            result.FilesCreated.Add(pricesPath);

            progress?.Report(new StaticExportProgress { Percentage = 75, Status = $"Exported {pricesCount} price records..." });

            // Generate manifest
            var manifestPath = Path.Combine(outputDirectory, "manifest.json");
            var manifest = new StaticExportManifest
            {
                PackageId = packageId,
                Version = version,
                CreatedAt = timestamp,
                ExportedBy = options.ExportedBy ?? "AdvGenPriceComparer",
                Description = options.Description,
                Location = new ExportLocationInfo
                {
                    Suburb = options.LocationSuburb,
                    State = options.LocationState,
                    Country = options.LocationCountry ?? "Australia"
                },
                DataStats = new DataStats
                {
                    StoreCount = storesCount,
                    ProductCount = productsCount,
                    PriceRecordCount = pricesCount
                },
                Files = new List<ManifestFileEntry>
                {
                    new() { Name = "stores.json", Type = "stores", RecordCount = storesCount },
                    new() { Name = "products.json", Type = "products", RecordCount = productsCount },
                    new() { Name = "prices.json", Type = "prices", RecordCount = pricesCount }
                },
                Filters = options.ToFilterInfo()
            };

            // Calculate checksums for all files
            foreach (var fileEntry in manifest.Files)
            {
                var filePath = Path.Combine(outputDirectory, fileEntry.Name);
                if (File.Exists(filePath))
                {
                    fileEntry.Size = new FileInfo(filePath).Length;
                    fileEntry.Checksum = await CalculateChecksumAsync(filePath);
                }
            }

            var manifestJson = JsonSerializer.Serialize(manifest, _jsonOptions);
            await File.WriteAllTextAsync(manifestPath, manifestJson);
            result.FilesCreated.Add(manifestPath);

            progress?.Report(new StaticExportProgress { Percentage = 90, Status = "Creating package archive..." });

            // Create compressed package if requested
            if (options.CreateCompressedPackage)
            {
                var archivePath = Path.Combine(
                    Path.GetDirectoryName(outputDirectory) ?? outputDirectory, 
                    $"price-data-{timestamp:yyyyMMdd-HHmmss}.zip");
                
                await CreateArchiveAsync(outputDirectory, archivePath);
                result.ArchivePath = archivePath;
                result.ArchiveSize = new FileInfo(archivePath).Length;
            }

            // Generate discovery file for P2P network
            if (options.GenerateDiscoveryFile)
            {
                var discoveryPath = Path.Combine(outputDirectory, "discovery.json");
                await ExportDiscoveryAsync(discoveryPath, manifest, options);
                result.FilesCreated.Add(discoveryPath);
            }

            result.Success = true;
            result.PackageId = packageId;
            result.ExportedAt = timestamp;
            result.TotalStores = storesCount;
            result.TotalProducts = productsCount;
            result.TotalPriceRecords = pricesCount;
            result.OutputDirectory = outputDirectory;
            result.Message = $"Successfully exported static package with {productsCount} products, {storesCount} stores, and {pricesCount} price records";

            _logger.LogInfo(result.Message);
            progress?.Report(new StaticExportProgress { Percentage = 100, Status = "Export complete!" });
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = $"Static export failed: {ex.Message}";
            _logger.LogError(result.ErrorMessage, ex);
            progress?.Report(new StaticExportProgress { Percentage = 0, Status = $"Error: {ex.Message}" });
        }
        finally
        {
            stopwatch.Stop();
            
            // Record export history
            var exportHistory = new ExportHistory
            {
                Id = result.PackageId ?? Guid.NewGuid().ToString(),
                ExportedAt = startTime,
                ExportType = ExportType.StaticPackage,
                StoresExported = result.TotalStores,
                ProductsExported = result.TotalProducts,
                PricesExported = result.TotalPriceRecords,
                TotalSizeBytes = result.ArchiveSize > 0 ? result.ArchiveSize : CalculateDirectorySizeSafe(outputDirectory),
                OutputPath = result.ArchivePath ?? outputDirectory,
                PackageId = result.PackageId,
                Description = options.Description,
                IsSuccessful = result.Success,
                ErrorMessage = result.ErrorMessage,
                Duration = stopwatch.Elapsed,
                FilterCriteria = SerializeFilterCriteria(options)
            };
            
            try
            {
                _exportHistoryRepository.Add(exportHistory);
                _logger.LogInfo($"Export history recorded: {exportHistory.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to record export history: {ex.Message}", ex);
            }
        }

        return result;
    }

    /// <summary>
    /// Calculate directory size safely, returning 0 if directory doesn't exist.
    /// </summary>
    private static long CalculateDirectorySizeSafe(string path)
    {
        if (!Directory.Exists(path))
            return 0;
        
        try
        {
            long size = 0;
            var dir = new DirectoryInfo(path);
            foreach (var file in dir.GetFiles("*", SearchOption.AllDirectories))
            {
                size += file.Length;
            }
            return size;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Serialize filter criteria to a string for history tracking.
    /// </summary>
    private static string? SerializeFilterCriteria(StaticExportOptions options)
    {
        var filters = new List<string>();
        
        if (!string.IsNullOrEmpty(options.Category))
            filters.Add($"Category: {options.Category}");
        if (!string.IsNullOrEmpty(options.Brand))
            filters.Add($"Brand: {options.Brand}");
        if (options.DateFrom.HasValue)
            filters.Add($"From: {options.DateFrom:yyyy-MM-dd}");
        if (options.DateTo.HasValue)
            filters.Add($"To: {options.DateTo:yyyy-MM-dd}");
        if (options.OnlyOnSale)
            filters.Add("OnlyOnSale: true");
        if (options.StoreIds?.Any() == true)
            filters.Add($"Stores: {options.StoreIds.Count}");
        
        return filters.Any() ? string.Join(", ", filters) : null;
    }

    /// <summary>
    /// Export stores to JSON format optimized for static sharing
    /// </summary>
    private async Task<int> ExportStoresAsync(string filePath)
    {
        var places = _placeRepository.GetAll()
            .Where(p => p.IsActive)
            .Select(p => new StaticStoreDto
            {
                Id = p.Id?.ToString() ?? Guid.NewGuid().ToString(),
                Name = p.Name,
                Chain = p.Chain,
                Address = p.Address,
                Suburb = p.Suburb,
                State = p.State,
                Postcode = p.Postcode,
                Country = "Australia",
                Latitude = p.Latitude,
                Longitude = p.Longitude,
                Phone = p.Phone,
                IsActive = p.IsActive
            })
            .ToList();

        var json = JsonSerializer.Serialize(new StaticStoresContainer { Stores = places }, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        
        return places.Count;
    }

    /// <summary>
    /// Export products to JSON format optimized for static sharing
    /// </summary>
    private async Task<int> ExportProductsAsync(string filePath, StaticExportOptions options)
    {
        var query = _itemRepository.GetAll().Where(i => i.IsActive).AsEnumerable();

        // Apply category filter
        if (!string.IsNullOrEmpty(options.Category))
        {
            query = query.Where(i => i.Category?.Equals(options.Category, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Apply brand filter
        if (!string.IsNullOrEmpty(options.Brand))
        {
            query = query.Where(i => i.Brand?.Equals(options.Brand, StringComparison.OrdinalIgnoreCase) == true);
        }

        // Apply date filter
        if (options.LastUpdatedAfter.HasValue)
        {
            query = query.Where(i => i.LastUpdated >= options.LastUpdatedAfter.Value);
        }

        var products = query.Select(i => new StaticProductDto
        {
            Id = i.Id?.ToString() ?? Guid.NewGuid().ToString(),
            Name = i.Name,
            Brand = i.Brand,
            Category = i.Category,
            SubCategory = i.SubCategory,
            Description = i.Description,
            PackageSize = i.PackageSize,
            Unit = i.Unit,
            Barcode = i.Barcode,
            ImageUrl = i.ImageUrl,
            Tags = i.Tags?.Count > 0 ? i.Tags : null,
            IsActive = i.IsActive
        }).ToList();

        var json = JsonSerializer.Serialize(new StaticProductsContainer { Products = products }, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        
        return products.Count;
    }

    /// <summary>
    /// Export price records to JSON format optimized for static sharing
    /// </summary>
    private async Task<int> ExportPricesAsync(string filePath, StaticExportOptions options)
    {
        var query = _priceRecordRepository.GetAll().AsEnumerable();

        // Apply date range filter
        if (options.DateFrom.HasValue)
        {
            query = query.Where(p => p.DateRecorded >= options.DateFrom.Value);
        }

        if (options.DateTo.HasValue)
        {
            query = query.Where(p => p.DateRecorded <= options.DateTo.Value);
        }

        // Apply store filter
        if (options.StoreIds?.Any() == true)
        {
            query = query.Where(p => options.StoreIds.Contains(p.PlaceId));
        }

        // Apply "only on sale" filter
        if (options.OnlyOnSale)
        {
            query = query.Where(p => p.IsOnSale);
        }

        var prices = query.Select(p => new StaticPriceDto
        {
            ProductId = p.ItemId?.ToString() ?? "",
            StoreId = p.PlaceId?.ToString() ?? "",
            Price = p.Price,
            OriginalPrice = p.OriginalPrice,
            Currency = "AUD",
            IsOnSale = p.IsOnSale,
            SaleDescription = p.SaleDescription,
            ValidFrom = p.ValidFrom,
            ValidTo = p.ValidTo,
            RecordedAt = p.DateRecorded,
            Source = p.Source
        }).ToList();

        var json = JsonSerializer.Serialize(new StaticPricesContainer { Prices = prices }, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        
        return prices.Count;
    }

    /// <summary>
    /// Export discovery file for P2P network
    /// </summary>
    private async Task ExportDiscoveryAsync(string filePath, StaticExportManifest manifest, StaticExportOptions options)
    {
        var discovery = new StaticDiscoveryDto
        {
            Type = "static_data_package",
            PackageId = manifest.PackageId,
            Version = manifest.Version,
            CreatedAt = manifest.CreatedAt,
            UpdatedAt = DateTime.UtcNow,
            ExportedBy = options.ExportedBy ?? "AdvGenPriceComparer",
            Description = options.Description ?? "Price data package for P2P sharing",
            Location = options.LocationSuburb != null 
                ? $"{options.LocationSuburb}, {options.LocationState ?? "Australia"}"
                : options.LocationCountry ?? "Australia",
            DataStats = new DiscoveryDataStats
            {
                Stores = manifest.DataStats.StoreCount,
                Products = manifest.DataStats.ProductCount,
                PriceRecords = manifest.DataStats.PriceRecordCount
            },
            Capabilities = new List<string> { "prices", "products", "stores" },
            ManifestUrl = "manifest.json",
            ValidFrom = DateTime.UtcNow,
            ValidTo = options.ValidTo ?? DateTime.UtcNow.AddDays(30)
        };

        var json = JsonSerializer.Serialize(discovery, _jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Create a ZIP archive of the exported data
    /// </summary>
    private async Task CreateArchiveAsync(string sourceDirectory, string archivePath)
    {
        if (File.Exists(archivePath))
        {
            File.Delete(archivePath);
        }

        await Task.Run(() => ZipFile.CreateFromDirectory(sourceDirectory, archivePath, CompressionLevel.Optimal, false));
    }

    /// <summary>
    /// Calculate SHA256 checksum of a file
    /// </summary>
    private async Task<string> CalculateChecksumAsync(string filePath)
    {
        using var stream = File.OpenRead(filePath);
        var hash = await SHA256.HashDataAsync(stream);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }
}

#region DTOs for Static Export

/// <summary>
/// Container for stores export
/// </summary>
public class StaticStoresContainer
{
    public List<StaticStoreDto> Stores { get; set; } = new();
}

/// <summary>
/// Store DTO for static export
/// </summary>
public class StaticStoreDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public string? Address { get; set; }
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Container for products export
/// </summary>
public class StaticProductsContainer
{
    public List<StaticProductDto> Products { get; set; } = new();
}

/// <summary>
/// Product DTO for static export
/// </summary>
public class StaticProductDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Description { get; set; }
    public string? PackageSize { get; set; }
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public List<string>? Tags { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Container for prices export
/// </summary>
public class StaticPricesContainer
{
    public List<StaticPriceDto> Prices { get; set; } = new();
}

/// <summary>
/// Price DTO for static export
/// </summary>
public class StaticPriceDto
{
    public string ProductId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsOnSale { get; set; }
    public string? SaleDescription { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime RecordedAt { get; set; }
    public string? Source { get; set; }
}

/// <summary>
/// Manifest for static export package
/// </summary>
public class StaticExportManifest
{
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ExportLocationInfo? Location { get; set; }
    public DataStats DataStats { get; set; } = new();
    public List<ManifestFileEntry> Files { get; set; } = new();
    public FilterInfo? Filters { get; set; }
}

/// <summary>
/// Location information for export
/// </summary>
public class ExportLocationInfo
{
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// Data statistics for manifest
/// </summary>
public class DataStats
{
    public int StoreCount { get; set; }
    public int ProductCount { get; set; }
    public int PriceRecordCount { get; set; }
}

/// <summary>
/// File entry in manifest
/// </summary>
public class ManifestFileEntry
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Size { get; set; }
    public int RecordCount { get; set; }
    public string Checksum { get; set; } = string.Empty;
}

/// <summary>
/// Filter information for manifest
/// </summary>
public class FilterInfo
{
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public List<string>? StoreIds { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public bool OnlyOnSale { get; set; }
    public DateTime? LastUpdatedAfter { get; set; }
}

/// <summary>
/// Discovery DTO for P2P network
/// </summary>
public class StaticDiscoveryDto
{
    public string Type { get; set; } = string.Empty;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string ExportedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DiscoveryDataStats DataStats { get; set; } = new();
    public List<string> Capabilities { get; set; } = new();
    public string ManifestUrl { get; set; } = string.Empty;
    public DateTime ValidFrom { get; set; }
    public DateTime ValidTo { get; set; }
}

/// <summary>
/// Data stats for discovery
/// </summary>
public class DiscoveryDataStats
{
    public int Stores { get; set; }
    public int Products { get; set; }
    public int PriceRecords { get; set; }
}

#endregion

#region Options and Results

/// <summary>
/// Options for static export
/// </summary>
public class StaticExportOptions
{
    // Filters
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public List<string>? StoreIds { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public DateTime? LastUpdatedAfter { get; set; }
    public bool OnlyOnSale { get; set; } = false;

    // Metadata
    public string? ExportedBy { get; set; }
    public string? Description { get; set; }
    public string? LocationSuburb { get; set; }
    public string? LocationState { get; set; }
    public string? LocationCountry { get; set; } = "Australia";

    // Export options
    public bool CreateCompressedPackage { get; set; } = true;
    public bool GenerateDiscoveryFile { get; set; } = true;
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// Convert options to filter info for manifest
    /// </summary>
    public FilterInfo ToFilterInfo()
    {
        return new FilterInfo
        {
            Category = Category,
            Brand = Brand,
            StoreIds = StoreIds,
            DateFrom = DateFrom,
            DateTo = DateTo,
            OnlyOnSale = OnlyOnSale,
            LastUpdatedAfter = LastUpdatedAfter
        };
    }
}

/// <summary>
/// Result of static export operation
/// </summary>
public class StaticExportResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public string? PackageId { get; set; }
    public DateTime ExportedAt { get; set; }
    public string? OutputDirectory { get; set; }
    public string? ArchivePath { get; set; }
    public long ArchiveSize { get; set; }
    public int TotalStores { get; set; }
    public int TotalProducts { get; set; }
    public int TotalPriceRecords { get; set; }
    public List<string> FilesCreated { get; set; } = new();
}

/// <summary>
/// Progress information for static export operations
/// </summary>
public class StaticExportProgress
{
    public int Percentage { get; set; }
    public string? Status { get; set; }
}

#endregion
