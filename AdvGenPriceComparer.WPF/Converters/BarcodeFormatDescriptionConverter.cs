using System;
using System.Globalization;
using System.Windows.Data;
using ZXing;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts a BarcodeFormat to a human-readable description
/// </summary>
public class BarcodeFormatDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is BarcodeFormat format)
        {
            return format switch
            {
                BarcodeFormat.EAN_13 => "13-digit European Article Number - Used for retail products worldwide",
                BarcodeFormat.EAN_8 => "8-digit EAN for small packages",
                BarcodeFormat.UPC_A => "12-digit Universal Product Code - Common in North America",
                BarcodeFormat.UPC_E => "6-digit compressed UPC for small items",
                BarcodeFormat.CODE_128 => "High-density alphanumeric code for shipping and logistics",
                BarcodeFormat.CODE_39 => "Alphanumeric code used in industrial and defense applications",
                BarcodeFormat.QR_CODE => "2D QR Code for URLs, payments, and product info",
                BarcodeFormat.DATA_MATRIX => "2D code for small parts and electronics",
                BarcodeFormat.PDF_417 => "2D stacked barcode for IDs and documents",
                BarcodeFormat.AZTEC => "2D code for transportation tickets",
                _ => $"{format} barcode format"
            };
        }
        return "Select a barcode format";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
