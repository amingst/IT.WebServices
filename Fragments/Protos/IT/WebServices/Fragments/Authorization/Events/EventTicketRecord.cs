using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Events
{
    public sealed partial class EventTicketRecord : pb::IMessage<EventTicketRecord>
    {
        public static List<EventTicketRecord> GenerateRecords(int numToGenerate, Fragments.Authorization.Events.EventRecord eventRecord, string userId, EventTicketClass ticketClass)
        {
            List<EventTicketRecord> tickets = new List<EventTicketRecord>();

            for (int i = 0; i <= numToGenerate; i++)
            {
                var now = Timestamp.FromDateTime(DateTime.UtcNow);
                var ticket = new EventTicketRecord()
                {
                    TicketId = Guid.NewGuid().ToString(),
                    Public = new EventTicketPublicRecord()
                    {
                        TicketClassId = ticketClass.TicketClassId,
                        Title = ticketClass.Name + " " + (eventRecord.EventPublicRecordOneOfCase == Fragments.Authorization.Events.EventRecord.EventPublicRecordOneOfOneofCase.SinglePublic ? eventRecord.SinglePublic.Title : eventRecord.RecurringPublic.Title),
                        EventId = eventRecord.EventId,
                        Status = EventTicketStatus.TicketStatusAvailable,
                        CreatedOnUTC = now,
                        ModifiedOnUTC = now,
                        ExpiredOnUTC = eventRecord.EventPublicRecordOneOfCase == Fragments.Authorization.Events.EventRecord.EventPublicRecordOneOfOneofCase.SinglePublic
                            ? eventRecord.SinglePublic.EndOnUTC
                            : eventRecord.RecurringPublic.TemplateEndOnUTC,
                    },
                    Private = new EventTicketPrivateRecord()
                    {
                        UserId = userId,
                        CreatedById = userId,
                        ModifiedById = userId,
                    },
                };

                tickets.Add(ticket);
            }

            return tickets;
        }
    }
}