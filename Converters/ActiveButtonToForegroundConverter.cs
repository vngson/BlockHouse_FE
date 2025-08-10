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
                ? Brushes.Black
                : Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}