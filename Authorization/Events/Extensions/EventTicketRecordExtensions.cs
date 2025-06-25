using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Extensions
{
    public static class EventTicketRecordExtensions
    {
        public static EventTicketListRecord ToTicketListRecord(this EventTicketRecord record)
        {
            return new EventTicketListRecord()
            {
                TicketId = record.TicketId,
                TicketName = record.Public.TicketName,
                Price = record.Public.Price,
                MaxAttendees = record.Public.MaxAttendees,
                MaxPerUser = record.Public.MaxPerUser,
            };
        }
    }
}
