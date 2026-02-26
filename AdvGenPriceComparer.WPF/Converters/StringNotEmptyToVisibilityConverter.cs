using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts a string to Visibility - Visible if not null or empty, Collapsed otherwise
/// </summary>
public class StringNotEmptyToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return string.IsNullOrWhiteSpace(str) ? Visibility.Collapsed : Visibility.Visible;
        }
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
