using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows.Data;

namespace AdvGenPriceComparer.WPF.Converters;

/// <summary>
/// Converts enum values to their description attributes or readable names
/// </summary>
public class EnumDescriptionConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        var type = value.GetType();
        var memberInfo = type.GetMember(value.ToString()!);
        
        if (memberInfo.Length > 0)
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }

        // Fallback: convert PascalCase to spaced words
        return AddSpacesToSentence(value.ToString()!, true);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }

    private static string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var newText = new System.Text.StringBuilder(text.Length * 2);
        newText.Append(text[0]);

        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
            {
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                {
                    newText.Append(' ');
                }
            }
            newText.Append(text[i]);
        }

        return newText.ToString();
    }
}

/// <summary>
/// Converts int values to visibility (visible if > 0)
/// </summary>
public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int intValue)
        {
            return intValue > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}

/// <summary>
/// Converts decimal values to visibility (visible if > 0)
/// </summary>
public class DecimalToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue > 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // One-way converter - no conversion back needed
        return Binding.DoNothing;
    }
}
