namespace AdvGenPriceComparer.Application.DTOs;

/// <summary>
/// Result of an export operation
/// </summary>
public class ExportResultDto
{
    /// <summary>
    /// Whether the export was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Total number of items exported
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of stores exported
    /// </summary>
    public int StoreCount { get; set; }

    /// <summary>
    /// Number of items/products exported
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// Number of price records exported
    /// </summary>
    public int PriceRecordCount { get; set; }

    /// <summary>
    /// Output file path
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// Duration of the export operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Error messages if any
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Checksum of the exported file (for integrity verification)
    /// </summary>
    public string? Checksum { get; set; }
}

/// <summary>
/// Progress information during export
/// </summary>
public class ExportProgressDto
{
    /// <summary>
    /// Total number of items to export
    /// </summary>
    public int TotalItems { get; set; }

    /// <summary>
    /// Number of items exported so far
    /// </summary>
    public int ExportedItems { get; set; }

    /// <summary>
    /// Current entity being exported (Stores, Items, Prices)
    /// </summary>
    public string? CurrentEntity { get; set; }

    /// <summary>
    /// Progress percentage (0-100)
    /// </summary>
    public int PercentComplete => TotalItems > 0 ? (ExportedItems * 100) / TotalItems : 0;

    /// <summary>
    /// Status message
    /// </summary>
    public string? StatusMessage { get; set; }
}

/// <summary>
/// Standard export format containing all data
/// </summary>
public class StandardExportDto
{
    /// <summary>
    /// Export format version
    /// </summary>
    public string ExportVersion { get; set; } = "1.0";

    /// <summary>
    /// Export timestamp
    /// </summary>
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Source application
    /// </summary>
    public string Source { get; set; } = "AdvGenPriceComparer";

    /// <summary>
    /// Location information
    /// </summary>
    public ExportLocationDto? Location { get; set; }

    /// <summary>
    /// Exported stores
    /// </summary>
    public List<ExportedStoreDto>? Stores { get; set; }

    /// <summary>
    /// Exported items
    /// </summary>
    public List<ExportedItemDto>? Items { get; set; }

    /// <summary>
    /// Exported price records
    /// </summary>
    public List<ExportedPriceDto>? Prices { get; set; }
}

/// <summary>
/// Location information for export
/// </summary>
public class ExportLocationDto
{
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
}

/// <summary>
/// Store export data
/// </summary>
public class ExportedStoreDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public string? Address { get; set; }
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Website { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Item export data
/// </summary>
public class ExportedItemDto
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
    public bool IsActive { get; set; }
}

/// <summary>
/// Price export data
/// </summary>
public class ExportedPriceDto
{
    public string ItemId { get; set; } = string.Empty;
    public string StoreId { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "AUD";
    public bool IsOnSale { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public DateTime RecordedAt { get; set; }
}
