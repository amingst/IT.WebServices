using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public interface IGenericSubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId);
        IAsyncEnumerable<GenericSubscriptionRecord> GetAll();
        IAsyncEnumerable<GenericSubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<GenericSubscriptionRecord?> GetById(Guid userId, Guid subId);
        Task<GenericSubscriptionRecord?> GetByProcessorId(string processorSubId);
        Task Save(GenericSubscriptionRecord record);
    }
}
