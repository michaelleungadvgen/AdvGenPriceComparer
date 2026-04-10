using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AdvGenPriceComparer.WPF.Converters;

public class BooleanToOpacityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
        {
            return 0.5; // Grayed out when checked
        }
        return 1.0; // Full opacity when not checked
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}

public class BooleanToStrikethroughConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isChecked && isChecked)
        {
            return System.Windows.TextDecorations.Strikethrough;
        }
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}

public class BooleanToFavoriteConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isFavorite && isFavorite)
        {
            return "⭐"; // Filled star
        }
        return "☆"; // Empty star
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}



public class SelectedItemBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
        {
            return System.Windows.Media.Brushes.White;
        }

        if (value.ToString() == parameter.ToString())
        {
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(227, 242, 253)); // Light blue
        }

        return System.Windows.Media.Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}
