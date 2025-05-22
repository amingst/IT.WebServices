using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.ParallelEconomy.Data
{
    public interface ISubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<ParallelEconomySubscriptionFullRecord> GetAll();
        IAsyncEnumerable<ParallelEconomySubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<ParallelEconomySubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(ParallelEconomySubscriptionFullRecord record);
    }
}
