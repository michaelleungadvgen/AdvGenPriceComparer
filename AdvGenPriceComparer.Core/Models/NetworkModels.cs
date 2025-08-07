using LiteDB;

namespace AdvGenPriceComparer.Core.Models;

public enum MessageType
{
    PriceShare,
    PriceRequest,
    ItemShare,
    PlaceShare,
    SyncRequest,
    SyncResponse,
    Heartbeat,
    Error
}

public class NetworkMessage
{
    public MessageType Type { get; set; }
    public string? SenderId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? Data { get; set; }
    public string? Signature { get; set; } // For future authentication
}

public class PriceShareMessage
{
    public string? ItemName { get; set; }
    public string? ItemBrand { get; set; }
    public string? ItemCategory { get; set; }
    public string? ItemBarcode { get; set; }
    public string? PackageSize { get; set; }
    
    public string? StoreName { get; set; }
    public string? StoreChain { get; set; }
    public string? StoreSuburb { get; set; }
    public string? StoreState { get; set; }
    
    public decimal Price { get; set; }
    public bool IsOnSale { get; set; }
    public decimal? OriginalPrice { get; set; }
    public string? SaleDescription { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? Source { get; set; } = "p2p";
}

public class ServerInfo
{
    public string? Name { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public bool IsSecure { get; set; } = false;
    public string? Region { get; set; } // AU, NSW, VIC, etc.
    public bool IsActive { get; set; } = true;
    public DateTime LastSeen { get; set; } = DateTime.MinValue;
    public string? Description { get; set; }
}

public class NetworkPeer
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Host { get; set; }
    public int Port { get; set; }
    public DateTime LastSeen { get; set; }
    public bool IsConnected { get; set; }
    public string? Version { get; set; }
    public string? Region { get; set; }
}

public class SyncRequestMessage
{
    public DateTime LastSyncTime { get; set; }
    public string[]? Categories { get; set; }
    public string[]? Chains { get; set; }
    public string? Region { get; set; }
}

public class SyncResponseMessage
{
    public PriceShareMessage[]? Prices { get; set; }
    public int TotalCount { get; set; }
    public DateTime SyncTime { get; set; }
}