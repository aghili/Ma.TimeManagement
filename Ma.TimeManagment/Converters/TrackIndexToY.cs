using System.Globalization;
using System.Windows.Data;

namespace Ma.TimeManagement.Converters
{
    public class TrackIndexToY : IValueConverter
    {
        public static readonly TrackIndexToY Instance = new();
        public object Convert(object value, Type t, object p, CultureInfo c)
            => value is int index ? index * 70.0 : 0.0;
        public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
    }
}
