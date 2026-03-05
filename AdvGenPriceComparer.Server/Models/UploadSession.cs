namespace AdvGenPriceComparer.Server.Models;

/// <summary>
/// Represents an upload session from a client
/// </summary>
public class UploadSession
{
    /// <summary>
    /// Internal database ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// API key used for the upload
    /// </summary>
    public int ApiKeyId { get; set; }

    /// <summary>
    /// Navigation property to API key
    /// </summary>
    public ApiKey ApiKey { get; set; } = null!;

    /// <summary>
    /// Number of items uploaded
    /// </summary>
    public int ItemsUploaded { get; set; }

    /// <summary>
    /// Number of places uploaded
    /// </summary>
    public int PlacesUploaded { get; set; }

    /// <summary>
    /// Number of price records uploaded
    /// </summary>
    public int PricesUploaded { get; set; }

    /// <summary>
    /// When the upload occurred
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Client version
    /// </summary>
    public string? ClientVersion { get; set; }

    /// <summary>
    /// Client IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Whether the upload was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Error message if upload failed
    /// </summary>
    public string? ErrorMessage { get; set; }
}
