using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using AdvGenPriceComparer.WPF.Chat.Models;

namespace AdvGenPriceComparer.WPF.Converters
{
    public class MessageRoleToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageRole role && parameter is string targetRole)
            {
                return role.ToString() == targetRole ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
