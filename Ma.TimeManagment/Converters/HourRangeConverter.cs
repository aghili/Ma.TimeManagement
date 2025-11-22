using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace Ma.TimeManagement.Converters;

public class HourRangeConverter : IValueConverter
{
    public static readonly HourRangeConverter Instance = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // Returns 08:00, 09:00, ..., 22:00
        return Enumerable.Range(8, 15).Select(h => DateTime.Today.AddHours(h));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}