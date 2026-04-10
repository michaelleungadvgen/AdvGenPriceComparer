using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converter to change error count color based on value
/// </summary>
public class ErrorCountToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int count && count > 0)
        {
            return Brushes.Red;
        }
        return Brushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}
