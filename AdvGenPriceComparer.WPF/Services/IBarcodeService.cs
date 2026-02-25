using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZXing;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service interface for barcode scanning and generation operations
/// </summary>
public interface IBarcodeService
{
    /// <summary>
    /// Decodes a barcode from an image file
    /// </summary>
    /// <param name="imagePath">Path to the image file containing the barcode</param>
    /// <returns>Barcode decode result, or null if no barcode found</returns>
    Task<BarcodeDecodeResult?> DecodeFromImageAsync(string imagePath);

    /// <summary>
    /// Decodes a barcode from image bytes
    /// </summary>
    /// <param name="imageBytes">Byte array of the image</param>
    /// <returns>Barcode decode result, or null if no barcode found</returns>
    Task<BarcodeDecodeResult?> DecodeFromBytesAsync(byte[] imageBytes);

    /// <summary>
    /// Generates a barcode image for a product
    /// </summary>
    /// <param name="data">The data to encode</param>
    /// <param name="format">Barcode format</param>
    /// <param name="width">Image width</param>
    /// <param name="height">Image height</param>
    /// <returns>Generated barcode as byte array</returns>
    Task<byte[]> GenerateBarcodeAsync(string data, BarcodeFormat format, int width, int height);

    /// <summary>
    /// Gets a list of supported barcode formats for grocery products
    /// </summary>
    /// <returns>List of supported formats</returns>
    List<BarcodeFormatInfo> GetSupportedFormats();

    /// <summary>
    /// Validates if a barcode is valid for a specific format
    /// </summary>
    /// <param name="barcode">The barcode text</param>
    /// <param name="format">The expected format</param>
    /// <returns>True if valid</returns>
    bool ValidateBarcode(string barcode, BarcodeFormat format);
}

/// <summary>
/// Result of a barcode decode operation
/// </summary>
public class BarcodeDecodeResult
{
    /// <summary>
    /// The decoded barcode text
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// The barcode format
    /// </summary>
    public BarcodeFormat Format { get; set; }

    /// <summary>
    /// Raw bytes of the barcode
    /// </summary>
    public byte[] RawBytes { get; set; } = System.Array.Empty<byte>();

    /// <summary>
    /// Points where the barcode was detected in the image
    /// </summary>
    public ZXing.ResultPoint[] ResultPoints { get; set; } = System.Array.Empty<ZXing.ResultPoint>();
}

/// <summary>
/// Information about a barcode format
/// </summary>
public class BarcodeFormatInfo
{
    /// <summary>
    /// The barcode format
    /// </summary>
    public BarcodeFormat Format { get; set; }

    /// <summary>
    /// Display name of the format
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of the format
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Typical use cases for this format
    /// </summary>
    public string[] UseCases { get; set; } = System.Array.Empty<string>();
}
