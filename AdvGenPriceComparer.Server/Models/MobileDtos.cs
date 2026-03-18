namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Mobile-optimized dashboard summary
/// </summary>
public class MobileDashboardSummary
{
    public int TotalItems { get; set; }
    public int TotalStores { get; set; }
    public int ActiveDeals { get; set; }
    public double AverageSavings { get; set; }
    public List<MobilePriceUpdate> RecentUpdates { get; set; } = new();
    public List<MobileDeal> BestDealsToday { get; set; } = new();
}

/// <summary>
/// Mobile price update notification
/// </summary>
public class MobilePriceUpdate
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public decimal OldPrice { get; set; }
    public decimal NewPrice { get; set; }
    public double ChangePercent { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Mobile-optimized deal information
/// </summary>
public class MobileDeal
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Size { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public decimal? Savings { get; set; }
    public double SavingsPercent { get; set; }
    public string? SpecialType { get; set; }
    public DateTime? ValidUntil { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// Mobile price check result
/// </summary>
public class MobilePriceCheckResult
{
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Barcode { get; set; }
    public List<MobileStorePrice> Prices { get; set; } = new();
    public MobileBestPrice? BestPrice { get; set; }
}

/// <summary>
/// Store price information for mobile
/// </summary>
public class MobileStorePrice
{
    public int StoreId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public decimal Price { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? SpecialType { get; set; }
    public DateTime? ValidUntil { get; set; }
    public double? Distance { get; set; }
}

/// <summary>
/// Best price information
/// </summary>
public class MobileBestPrice
{
    public string StoreName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? Savings { get; set; }
}

/// <summary>
/// Nearby store result with distance
/// </summary>
public class MobileNearbyStore
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Chain { get; set; }
    public string? Address { get; set; }
    public string? Suburb { get; set; }
    public string? State { get; set; }
    public string? Postcode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double Distance { get; set; }
    public string? Bearing { get; set; }
    public int CurrentDeals { get; set; }
}

/// <summary>
/// Compact item for mobile list views
/// </summary>
public class MobileCompactItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Category { get; set; }
    public string? Barcode { get; set; }
    public string? Size { get; set; }
    public decimal? BestPrice { get; set; }
    public string? BestStore { get; set; }
    public decimal? AvgPrice { get; set; }
    public decimal? PriceChange { get; set; }
}

/// <summary>
/// Price history summary for mobile
/// </summary>
public class MobilePriceHistorySummary
{
    public decimal Average30Day { get; set; }
    public decimal Lowest30Day { get; set; }
    public decimal Highest30Day { get; set; }
}

/// <summary>
/// Barcode lookup result
/// </summary>
public class MobileBarcodeResult
{
    public bool Found { get; set; }
    public MobileCompactItem? Item { get; set; }
    public List<MobileStorePrice>? Prices { get; set; }
    public MobileBestPrice? BestDeal { get; set; }
    public MobilePriceHistorySummary? PriceHistory { get; set; }
}

/// <summary>
/// Mobile shopping list
/// </summary>
public class MobileShoppingList
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsFavorite { get; set; }
    public int ItemCount { get; set; }
    public int CompletedCount { get; set; }
    public decimal? TotalEstimatedCost { get; set; }
    public DateTime LastModified { get; set; }
    public List<MobileShoppingListItem> Items { get; set; } = new();
}

/// <summary>
/// Mobile shopping list item
/// </summary>
public class MobileShoppingListItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; } = 1;
    public string? Unit { get; set; }
    public bool IsChecked { get; set; }
    public decimal? EstimatedPrice { get; set; }
    public string? BestStore { get; set; }
}

/// <summary>
/// Request to create/update shopping list
/// </summary>
public class MobileShoppingListRequest
{
    public string? Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<MobileShoppingListItem> Items { get; set; } = new();
}

/// <summary>
/// Shopping list sync request
/// </summary>
public class MobileSyncRequest
{
    public DateTime? ClientLastSync { get; set; }
    public List<MobileSyncListInfo> Lists { get; set; } = new();
}

/// <summary>
/// List info for sync
/// </summary>
public class MobileSyncListInfo
{
    public string Id { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Shopping list sync response
/// </summary>
public class MobileSyncResponse
{
    public DateTime ServerTime { get; set; }
    public List<MobileSyncAction> ListsToUpdate { get; set; } = new();
    public List<string> ListsToDelete { get; set; } = new();
}

/// <summary>
/// Sync action for a list
/// </summary>
public class MobileSyncAction
{
    public string Id { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // "update", "delete"
    public MobileShoppingList? Data { get; set; }
}

/// <summary>
/// Mobile price alert
/// </summary>
public class MobilePriceAlert
{
    public string Id { get; set; } = string.Empty;
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public decimal TargetPrice { get; set; }
    public decimal? CurrentPrice { get; set; }
    public string Condition { get; set; } = "Below"; // "Below", "Above"
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request to create price alert
/// </summary>
public class MobilePriceAlertRequest
{
    public int ItemId { get; set; }
    public decimal TargetPrice { get; set; }
    public string Condition { get; set; } = "Below";
}

/// <summary>
/// Push notification registration request
/// </summary>
public class MobilePushRegistrationRequest
{
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty; // "android", "ios"
    public string DeviceId { get; set; } = string.Empty;
    public MobileNotificationPreferences Preferences { get; set; } = new();
}

/// <summary>
/// Notification preferences
/// </summary>
public class MobileNotificationPreferences
{
    public bool PriceDrops { get; set; } = true;
    public bool DealAlerts { get; set; } = true;
    public bool WeeklyDigest { get; set; } = false;
}

/// <summary>
/// Push unregistration request
/// </summary>
public class MobilePushUnregistrationRequest
{
    public string DeviceToken { get; set; } = string.Empty;
}

/// <summary>
/// Generic API response wrapper
/// </summary>
public class MobileApiResponse<T>
{
    public bool Success { get; set; } = true;
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int? TotalCount { get; set; }
}

/// <summary>
/// Error response
/// </summary>
public class MobileApiError
{
    public string Error { get; set; } = string.Empty;
    public string? Details { get; set; }
}
