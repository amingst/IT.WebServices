using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Data
{
    public interface IRSVPDataProvider
    {
        Task<bool> Create(EventRSVPRecord record);
        Task<bool> Cancel(Guid eventRsvpGuid, string reason);
        IAsyncEnumerable<EventRSVPRecord> GetAllByEvent(Guid eventGuid);
        IAsyncEnumerable<EventRSVPRecord> GetAllByUser(Guid userGuid);
        Task<EventRSVPRecord> GetById(Guid id);
        Task Save(EventRSVPRecord record);
    }
}
