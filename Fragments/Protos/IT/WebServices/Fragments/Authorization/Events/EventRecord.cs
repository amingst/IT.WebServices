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
    public sealed partial class EventRecord : pb::IMessage<EventRecord>
    {
        public EventTicketClass GetTicketClass(string ticketClassId)
        {
            switch (EventPublicRecordOneOfCase)
            {
                case EventPublicRecordOneOfOneofCase.SinglePublic:
                    return GetTicketClassFromSingle(ticketClassId);
                case EventPublicRecordOneOfOneofCase.RecurringPublic:
                    return GetTicketClassFromRecurring(ticketClassId);
                default:
                    return null;
            }
        }

        private EventTicketClass GetTicketClassFromSingle(string ticketClassId)
        {
            if (SinglePublic.TicketClasses.Any(tc => tc.TicketClassId == ticketClassId))
            {
                return SinglePublic.TicketClasses.First(tc => tc.TicketClassId == ticketClassId);
            }
            return null;
        }

        private EventTicketClass GetTicketClassFromRecurring(string ticketClassId)
        {
            if (RecurringPublic.TicketClasses.Any(tc => tc.TicketClassId == ticketClassId))
            {
                return RecurringPublic.TicketClasses.First(tc => tc.TicketClassId == ticketClassId);
            }
            return null;
        }
    }
}
