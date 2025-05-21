using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.ParallelEconomy.Data
{
    public interface IPaymentRecordProvider
    {
        Task Delete(Guid userId, Guid subId, Guid paymentId);
        Task DeleteAll(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId, Guid paymentId);
        IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAll();
        IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId);
        IAsyncEnumerable<ParallelEconomyPaymentRecord> GetAllByUserId(Guid userId);
        Task<ParallelEconomyPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId);
        Task Save(ParallelEconomyPaymentRecord record);
    }
}
