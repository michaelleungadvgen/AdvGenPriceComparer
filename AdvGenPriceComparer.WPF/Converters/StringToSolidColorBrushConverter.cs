using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts a hex color string to a SolidColorBrush
/// </summary>
public class StringToSolidColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string hexColor)
        {
            try
            {
                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
                brush.Freeze();
                return brush;
            }
            catch
            {
                return new SolidColorBrush(Colors.Gray);
            }
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
