namespace Microsoft.UI.Xaml.Data
{
    public interface IValueConverter
    {
        object Convert(object value, System.Type targetType, object parameter, string language);
        object ConvertBack(object value, System.Type targetType, object parameter, string language);
    }
}
