using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Manual.Extensions
{
    public static class ParserExtensions
    {
        public static EventRecord ParseEventRecord(this DbDataReader rdr)
        {
            var eventRecord = new EventRecord()
            {
                EventId = rdr["EventID"] as string,
                Public = new()
                {
                    Title = rdr["Title"] as string ?? "",
                    Description = rdr["Description"] as string ?? "",
                    AccessData = new() { MinimumLevel = (uint)(int)rdr["MinimumAccessLevel"] },
                    LocationData = new()
                    {
                        VenueName = rdr["VenueName"] as string ?? "",
                        VenueStreetAddress = rdr["VenueStreetAddress"] as string ?? "",
                        SecondaryVenueStreetAddress =
                            rdr["SecondaryVenueStreetAddress"] as string ?? "",
                        VenueCity = rdr["VenueCity"] as string ?? "",
                        VenueStateOrProvince = rdr["VenueStateOrProvince"] as string ?? "",
                        VenuePostalCode = rdr["VenuePostalCode"] as string ?? "",
                        VenueCountry = rdr["VenueCountry"] as string ?? "",
                    },
                },
                Private = new()
                {
                    LifecycleMetadata = new()
                    {
                        CreatedById = rdr["CreatedById"] as string ?? "",
                        ModifiedById = rdr["ModifiedById"] as string ?? "",
                        DeletedById = rdr["DeletedById"] as string ?? "",
                    },
                },
            };

            DateTime d;
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

            return eventRecord;
        }

        public static EventTicketRecord ParseEventTicketRecord(this DbDataReader rdr)
        {
            var ticketRecord = new EventTicketRecord()
            {
                TicketId = rdr["TicketId"] as string,
                Public = new()
                {
                    TicketName = rdr["TicketName"] as string ?? "",
                    EventId = rdr["EventId"] as string,
                    Price = (uint)(int)rdr["TicketPrice"],
                    MaxAttendees = (uint)(int)rdr["MaxAttendees"],
                    MaxPerUser = (uint)(int)rdr["MaxPerUser"],
                },
                Private = new() { QuantityAvailible = (uint)(int)rdr["QuantityAvailible"] },
            };

            DateTime d;
            if (!(rdr["SaleStartOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["SaleStartOnUTC"], DateTimeKind.Utc);
                ticketRecord.Private.SaleStartOnUTC =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["SaleEndOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["SaleEndOnUTC"], DateTimeKind.Utc);
                ticketRecord.Private.SaleEndOnUTC =
                    Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return ticketRecord;
        }
    }
}
