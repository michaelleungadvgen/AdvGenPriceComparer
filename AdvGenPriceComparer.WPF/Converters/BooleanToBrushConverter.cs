using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts a boolean value to a brush (e.g., for validation status)
/// </summary>
public class BooleanToBrushConverter : IValueConverter
{
    public Brush TrueBrush { get; set; } = new SolidColorBrush(Colors.Green);
    public Brush FalseBrush { get; set; } = new SolidColorBrush(Colors.Red);

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? TrueBrush : FalseBrush;
        }
        return FalseBrush;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
