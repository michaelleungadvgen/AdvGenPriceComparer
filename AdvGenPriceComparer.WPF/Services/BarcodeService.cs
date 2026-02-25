using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SkiaSharp;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace AdvGenPriceComparer.WPF.Services;

/// <summary>
/// Service for barcode scanning and generation using ZXing.Net
/// </summary>
public class BarcodeService : IBarcodeService
{
    private readonly ILoggerService _logger;
    private readonly BarcodeReader<SKBitmap> _barcodeReader;

    public BarcodeService(ILoggerService logger)
    {
        _logger = logger;
        
        // Initialize barcode reader with SkiaSharp binding for .NET 9 compatibility
        _barcodeReader = new BarcodeReader<SKBitmap>(
            bitmap => new SKBitmapLuminanceSource(bitmap))
        {
            AutoRotate = true,
            Options = new DecodingOptions
            {
                TryHarder = true,
                TryInverted = true,
                PossibleFormats = new List<BarcodeFormat>
                {
                    BarcodeFormat.EAN_13,
                    BarcodeFormat.EAN_8,
                    BarcodeFormat.UPC_A,
                    BarcodeFormat.UPC_E,
                    BarcodeFormat.CODE_128,
                    BarcodeFormat.CODE_39,
                    BarcodeFormat.QR_CODE,
                    BarcodeFormat.DATA_MATRIX
                }
            }
        };

        _logger.LogInfo("BarcodeService initialized with ZXing.Net");
    }

