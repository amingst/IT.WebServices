using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Stripe.Data
{
    public interface ISubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<StripeSubscriptionFullRecord> GetAll();
        IAsyncEnumerable<StripeSubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<StripeSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(StripeSubscriptionFullRecord record);
    }
}
