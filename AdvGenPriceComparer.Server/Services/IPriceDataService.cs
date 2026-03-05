using AdvGenPriceComparer.Server.Models;

namespace AdvGenPriceComparer.Server.Services;

/// <summary>
/// Service interface for managing shared price data
/// </summary>
public interface IPriceDataService
{
    /// <summary>
    /// Get items with optional filtering
    /// </summary>
    Task<IEnumerable<SharedItem>> GetItemsAsync(ItemFilter? filter = null, int page = 1, int pageSize = 100);

    /// <summary>
    /// Get a single item by ID
    /// </summary>
    Task<SharedItem?> GetItemByIdAsync(int id);

    /// <summary>
    /// Get a single item by product ID
    /// </summary>
    Task<SharedItem?> GetItemByProductIdAsync(string productId);

    /// <summary>
    /// Search items by name
    /// </summary>
    Task<IEnumerable<SharedItem>> SearchItemsAsync(string query, int limit = 20);

    /// <summary>
    /// Create or update an item
    /// </summary>
    Task<SharedItem> UpsertItemAsync(SharedItem item);

    /// <summary>
    /// Get places with optional filtering
    /// </summary>
    Task<IEnumerable<SharedPlace>> GetPlacesAsync(PlaceFilter? filter = null, int page = 1, int pageSize = 100);

    /// <summary>
    /// Get a single place by ID
    /// </summary>
    Task<SharedPlace?> GetPlaceByIdAsync(int id);

    /// <summary>
    /// Create or update a place
    /// </summary>
    Task<SharedPlace> UpsertPlaceAsync(SharedPlace place);

    /// <summary>
    /// Get price records with optional filtering
    /// </summary>
    Task<IEnumerable<SharedPriceRecord>> GetPriceRecordsAsync(PriceFilter? filter = null, int page = 1, int pageSize = 100);

    /// <summary>
    /// Get current price for an item at a specific place
    /// </summary>
    Task<SharedPriceRecord?> GetCurrentPriceAsync(int itemId, int placeId);

    /// <summary>
    /// Create a new price record
    /// </summary>
    Task<SharedPriceRecord> CreatePriceRecordAsync(SharedPriceRecord record);

    /// <summary>
    /// Get latest deals across all stores
    /// </summary>
    Task<IEnumerable<SharedPriceRecord>> GetLatestDealsAsync(int limit = 50);

    /// <summary>
    /// Compare prices for an item across all stores
    /// </summary>
    Task<IEnumerable<SharedPriceRecord>> ComparePricesAsync(int itemId);

    /// <summary>
    /// Get price history for an item
    /// </summary>
    Task<IEnumerable<SharedPriceRecord>> GetPriceHistoryAsync(int itemId, int? placeId = null, DateTime? from = null, DateTime? to = null);

    /// <summary>
    /// Upload batch data from a client
    /// </summary>
    Task<UploadResult> UploadDataAsync(DataUploadRequest request, int apiKeyId);

    /// <summary>
    /// Get server statistics
    /// </summary>
    Task<ServerStats> GetServerStatsAsync();
}

/// <summary>
/// Filter options for item queries
/// </summary>
public class ItemFilter
{
    public string? Category { get; set; }
    public string? Brand { get; set; }
    public string? SearchQuery { get; set; }
}

/// <summary>
/// Filter options for place queries
/// </summary>
public class PlaceFilter
{
    public string? Chain { get; set; }
    public string? State { get; set; }
    public string? SearchQuery { get; set; }
}

/// <summary>
/// Filter options for price queries
/// </summary>
public class PriceFilter
{
    public int? ItemId { get; set; }
    public int? PlaceId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsCurrent { get; set; }
    public decimal? MinDiscount { get; set; }
}

/// <summary>
/// Request for uploading batch data
/// </summary>
public class DataUploadRequest
{
    public List<SharedItem> Items { get; set; } = new();
    public List<SharedPlace> Places { get; set; } = new();
    public List<SharedPriceRecord> PriceRecords { get; set; } = new();
    public string? ClientVersion { get; set; }
}

/// <summary>
/// Result of an upload operation
/// </summary>
public class UploadResult
{
    public bool Success { get; set; }
    public int ItemsUploaded { get; set; }
    public int PlacesUploaded { get; set; }
    public int PricesUploaded { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Server statistics
/// </summary>
public class ServerStats
{
    public int TotalItems { get; set; }
    public int TotalPlaces { get; set; }
    public int TotalPriceRecords { get; set; }
    public int CurrentPrices { get; set; }
    public int ApiKeysActive { get; set; }
    public DateTime ServerTime { get; set; } = DateTime.UtcNow;
}
