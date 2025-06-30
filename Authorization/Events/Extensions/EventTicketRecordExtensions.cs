using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Events;
using System.Collections.Generic;
using System.Linq;

namespace IT.WebServices.Authorization.Events.Extensions
{
    public static class EventTicketRecordExtensions
    {
        public static ClaimRecord[] ToClaimRecords(this IEnumerable<EventTicketRecord> tickets)
        {
            return tickets
                .Where(t => t != null)
                .Select(t => new ClaimRecord
                {
                    Name = t.TicketId ?? "",
                    Value = t.Public?.EventId ?? "",
                    ExpiresOnUTC = t.Public?.ExpiredOnUTC ?? default,
                })
                .ToArray();
        }
    }
}