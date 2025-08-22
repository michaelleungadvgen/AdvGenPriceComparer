using System;
using Microsoft.UI.Xaml.Data;

namespace AdvGenPriceComparer.Desktop.WinUI.Converters;

public class PriceFormatConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is decimal price)
        {
            return $"${price:F2}";
        }
        
        if (value is double doublePrice)
        {
            return $"${doublePrice:F2}";
        }
        
        return "$0.00";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}