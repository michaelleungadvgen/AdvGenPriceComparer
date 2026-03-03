using System;

namespace Microsoft.UI.Xaml
{
    public enum Visibility
    {
        Visible = 0,
        Collapsed = 1
    }
}

namespace Microsoft.UI.Xaml.Data
{
    public interface IValueConverter
    {
        object Convert(object value, Type targetType, object parameter, string language);
        object ConvertBack(object value, Type targetType, object parameter, string language);
    }
}
