namespace AdvGenPriceComparer.Application.DTOs;

/// <summary>
/// Request DTO for exporting data
/// </summary>
public class ExportRequestDto
{
    /// <summary>
    /// Output file path
    /// </summary>
    public string OutputPath { get; set; } = string.Empty;

    /// <summary>
    /// Filter options for export
    /// </summary>
    public ExportFilterDto Filters { get; set; } = new();

    /// <summary>
    /// Export format options
    /// </summary>
    public ExportOptionsDto Options { get; set; } = new();
}

/// <summary>
/// Filter options for export
/// </summary>
public class ExportFilterDto
{
    /// <summary>
    /// Filter by date range (items valid from)
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// Filter by date range (items valid to)
    /// </summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>
    /// Filter by store IDs
    /// </summary>
    public List<string>? StoreIds { get; set; }

    /// <summary>
    /// Filter by categories
    /// </summary>
    public List<string>? Categories { get; set; }

    /// <summary>
    /// Filter by minimum price
    /// </summary>
    public decimal? MinPrice { get; set; }

    /// <summary>
    /// Filter by maximum price
    /// </summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>
    /// Only export items with discounts/savings
    /// </summary>
    public bool OnlyDiscountedItems { get; set; }

    /// <summary>
    /// Filter by product name (contains)
    /// </summary>
    public string? ProductNameContains { get; set; }
}

/// <summary>
/// Export format options
/// </summary>
public class ExportOptionsDto
{
    /// <summary>
    /// Include stores in export
    /// </summary>
    public bool IncludeStores { get; set; } = true;

    /// <summary>
    /// Include items/products in export
    /// </summary>
    public bool IncludeItems { get; set; } = true;

    /// <summary>
    /// Include price records in export
    /// </summary>
    public bool IncludePrices { get; set; } = true;

    /// <summary>
    /// Start date for price filter
    /// </summary>
    public DateTime? FromDate { get; set; }

    /// <summary>
    /// End date for price filter
    /// </summary>
    public DateTime? ToDate { get; set; }

    /// <summary>
    /// Include metadata in export
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>
    /// Pretty-print JSON output
    /// </summary>
    public bool PrettyPrint { get; set; } = true;

    /// <summary>
    /// Include price history
    /// </summary>
    public bool IncludePriceHistory { get; set; } = false;

    /// <summary>
    /// Maximum items to export (null = unlimited)
    /// </summary>
    public int? MaxItems { get; set; }
}

/// <summary>
/// Request DTO for incremental export
/// </summary>
public class IncrementalExportRequestDto : ExportRequestDto
{
    /// <summary>
    /// Only export items changed since this date
    /// </summary>
    public DateTime ChangedSince { get; set; }
}

/// <summary>
/// Request DTO for P2P export
/// </summary>
public class P2PExportRequestDto
{
    /// <summary>
    /// Output directory path
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Export filters
    /// </summary>
    public ExportFilterDto Filters { get; set; } = new();

    /// <summary>
    /// Create ZIP archive of exported files
    /// </summary>
    public bool CreateArchive { get; set; } = true;

    /// <summary>
    /// Archive file name (if CreateArchive is true)
    /// </summary>
    public string? ArchiveFileName { get; set; }

    /// <summary>
    /// Include manifest file with checksums
    /// </summary>
    public bool IncludeManifest { get; set; } = true;

    /// <summary>
    /// Include discovery.json for peer advertising
    /// </summary>
    public bool IncludeDiscovery { get; set; } = false;

    /// <summary>
    /// Server information for discovery.json
    /// </summary>
    public DiscoveryInfoDto? DiscoveryInfo { get; set; }
}

/// <summary>
/// Server information for P2P discovery
/// </summary>
public class DiscoveryInfoDto
{
    /// <summary>
    /// Server ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Server type (full_peer or static_peer)
    /// </summary>
    public string Type { get; set; } = "static_peer";

    /// <summary>
    /// Server address
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// Geographic location
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Server description
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Result DTO for P2P export
/// </summary>
public class P2PExportResultDto
{
    /// <summary>
    /// Whether the export was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Output directory path
    /// </summary>
    public string OutputDirectory { get; set; } = string.Empty;

    /// <summary>
    /// Path to the archive file (if created)
    /// </summary>
    public string? ArchivePath { get; set; }

    /// <summary>
    /// Files that were exported
    /// </summary>
    public List<ExportedFileDto> Files { get; set; } = new();

    /// <summary>
    /// Total items exported across all files
    /// </summary>
    public int TotalItemsExported { get; set; }

    /// <summary>
    /// Export manifest with checksums
    /// </summary>
    public ExportManifestDto? Manifest { get; set; }

    /// <summary>
    /// Duration of the export operation
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Human-readable summary message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Information about an exported file
/// </summary>
public class ExportedFileDto
{
    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Full file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSizeBytes { get; set; }

    /// <summary>
    /// Number of items in this file
    /// </summary>
    public int ItemCount { get; set; }

    /// <summary>
    /// File checksum (SHA256)
    /// </summary>
    public string? Checksum { get; set; }
}

/// <summary>
/// Export manifest with metadata and checksums
/// </summary>
public class ExportManifestDto
{
    /// <summary>
    /// Manifest version
    /// </summary>
    public string Version { get; set; } = "1.0";

    /// <summary>
    /// Export timestamp
    /// </summary>
    public DateTime GeneratedAt { get; set; }

    /// <summary>
    /// Source application
    /// </summary>
    public string Source { get; set; } = "AdvGenPriceComparer";

    /// <summary>
    /// Export region/location
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Files in this export
    /// </summary>
    public List<ManifestFileEntryDto> Files { get; set; } = new();
}

/// <summary>
/// Individual file entry in export manifest
/// </summary>
public class ManifestFileEntryDto
{
    /// <summary>
    /// File name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File type (shops, goods, prices, records, discovery)
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// SHA256 checksum
    /// </summary>
    public string Checksum { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Number of records/items
    /// </summary>
    public int RecordCount { get; set; }
}
