using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Helpers
{
    public static class EventInstanceHelper
    {
        // TODO: Support Pagination
        public static List<EventInstance> BuildInstancesForEvent(EventRecord record)
        {
            var res = new List<EventInstance>();

            var start = record.Public.StartDate.ToDateTime();
            var end = record.Public.EndDate.ToDateTime();
            var duration = end - start;

            var recurrence = record.Public.Recurrence;
            if (recurrence is null)
                return res;

            if (recurrence.Count == 0 && recurrence.RepeatUntilUTC == null)
                return res;

            var count = recurrence.Count > 0 ? recurrence.Count : int.MaxValue;
            var repeatUntil = recurrence.RepeatUntilUTC?.ToDateTime() ?? DateTime.MaxValue;
            var interval = recurrence.Interval > 0 ? recurrence.Interval : 1;

            var generated = 0;

            switch (recurrence.Frequency)
            {
                case RecurrenceFrequency.RepeatWeekly:
                {
                    var weekStart = start.Date;

                    while (generated < count && weekStart <= repeatUntil)
                    {
                        foreach (var day in recurrence.ByWeekday)
                        {
                            var targetDate = weekStart.AddDays(
                                ((int)day - (int)weekStart.DayOfWeek + 7) % 7
                            );

                            if (targetDate < start)
                                continue;

                            if (targetDate > repeatUntil)
                                break;

                            res.Add(CreateInstance(record, targetDate, duration));
                            generated++;
                            if (generated >= count)
                                break;
                        }

                        weekStart = weekStart.AddDays(7 * interval);
                    }

                    break;
                }

                case RecurrenceFrequency.RepeatDaily:
                {
                    var current = start;

                    while (generated < count && current <= repeatUntil)
                    {
                        res.Add(CreateInstance(record, current, duration));
                        current = current.AddDays(interval);
                        generated++;
                    }

                    break;
                }

                case RecurrenceFrequency.RepeateMonthly:
                {
                    var current = start;

                    while (generated < count && current <= repeatUntil)
                    {
                        res.Add(CreateInstance(record, current, duration));
                        current = current.AddMonths((int)interval);
                        generated++;
                    }

                    break;
                }

                case RecurrenceFrequency.RepeatYearly:
                {
                    var current = start;

                    while (generated < count && current <= repeatUntil)
                    {
                        res.Add(CreateInstance(record, current, duration));
                        current = current.AddYears((int)interval);
                        generated++;
                    }

                    break;
                }

                default:
                    break;
            }

            return res;
        }

        private static EventInstance CreateInstance(
            EventRecord record,
            DateTime start,
            TimeSpan duration
        )
        {
            return new EventInstance
            {
                InstanceId = GenerateInstanceId(record.EventId, start.ToUniversalTime()),
                ParentEventId = record.EventId,
                StartDate = Timestamp.FromDateTime(start.ToUniversalTime()),
                EndDate = Timestamp.FromDateTime(start.Add(duration).ToUniversalTime()),
                IsCancelled = false, // TODO: Add cancel logic
            };
        }

        private static string GenerateInstanceId(string parentEventId, DateTime startDateUtc)
        {
            var input = $"{parentEventId}_{startDateUtc:O}";
            using var sha = System.Security.Cryptography.SHA256.Create();
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }
    }
}
