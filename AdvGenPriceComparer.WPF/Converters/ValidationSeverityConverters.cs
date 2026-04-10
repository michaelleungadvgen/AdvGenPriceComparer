using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AdvGenPriceComparer.Core.Models;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts ValidationSeverity to a Brush color
/// </summary>
public class ValidationSeverityToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Critical => new SolidColorBrush(Color.FromRgb(220, 53, 69)),    // Red
                ValidationSeverity.Error => new SolidColorBrush(Color.FromRgb(220, 53, 69)),       // Red
                ValidationSeverity.Warning => new SolidColorBrush(Color.FromRgb(255, 193, 7)),     // Yellow/Orange
                ValidationSeverity.Info => new SolidColorBrush(Color.FromRgb(23, 162, 184)),       // Cyan
                _ => new SolidColorBrush(Color.FromRgb(108, 117, 125))                             // Gray
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts ValidationSeverity to an icon string
/// </summary>
public class ValidationSeverityToIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ValidationSeverity severity)
        {
            return severity switch
            {
                ValidationSeverity.Critical => "❌",
                ValidationSeverity.Error => "❌",
                ValidationSeverity.Warning => "⚠️",
                ValidationSeverity.Info => "ℹ️",
                _ => ""
            };
        }
        return "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts ValidationStatus to a Brush color
/// </summary>
public class ValidationStatusToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ValidationStatus status)
        {
            return status switch
            {
                ValidationStatus.Valid => new SolidColorBrush(Color.FromRgb(40, 167, 69)),           // Green
                ValidationStatus.ValidWithWarnings => new SolidColorBrush(Color.FromRgb(255, 193, 7)), // Yellow
                ValidationStatus.Invalid => new SolidColorBrush(Color.FromRgb(220, 53, 69)),        // Red
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts ValidationEntryType to a display string
/// </summary>
public class ValidationEntryTypeToDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ValidationEntryType entryType)
        {
            return entryType switch
            {
                ValidationEntryType.Store => "Store",
                ValidationEntryType.Product => "Product",
                ValidationEntryType.PriceRecord => "Price",
                ValidationEntryType.Manifest => "Manifest",
                ValidationEntryType.Schema => "Schema",
                ValidationEntryType.Checksum => "Checksum",
                _ => entryType.ToString()
            };
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return Binding.DoNothing;
    }
}
