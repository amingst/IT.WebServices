using IT.WebServices.Fragments.Authorization.Payment.Stripe;

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
