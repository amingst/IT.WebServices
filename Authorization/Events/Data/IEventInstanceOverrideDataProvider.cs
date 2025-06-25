using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Events;

namespace IT.WebServices.Authorization.Events.Data
{
    public interface IEventInstanceOverrideDataProvider
    {
        Task<bool> Create(EventInstanceOverride record);
        IAsyncEnumerable<EventInstanceOverride> GetByEventId(Guid eventId);
        Task<bool> HasOverride(Guid eventId, string InstanceId);
        Task Save(EventInstanceOverride record);
    }
}
