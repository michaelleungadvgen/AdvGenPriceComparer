namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents a price record shared across the network
/// </summary>
public class SharedPriceRecord
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Reference to the item
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// Navigation property to item
    /// </summary>
    public SharedItem Item { get; set; } = null!;

    /// <summary>
    /// Reference to the place/store
    /// </summary>
    public int PlaceId { get; set; }

    /// <summary>
    /// Navigation property to place
    /// </summary>
    public SharedPlace Place { get; set; } = null!;

    /// <summary>
    /// Current price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Original price (before discount)
    /// </summary>
    public decimal? OriginalPrice { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "AUD";

    /// <summary>
    /// Special offer type
    /// </summary>
    public string? SpecialType { get; set; }

    /// <summary>
    /// When the price is valid from
    /// </summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>
    /// When the price is valid until
    /// </summary>
    public DateTime? ValidUntil { get; set; }

    /// <summary>
    /// When the price was recorded
    /// </summary>
    public DateTime DateRecorded { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Whether this is the current price
    /// </summary>
    public bool IsCurrent { get; set; } = true;

    /// <summary>
    /// Client version that submitted this record
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// Source of the price data
    /// </summary>
    public string? Source { get; set; }
}
