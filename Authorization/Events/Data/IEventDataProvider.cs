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
        Task<CreateEventErrorType> Create(EventRecord record);
        Task<CreateRecurringEventErrorType> CreateRecurring(IEnumerable<EventRecord> records);
        Task<(EventRecord, GetEventErrorType)> GetById(Guid id);
        IAsyncEnumerable<EventRecord> GetEvents();
        Task<CreateEventErrorType> Update(EventRecord record);
        Task<CreateRecurringEventErrorType> UpdateRecurring(IEnumerable<EventRecord> records);
        Task<bool> Exists(Guid eventId);
    }
}
