using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using AdvGenPriceComparer.Core.Interfaces;

namespace AdvGenPriceComparer.Data.LiteDB.Services;

/// <summary>
/// Service for exporting data to JSON files according to the AdvGenPriceComparer JSON File Format standard.
/// See: https://github.com/michaelleungadvgen/AdvGenPriceComparer/wiki/Json-File-Format
/// </summary>
public class JsonExportService
{
    private readonly IGroceryDataService _dataService;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonExportService(IGroceryDataService dataService)
    {
        _dataService = dataService;
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    #region Shop.json Export

    /// <summary>
    /// Exports all stores to Shop.json format
    /// </summary>
    public void ExportShops(string filePath)
    {
        var shops = _dataService.GetAllPlaces().Select(place => new ShopDto
        {
            ShopID = place.Id,
            ShopName = place.Name,
            Chain = place.Chain,
            Location = new LocationDto
            {
                Address = place.Address,
                Suburb = place.Suburb,
                State = place.State,
                Postcode = place.Postcode,
                Country = "Australia"
            },
            Contact = new ContactDto
            {
                Phone = place.Phone,
                Email = place.Email,
                Website = place.Website
            },
            Coordinates = place.Latitude.HasValue && place.Longitude.HasValue
                ? new CoordinatesDto
                {
                    Latitude = place.Latitude.Value,
                    Longitude = place.Longitude.Value
                }
                : null,
            IsActive = place.IsActive
        }).ToList();

        var json = JsonSerializer.Serialize(shops, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    #endregion

    #region Goods.json Export

    /// <summary>
    /// Exports all items to Goods.json format
    /// </summary>
    public void ExportGoods(string filePath)
    {
        var goods = _dataService.Items.GetAll().Select(item => new GoodsDto
        {
            ProductID = item.Id.ToString(),
            ProductName = item.Name,
            Category = item.Category,
            SubCategory = item.SubCategory,
            Brand = item.Brand,
            Description = item.Description,
            PackageSize = item.PackageSize,
            Unit = item.Unit,
            Barcode = item.Barcode,
            ImageUrl = item.ImageUrl,
            NutritionalInfo = item.NutritionalInfo.Count > 0 ? item.NutritionalInfo : null,
            Allergens = item.Allergens.Count > 0 ? item.Allergens : null,
            DietaryFlags = item.DietaryFlags.Count > 0 ? item.DietaryFlags : null,
            Tags = item.Tags.Count > 0 ? item.Tags : null,
            IsActive = item.IsActive
        }).ToList();

        var json = JsonSerializer.Serialize(goods, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    #endregion

    #region Price Export (price-{timestamp}.json)

    /// <summary>
    /// Exports price records to price-{timestamp}.json format
    /// </summary>
    public void ExportPrices(string filePath, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _dataService.PriceRecords.GetAll().AsEnumerable();

        if (fromDate.HasValue)
            query = query.Where(p => p.DateRecorded >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(p => p.DateRecorded <= toDate.Value);

        var prices = query.Select(record => new PriceDto
        {
            ShopID = record.PlaceId.ToString(),
            ProductID = record.ItemId.ToString(),
            Price = record.Price,
            Currency = "AUD",
            IsOnSale = record.IsOnSale,
            OriginalPrice = record.OriginalPrice,
            DiscountPercentage = record.OriginalPrice.HasValue && record.OriginalPrice.Value > 0
                ? Math.Round((record.OriginalPrice.Value - record.Price) / record.OriginalPrice.Value * 100, 2)
                : null,
            ValidFrom = record.ValidFrom,
            ValidTo = record.ValidTo,
            RecordedAt = record.DateRecorded
        }).ToList();

        var priceFile = new PriceFileDto
        {
            Timestamp = DateTime.UtcNow,
            RecordCount = prices.Count,
            DateRange = new DateRangeDto
            {
                From = fromDate ?? prices.MinBy(p => p.RecordedAt)?.RecordedAt,
                To = toDate ?? prices.MaxBy(p => p.RecordedAt)?.RecordedAt
            },
            Prices = prices
        };

        var json = JsonSerializer.Serialize(priceFile, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    #endregion

    #region Records.json (Manifest)

    /// <summary>
    /// Exports a manifest file listing all available price files
    /// </summary>
    public void ExportRecordsManifest(string filePath, string priceFilesDirectory)
    {
        var priceFiles = Directory.GetFiles(priceFilesDirectory, "price-*.json")
            .Select(f => new PriceFileManifestEntry
            {
                FileName = Path.GetFileName(f),
                FilePath = Path.GetRelativePath(Path.GetDirectoryName(filePath) ?? "", f),
                FileSize = new FileInfo(f).Length,
                CreatedAt = File.GetCreationTimeUtc(f),
                ModifiedAt = File.GetLastWriteTimeUtc(f)
            })
            .OrderByDescending(f => f.CreatedAt)
            .ToList();

        var manifest = new RecordsManifestDto
        {
            GeneratedAt = DateTime.UtcNow,
            PriceFileCount = priceFiles.Count,
            PriceRecords = priceFiles
        };

        var json = JsonSerializer.Serialize(manifest, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    #endregion

    #region Discovery.json Export

    /// <summary>
    /// Exports discovery information for P2P network
    /// </summary>
    public void ExportDiscovery(string filePath, string serverId, string serverAddress, string serverLocation)
    {
        var discovery = new List<DiscoveryDto>
        {
            new DiscoveryDto
            {
                Id = serverId,
                Type = "full_peer",
                Address = serverAddress,
                Location = serverLocation,
                LastSeen = DateTime.UtcNow,
                Description = "AdvGen Price Comparer Server",
                Capabilities = new List<string> { "prices", "shops", "goods" }
            }
        };

        var json = JsonSerializer.Serialize(discovery, _jsonOptions);
        File.WriteAllText(filePath, json);
    }

    #endregion

    #region Export All

    /// <summary>
    /// Exports all data to a directory following the standard format
    /// </summary>
    public ExportResult ExportAll(string outputDirectory)
    {
        try
        {
            Directory.CreateDirectory(outputDirectory);

            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var result = new ExportResult { Success = true };

            // Export Shops
            var shopsPath = Path.Combine(outputDirectory, "Shop.json");
            ExportShops(shopsPath);
            result.FilesCreated.Add(shopsPath);

            // Export Goods
            var goodsPath = Path.Combine(outputDirectory, "Goods.json");
            ExportGoods(goodsPath);
            result.FilesCreated.Add(goodsPath);

            // Export Prices
            var pricesPath = Path.Combine(outputDirectory, $"price-{timestamp}.json");
            ExportPrices(pricesPath);
            result.FilesCreated.Add(pricesPath);

            // Export Records Manifest
            var recordsPath = Path.Combine(outputDirectory, "records.json");
            ExportRecordsManifest(recordsPath, outputDirectory);
            result.FilesCreated.Add(recordsPath);

            result.Message = $"Successfully exported {result.FilesCreated.Count} files to {outputDirectory}";
            return result;
        }
        catch (Exception ex)
        {
            return new ExportResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    #endregion
}

#region DTOs (Data Transfer Objects)

// Shop.json DTOs
public class ShopDto
{
    public string ShopID { get; set; } = string.Empty;
    public string ShopName { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public LocationDto? Location { get; set; }
    public ContactDto? Contact { get; set; }
    public CoordinatesDto? Coordinates { get; set; }
    public bool IsActive { get; set; }
}

public class LocationDto
{
    public string? Address { get; set; }
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
}

public class ContactDto
{
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
}

public class CoordinatesDto
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}

// Goods.json DTOs
public class GoodsDto
{
    public string ProductID { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? SubCategory { get; set; }
    public string? Brand { get; set; }
    public string? Description { get; set; }
    public string? PackageSize { get; set; }
    public string? Unit { get; set; }
    public string? Barcode { get; set; }
    public string? ImageUrl { get; set; }
    public Dictionary<string, decimal>? NutritionalInfo { get; set; }
    public List<string>? Allergens { get; set; }
    public List<string>? DietaryFlags { get; set; }
    public List<string>? Tags { get; set; }
    public bool IsActive { get; set; }
}

// price-{timestamp}.json DTOs
public class PriceFileDto
{
    public DateTime Timestamp { get; set; }
    public int RecordCount { get; set; }
    public DateRangeDto? DateRange { get; set; }
    public List<PriceDto> Prices { get; set; } = new();
}

public class DateRangeDto
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class PriceDto
{
    public string ShopID { get; set; } = string.Empty;
    public string ProductID { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsOnSale { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime RecordedAt { get; set; }
}

// records.json DTOs
public class RecordsManifestDto
{
    public DateTime GeneratedAt { get; set; }
    public int PriceFileCount { get; set; }
    public List<PriceFileManifestEntry> PriceRecords { get; set; } = new();
}

public class PriceFileManifestEntry
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
}

// Discovery.json DTOs
public class DiscoveryDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime LastSeen { get; set; }
    public string? Description { get; set; }
    public List<string>? Capabilities { get; set; }
}

// Export Result
public class ExportResult
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string> FilesCreated { get; set; } = new();
}

#endregion
