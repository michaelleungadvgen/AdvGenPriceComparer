namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents a product item shared across the network
/// </summary>
public class SharedItem
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Unique product identifier (from client)
    /// </summary>
    public string ProductId { get; set; } = string.Empty;

    /// <summary>
    /// Product name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Brand name
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Product category
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Product description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Barcode/EAN
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// Unit of measurement (ea, kg, litre, etc.)
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Package size/quantity
    /// </summary>
    public string? Size { get; set; }

    /// <summary>
    /// When the item was first added to the server
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When the item was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Related price records
    /// </summary>
    public ICollection<SharedPriceRecord> PriceRecords { get; set; } = new List<SharedPriceRecord>();
}
