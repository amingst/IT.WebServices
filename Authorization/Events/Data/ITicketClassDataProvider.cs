using IT.WebServices.Fragments.Authorization.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Events.Data
{
    public interface ITicketClassDataProvider
    {
        Task<bool> Create(EventTicketClass ticketClass);
        Task<EventTicketClass> GetById(Guid ticketClassId);
        IAsyncEnumerable<EventTicketClass> GetAll();
        Task<bool> Exists(Guid ticketClassId);
    }
}
