using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BlockHouse.Converters
{
    public class ActiveButtonToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value?.ToString() == parameter?.ToString())
            {
                var brush = new LinearGradientBrush(
                    Color.FromArgb(30, 201, 168, 76),
                    Color.FromArgb(18, 201, 168, 76),
                    new Point(0, 0), new Point(1, 1));
                return brush;
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
