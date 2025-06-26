using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Extensions
{
    public static class ParserExtensions
    {
        public static EventRecord ParseEventRecord(this DbDataReader rdr)
        {
            var eventRecord = new EventRecord()
            {
                EventId = rdr["EventID"] as string,
                Public = new EventPublicRecord()
                {
                    Title = rdr["Title"] as string ?? "",
                    Description = rdr["Description"] as string ?? "",
                    Type = Enum.TryParse<EventType>(rdr["EventType"]?.ToString(), out var eventType)
                        ? eventType
                        : EventType.Live, // Default fallback
                    MaxAttendees =
                        rdr["MaxAttendees"] is DBNull ? 0 : Convert.ToUInt32(rdr["MaxAttendees"]),
                    AccessData = new EventAccessData
                    {
                        MinimumLevel =
                            rdr["MinimumAccessLevel"] is DBNull
                                ? 0
                                : Convert.ToUInt32(rdr["MinimumAccessLevel"]),
                        Access = Enum.TryParse<AccessType>(
                            rdr["AccessType"]?.ToString(),
                            out var accessType
                        )
                            ? accessType
                            : AccessType.AllAccess,
                    },
                    LocationData = new EventLocationData()
                    {
                        VenueName = rdr["VenueName"] as string ?? "",
                        VenueStreetAddress = rdr["VenueStreetAddress"] as string ?? "",
                        SecondaryVenueStreetAddress =
                            rdr["SecondaryVenueStreetAddress"] as string ?? "",
                        VenueCity = rdr["VenueCity"] as string ?? "",
                        VenueStateOrProvince = rdr["VenueStateOrProvince"] as string ?? "",
                        VenuePostalCode = rdr["VenuePostalCode"] as string ?? "",
                        VenueCountry = rdr["VenueCountry"] as string ?? "",
                        Latitude = rdr["Latitude"] as string ?? "",
                        Longitude = rdr["Longitude"] as string ?? "",
                    },
                    Recurrence = null, // We'll handle this next
                    StartDate = null,
                    EndDate = null,
                    LifecycleMetadata = new LifecycleMetadataPublic(),
                },
                Private = new EventPrivateRecord()
                {
                    LifecycleMetadata = new LifecycleMetadataPrivate()
                    {
                        CreatedById = rdr["CreatedById"] as string ?? "",
                        ModifiedById = rdr["ModifiedById"] as string ?? "",
                        DeletedById = rdr["DeletedById"] as string ?? "",
                    },
                    EventSettings = new EventSettings()
                    {
                        ShowStartDate =
                            rdr["ShowStartDate"] is DBNull ? false : (bool)rdr["ShowStartDate"],
                        ShowEndDate =
                            rdr["ShowEndDate"] is DBNull ? false : (bool)rdr["ShowEndDate"],
                        ShowMaxAttendees =
                            rdr["ShowMaxAttendees"] is DBNull
                                ? false
                                : (bool)rdr["ShowMaxAttendees"],
                        LocationSettings = new EventLocationSettings()
                        {
                            ShowLocationBeforeEvent =
                                rdr["ShowLocationBeforeEvent"] is DBNull
                                    ? false
                                    : (bool)rdr["ShowLocationBeforeEvent"],
                            ShowLocationOnUTC = null, // parse below if exists
                        },
                    },
                },
            };

            DateTime d;

            // Parse timestamps safely:
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                eventRecord.Public.LifecycleMetadata.CreatedOnUTC =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                eventRecord.Public.LifecycleMetadata.ModifiedOnUTC =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["StartDate"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["StartDate"], DateTimeKind.Utc);
                eventRecord.Public.StartDate =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["EndDate"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["EndDate"], DateTimeKind.Utc);
                eventRecord.Public.EndDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(
                    d
                );
            }

            if (!(rdr["ShowLocationOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ShowLocationOnUTC"], DateTimeKind.Utc);
                eventRecord.Private.EventSettings.LocationSettings.ShowLocationOnUTC =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            // Parse recurrence rule if present (assuming JSON or separate fields for frequency, interval, etc.)
            // If stored as JSON or separate columns, deserialize or map accordingly:
            var freqObj = rdr["RecurrenceFrequency"];
            if (freqObj != DBNull.Value)
            {
                var recurrence = new EventRecurrenceRule();

                recurrence.Frequency = Enum.TryParse<RecurrenceFrequency>(
                    freqObj.ToString(),
                    out var freq
                )
                    ? freq
                    : RecurrenceFrequency.RepeatNone;

                recurrence.Interval =
                    rdr["RecurrenceInterval"] is DBNull
                        ? 1
                        : Convert.ToUInt32(rdr["RecurrenceInterval"]);

                // Parse ByWeekday, Count, RepeatUntilUTC, ExcludeDatesUTC similarly
                // Example for Count:
                if (rdr["RecurrenceCount"] is not DBNull)
                    recurrence.Count = Convert.ToUInt32(rdr["RecurrenceCount"]);

                if (!(rdr["RecurrenceRepeatUntilUTC"] is DBNull))
                {
                    d = DateTime.SpecifyKind(
                        (DateTime)rdr["RecurrenceRepeatUntilUTC"],
                        DateTimeKind.Utc
                    );
                    recurrence.RepeatUntilUTC =
                        Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
                }

                // ByWeekday and ExcludeDatesUTC parsing depends on how you store them (e.g., JSON string or CSV)
                // You may need to parse them here

                eventRecord.Public.Recurrence = recurrence;
            }

            return eventRecord;
        }

        //public static EventTicketRecord ParseEventTicketRecord(this DbDataReader rdr)
        //{
        //    var ticketRecord = new EventTicketRecord()
        //    {
        //        TicketId = rdr["TicketId"] as string,
        //        Public = new()
        //        {
        //            TicketName = rdr["TicketName"] as string ?? "",
        //            EventId = rdr["EventId"] as string,
        //            Price = (uint)(int)rdr["TicketPrice"],
        //            MaxAttendees = (uint)(int)rdr["MaxAttendees"],
        //            MaxPerUser = (uint)(int)rdr["MaxPerUser"],
        //        },
        //        Private = new() { QuantityAvailible = (uint)(int)rdr["QuantityAvailible"] },
        //    };

        //    DateTime d;
        //    if (!(rdr["SaleStartOnUTC"] is DBNull))
        //    {
        //        d = DateTime.SpecifyKind((DateTime)rdr["SaleStartOnUTC"], DateTimeKind.Utc);
        //        ticketRecord.Private.SaleStartOnUTC =
        //            Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
        //    }

        //    if (!(rdr["SaleEndOnUTC"] is DBNull))
        //    {
        //        d = DateTime.SpecifyKind((DateTime)rdr["SaleEndOnUTC"], DateTimeKind.Utc);
        //        ticketRecord.Private.SaleEndOnUTC =
        //            Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
        //    }

        //    return ticketRecord;
        //}
    }
}
