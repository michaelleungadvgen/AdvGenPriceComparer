using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdvGenPriceComparer.WPF.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string strValue && parameter is string targetValue)
        {
            return strValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}
