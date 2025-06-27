using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Helpers
{
    public static class RecurrenceHelper
    {
        public record EventInstance(DateTime Start, DateTime End);

        public static string GenerateRecurrenceHash(string input)
        {
            using var sha256 = SHA256.Create();
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            byte[] hashBytes = sha256.ComputeHash(inputBytes);

            // Convert bytes to hex string
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static List<EventInstance> GenerateInstances(EventRecord eventRecord)
        {
            if (
                eventRecord == null
                || eventRecord.OneOfType != EventRecordOneOfType.EventOneOfRecurring
            )
                return new();

            var rule = eventRecord.RecurringPublic.Recurrence;
            var templateStart = eventRecord.RecurringPublic.TemplateStartOnUTC.ToDateTime();
            var templateEnd = eventRecord.RecurringPublic.TemplateEndOnUTC.ToDateTime();

            if (rule == null || rule.Frequency == RecurrenceFrequency.RepeatNone)
                return new();

            var results = new List<EventInstance>();

            var interval = Math.Max(1, rule.Interval);
            var duration = templateEnd - templateStart;

            var current = templateStart;
            var limit = GetEndConditionLimit(rule, templateStart);
            var count = 0;
            var maxCount = rule.HasCount ? rule.Count : int.MaxValue;

            while (count < maxCount && current <= limit)
            {
                // Skip excluded dates
                if (!rule.ExcludeDatesUTC.Any(d => d.ToDateTime().Date == current.Date))
                {
                    // If ByWeekday is specified, ensure current day matches
                    if (
                        rule.ByWeekday.Count == 0
                        || rule.ByWeekday.Contains(ToWeekdayEnum(current.DayOfWeek))
                    )
                    {
                        results.Add(new EventInstance(current, current + duration));
                        count++;
                    }
                }

                // Advance by frequency type and interval
                current = rule.Frequency switch
                {
                    RecurrenceFrequency.RepeatDaily => current.AddDays(interval),
                    RecurrenceFrequency.RepeatWeekly => current.AddDays(7 * interval),
                    RecurrenceFrequency.RepeateMonthly => current.AddMonths((int)interval),
                    RecurrenceFrequency.RepeatYearly => current.AddYears((int)interval),
                    _ => current.AddDays(1),
                };
            }

            return results;
        }

        private static DateTime GetEndConditionLimit(EventRecurrenceRule rule, DateTime start)
        {
            if (rule.RepeatUntilUTC is not null)
                return rule.RepeatUntilUTC.ToDateTime();
            return start.AddYears(5); // Arbitrary safety cap if no limit set
        }

        private static WeekdayEnum ToWeekdayEnum(DayOfWeek day)
        {
            return day switch
            {
                DayOfWeek.Monday => WeekdayEnum.Monday,
                DayOfWeek.Tuesday => WeekdayEnum.Tuesday,
                DayOfWeek.Wednesday => WeekdayEnum.Wednesday,
                DayOfWeek.Thursday => WeekdayEnum.Thursday,
                DayOfWeek.Friday => WeekdayEnum.Friday,
                DayOfWeek.Saturday => WeekdayEnum.Saturday,
                DayOfWeek.Sunday => WeekdayEnum.Sunday,
                _ => WeekdayEnum.Monday, // Fallback
            };
        }
    }
}
