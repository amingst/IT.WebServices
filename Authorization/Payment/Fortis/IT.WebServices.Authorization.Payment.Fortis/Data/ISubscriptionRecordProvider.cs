using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId);
        IAsyncEnumerable<FortisSubscriptionRecord> GetAll();
        IAsyncEnumerable<FortisSubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<FortisSubscriptionRecord?> GetById(Guid userId, Guid subId);
        Task Save(FortisSubscriptionRecord record);
    }
}
