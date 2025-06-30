using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
{
    public interface ISubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<FortisSubscriptionFullRecord> GetAll();
        IAsyncEnumerable<FortisSubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<FortisSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(FortisSubscriptionFullRecord record);
    }
}
