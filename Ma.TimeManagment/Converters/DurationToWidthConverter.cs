using System;
using System.Globalization;
using System.Windows.Data;

namespace Ma.TimeManagement.Converters;

public class DurationToWidthConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 3 || values[0] is not DateTime start || values[1] is not DateTime end)
            return 100.0;

        var minutes = (end - start).TotalMinutes;
        var pixelsPerHour = values[2] is double z ? z : 80.0;
        var width = minutes * (pixelsPerHour / 60.0);

        return Math.Max(50, width); // minimum readable width
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}