using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ma.TimeManagement.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime At(this DateTime date, int hour, int minute = 0)
            => date.Date.AddHours(hour).AddMinutes(minute);
    }
}
