using System.Globalization;
using System.Windows.Data;

namespace Ma.TimeManagement.Converters
{
    public class TimeToXMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2 || values[0] is not DateTime time || values[1] is not double zoom)
                return 0.0;

            var dayStart = DateTime.Today;
            var minutesFromStart = (time - dayStart).TotalMinutes;
            return minutesFromStart * (zoom / 60.0);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}