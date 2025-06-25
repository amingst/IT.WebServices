using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Data
{
    public interface IEventDataProvider
    {
        Task<bool> Create(EventRecord eventRecord);
        Task<bool> Delete(Guid eventId);
        Task<bool> Exists(Guid eventId);
        Task<EventRecord> GetById(Guid eventId);
        IAsyncEnumerable<EventRecord> GetAll();
        Task Save(EventRecord record);
    }
}