    /// <inheritdoc />
    public async Task<BarcodeDecodeResult?> DecodeFromImageAsync(string imagePath)
    {
        try
        {
            if (!File.Exists(imagePath))
            {
                _logger.LogWarning($"Image file not found: {imagePath}");
                return null;
            }

            _logger.LogInfo($"Decoding barcode from image: {imagePath}");

            // Read image file
            var imageBytes = await File.ReadAllBytesAsync(imagePath);
            return await DecodeFromBytesAsync(imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to decode barcode from image: {imagePath}", ex);
            return null;
        }
    }

    /// <inheritdoc />
    public Task<BarcodeDecodeResult?> DecodeFromBytesAsync(byte[] imageBytes)
    {
        try
        {
            // Load image using SkiaSharp
            using var bitmap = SKBitmap.Decode(imageBytes);
            if (bitmap == null)
            {
                _logger.LogWarning("Failed to decode image bytes");
                return Task.FromResult<BarcodeDecodeResult?>(null);
            }

            // Decode barcode
            var result = _barcodeReader.Decode(bitmap);

            if (result == null)
            {
                _logger.LogInfo("No barcode found in image");
                return Task.FromResult<BarcodeDecodeResult?>(null);
            }

            _logger.LogInfo($"Barcode decoded: {result.Text} (Format: {result.BarcodeFormat})");

            return Task.FromResult<BarcodeDecodeResult?>(new BarcodeDecodeResult
            {
                Text = result.Text,
                Format = result.BarcodeFormat,
                RawBytes = result.RawBytes ?? Array.Empty<byte>(),
                ResultPoints = result.ResultPoints ?? Array.Empty<ResultPoint>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to decode barcode from bytes", ex);
            return Task.FromResult<BarcodeDecodeResult?>(null);
        }
    }

    /// <inheritdoc />
    public Task<byte[]> GenerateBarcodeAsync(string data, BarcodeFormat format, int width, int height)
    {
        try
        {
            _logger.LogInfo($"Generating {format} barcode for: {data}");

            var writer = new BarcodeWriter<SKBitmap>
            {
                Format = format,
                Options = new EncodingOptions
                {
                    Width = width,
                    Height = height,
                    Margin = 10
                },
                Renderer = new ZXing.SkiaSharp.Rendering.SKBitmapRenderer()
            };

            using var bitmap = writer.Write(data);
            
            // Encode to PNG
            using var image = SKImage.FromBitmap(bitmap);
            using var data_png = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data_png.ToArray();

            _logger.LogInfo($"Barcode generated successfully: {width}x{height}px");
            return Task.FromResult(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to generate barcode: {data}", ex);
            throw;
        }
    }

    /// <inheritdoc />
    public List<BarcodeFormatInfo> GetSupportedFormats()
    {
        return new List<BarcodeFormatInfo>
        {
            new()
            {
                Format = BarcodeFormat.EAN_13,
                Name = "EAN-13",
                Description = "European Article Number - 13 digits",
                UseCases = new[] { "Retail products", "Grocery items", "International products" }
            },
            new()
            {
                Format = BarcodeFormat.EAN_8,
                Name = "EAN-8",
                Description = "European Article Number - 8 digits",
                UseCases = new[] { "Small retail products", "Limited space packaging" }
            },
            new()
            {
                Format = BarcodeFormat.UPC_A,
                Name = "UPC-A",
                Description = "Universal Product Code - 12 digits",
                UseCases = new[] { "North American retail", "Grocery items", "Consumer goods" }
            },
            new()
            {
                Format = BarcodeFormat.UPC_E,
                Name = "UPC-E",
                Description = "Universal Product Code - 6 digits (compressed)",
                UseCases = new[] { "Small packages", "Limited space products" }
            },
            new()
            {
                Format = BarcodeFormat.CODE_128,
                Name = "Code 128",
                Description = "High-density alphanumeric code",
                UseCases = new[] { "Shipping labels", "Asset tracking", "Inventory management" }
            },
            new()
            {
                Format = BarcodeFormat.CODE_39,
                Name = "Code 39",
                Description = "Alphanumeric barcode",
                UseCases = new[] { "Industrial applications", "Healthcare", "Defense" }
            },
            new()
            {
                Format = BarcodeFormat.QR_CODE,
                Name = "QR Code",
                Description = "2D barcode with high data capacity",
                UseCases = new[] { "Product information", "URLs", "Digital payments" }
            },
            new()
            {
                Format = BarcodeFormat.DATA_MATRIX,
                Name = "Data Matrix",
                Description = "2D barcode for small items",
                UseCases = new[] { "Electronics", "Small parts", "Pharmaceuticals" }
            }
        };
    }

    /// <inheritdoc />
    public bool ValidateBarcode(string barcode, BarcodeFormat format)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        // Remove any whitespace or hyphens
        barcode = barcode.Replace(" ", "").Replace("-", "");

        return format switch
        {
            BarcodeFormat.EAN_13 => ValidateEAN13(barcode),
            BarcodeFormat.EAN_8 => ValidateEAN8(barcode),
            BarcodeFormat.UPC_A => ValidateUPCA(barcode),
            BarcodeFormat.UPC_E => ValidateUPCE(barcode),
            BarcodeFormat.CODE_128 => barcode.Length <= 128,
            BarcodeFormat.CODE_39 => barcode.All(c => IsCode39Valid(c)),
            BarcodeFormat.QR_CODE => barcode.Length <= 4296,
            BarcodeFormat.DATA_MATRIX => barcode.Length <= 3116,
            _ => true
        };
    }

    private bool ValidateEAN13(string barcode)
    {
        if (barcode.Length != 13 || !barcode.All(char.IsDigit))
            return false;

        // Validate check digit
        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[12] - '0');
    }

    private bool ValidateEAN8(string barcode)
    {
        if (barcode.Length != 8 || !barcode.All(char.IsDigit))
            return false;

        // Validate check digit
        int sum = 0;
        for (int i = 0; i < 7; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit * 3 : digit;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[7] - '0');
    }

    private bool ValidateUPCA(string barcode)
    {
        // UPC-A is essentially EAN-13 with leading zero
        if (barcode.Length != 12 || !barcode.All(char.IsDigit))
            return false;

        // Validate check digit
        int sum = 0;
        for (int i = 0; i < 11; i++)
        {
            int digit = barcode[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        return checkDigit == (barcode[11] - '0');
    }

    private bool ValidateUPCE(string barcode)
    {
        return barcode.Length == 6 && barcode.All(char.IsDigit);
    }

    private bool IsCode39Valid(char c)
    {
        // Code 39 supports digits, uppercase letters, and some special characters
        const string validChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-. $/+%";
        return validChars.Contains(c);
    }
}
