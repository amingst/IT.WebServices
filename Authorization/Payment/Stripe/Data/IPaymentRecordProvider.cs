using Google.Protobuf.Collections;
using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Stripe.Data
{
    public interface IPaymentRecordProvider
    {
        Task Delete(Guid userId, Guid subscriptionId, Guid paymentId);
        Task DeleteAll(Guid userId, Guid subscriptionId);
        Task<bool> Exists(Guid userId, Guid subscriptionId, Guid paymentId);
        IAsyncEnumerable<StripePaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subscriptionId);
        IAsyncEnumerable<StripePaymentRecord> GetAllByUserId(Guid userId);
        Task<StripePaymentRecord?> GetById(Guid userId, Guid subscriptionId, Guid paymentId);
        Task Save(StripePaymentRecord record);
        Task SaveAll(IEnumerable<StripePaymentRecord> payments);
    }
}
