using System;
using Microsoft.UI.Xaml.Data;

namespace AdvGenPriceComparer.Desktop.WinUI.Converters;

public class PercentageFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal percentage)
        {
            return $"{percentage:F1}%";
        }
        
        if (value is double doublePercentage)
        {
            return $"{doublePercentage:F1}%";
        }
        
        return "0.0%";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}