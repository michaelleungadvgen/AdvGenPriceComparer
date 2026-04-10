using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AdvGenPriceComparer.WPF.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public Brush TrueBrush { get; set; } = new SolidColorBrush(Color.FromRgb(200, 230, 201)); // Light green
        public Brush FalseBrush { get; set; } = new SolidColorBrush(Color.FromRgb(255, 205, 210)); // Light red

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
            // One-way converter - no conversion back needed
            return Binding.DoNothing;
        }
    }
}
