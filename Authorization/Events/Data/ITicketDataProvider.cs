using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Data
{
    public interface ITicketDataProvider
    {
        Task<bool> Create(EventTicketRecord record);
        Task<EventTicketRecord> GetById(Guid ticketId, Guid eventId);
        IAsyncEnumerable<EventTicketRecord> GetAllByEvent(Guid eventId);
    }
}
