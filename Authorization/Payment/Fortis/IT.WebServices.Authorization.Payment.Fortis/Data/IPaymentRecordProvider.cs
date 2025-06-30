using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Data
{
    public interface IPaymentRecordProvider
    {
        Task Delete(Guid userId, Guid subId, Guid paymentId);
        Task DeleteAll(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId, Guid paymentId);
        IAsyncEnumerable<FortisPaymentRecord> GetAll();
        IAsyncEnumerable<FortisPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId);
        IAsyncEnumerable<FortisPaymentRecord> GetAllByUserId(Guid userId);
        Task<FortisPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId);
        Task Save(FortisPaymentRecord record);
    }
}
