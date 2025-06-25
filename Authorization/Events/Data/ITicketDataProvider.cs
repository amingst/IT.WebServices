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
        Task<bool> Delete(Guid ticketId);
        Task<bool> Exists(Guid eventId, Guid ticketId);
        Task<EventTicketRecord> GetById(Guid ticketId);
        IAsyncEnumerable<EventTicketRecord> GetAll();
        IAsyncEnumerable<EventTicketRecord> GetAllByEvent(Guid eventId);
        Task Save(EventTicketRecord record);
    }
}
