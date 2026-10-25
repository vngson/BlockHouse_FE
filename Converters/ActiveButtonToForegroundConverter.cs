using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace BlockHouse.Converters
{
    public class ActiveButtonToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString()
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C9A84C"))
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#78716C"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
