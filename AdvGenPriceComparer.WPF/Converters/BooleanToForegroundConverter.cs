using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvGenPriceComparer.WPF.Converters
{
    public class BooleanToForegroundConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Color.FromRgb(27, 94, 32)); // Dark green
        public Brush FalseBrush { get; set; } = new SolidColorBrush(Color.FromRgb(183, 28, 28)); // Dark red

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isAvailable)
            {
                return isAvailable ? TrueBrush : FalseBrush;
            }
            return FalseBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
