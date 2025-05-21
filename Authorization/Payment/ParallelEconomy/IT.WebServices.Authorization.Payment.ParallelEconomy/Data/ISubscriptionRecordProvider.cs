using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.ParallelEconomy.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId);
        IAsyncEnumerable<ParallelEconomySubscriptionRecord> GetAll();
        IAsyncEnumerable<ParallelEconomySubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<ParallelEconomySubscriptionRecord?> GetById(Guid userId, Guid subId);
        Task Save(ParallelEconomySubscriptionRecord record);
    }
}
