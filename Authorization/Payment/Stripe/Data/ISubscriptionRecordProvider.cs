using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Stripe.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subscriptionId);
        Task<bool> Exists(Guid userId, Guid subscriptionId);
        IAsyncEnumerable<StripeSubscriptionRecord> GetAll();
        IAsyncEnumerable<StripeSubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<StripeSubscriptionRecord?> GetById(Guid userId, Guid subscriptionId);
        Task Save(StripeSubscriptionRecord record);
    }
}
