namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents a store location shared across the network
/// </summary>
public class SharedPlace
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique store identifier (from client)
    /// </summary>
    public string StoreId { get; set; } = string.Empty;

    /// <summary>
    /// Store name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Store chain (Coles, Woolworths, etc.)
    /// </summary>
    public string? Chain { get; set; }

    /// <summary>
    /// Street address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Suburb
    /// </summary>
    public string? Suburb { get; set; }

    /// <summary>
    /// State/Province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Postal code
    /// </summary>
    public string? Postcode { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = "Australia";

    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// When the place was first added
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Related price records
    /// </summary>
    public ICollection<SharedPriceRecord> PriceRecords { get; set; } = new List<SharedPriceRecord>();
}
