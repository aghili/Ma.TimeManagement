using System.Globalization;
using System.Windows.Data;

namespace Ma.TimeManagement.Converters
{
    public class TrackYConverter : IValueConverter
    {
        public static readonly TrackYConverter Instance = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int index ? index * 90.0 : 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}