using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Helpers.Models
{
    public class DateTimeOffsetRange
    {
        public DateTimeOffset Begin;
        public DateTimeOffset End;
        
        public DateTimeOffsetRange() { }
        public DateTimeOffsetRange(DateTimeOffset begin, DateTimeOffset end)
        {
            Begin = DateTimeOffsetExtensions.Min(begin, end);
            End = DateTimeOffsetExtensions.Max(begin, end);
        }

        public IEnumerable<DateTimeOffsetRange> BreakBy(TimeSpan jumpSpan)
        {
            var currentBegin = Begin;

            while (currentBegin < End)
            {
                var currentEnd = currentBegin.Add(jumpSpan);
                currentEnd = DateTimeOffsetExtensions.Min(currentEnd, End);
                yield return new(currentBegin, currentEnd);

                currentBegin = currentEnd;
            }
        }

        public IEnumerable<DateTimeOffsetRange> BreakIntoDays()
        {
            return BreakBy(TimeSpan.FromDays(1));
        }

        public IEnumerable<DateTimeOffsetRange> BreakIntoHours()
        {
            return BreakBy(TimeSpan.FromHours(1));
        }
    }
}
