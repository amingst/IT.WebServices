using IT.WebServices.Authorization.Payment.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Helpers
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset Max(DateTimeOffset val1, DateTimeOffset val2)
        {
            return (val1 >= val2) ? val1 : val2;
        }

        public static DateTimeOffset Min(DateTimeOffset val1, DateTimeOffset val2)
        {
            return (val1 <= val2) ? val1 : val2;
        }

        public static DateTimeOffsetRange ToRange(this DateTimeOffset begin, DateTimeOffset end)
        {
            if (begin <= end)
                return new(begin, end);
            else
                return new(end, begin);
        }

        public static DateTimeOffsetRange ToRangeGoingBackDays(this DateTimeOffset end, int daysBack)
        {
            return new(end, end.AddDays(-daysBack));
        }

        public static DateTimeOffsetRange ToRangeGoingBackHours(this DateTimeOffset end, int hoursBack)
        {
            return new(end, end.AddHours(-hoursBack));
        }

        public static DateTimeOffsetRange ToRangeGoingBackMonths(this DateTimeOffset end, int monthsBack)
        {
            return new(end, end.AddMonths(-monthsBack));
        }
    }
}
