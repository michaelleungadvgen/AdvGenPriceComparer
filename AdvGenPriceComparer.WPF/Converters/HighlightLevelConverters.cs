using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AdvGenPriceComparer.WPF.Services;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts PriceHighlightLevel to display text
/// </summary>
public class HighlightLevelToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PriceHighlightLevel level)
        {
            return level switch
            {
                PriceHighlightLevel.BestPrice => "🔥 BEST",
                PriceHighlightLevel.GreatDeal => "⭐ GREAT",
                PriceHighlightLevel.GoodDeal => "✓ GOOD",
                _ => string.Empty
            };
        }
        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts PriceHighlightLevel to color brush
/// </summary>
public class HighlightLevelToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PriceHighlightLevel level)
        {
            return level switch
            {
                PriceHighlightLevel.BestPrice => new SolidColorBrush(Colors.Goldenrod),
                PriceHighlightLevel.GreatDeal => new SolidColorBrush(Colors.Green),
                PriceHighlightLevel.GoodDeal => new SolidColorBrush(Colors.DodgerBlue),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts PriceHighlightLevel to background color brush
/// </summary>
public class HighlightLevelToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is PriceHighlightLevel level)
        {
            return level switch
            {
                PriceHighlightLevel.BestPrice => new SolidColorBrush(Color.FromArgb(30, 255, 215, 0)), // Light gold
                PriceHighlightLevel.GreatDeal => new SolidColorBrush(Color.FromArgb(30, 76, 175, 80)),  // Light green
                PriceHighlightLevel.GoodDeal => new SolidColorBrush(Color.FromArgb(30, 33, 150, 243)),  // Light blue
                _ => Brushes.Transparent
            };
        }
        return Brushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
