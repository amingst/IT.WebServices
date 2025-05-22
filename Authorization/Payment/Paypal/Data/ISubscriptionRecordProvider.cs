using IT.WebServices.Fragments.Authorization.Payment.Paypal;

namespace IT.WebServices.Authorization.Payment.Paypal.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subscriptionId);
        Task<bool> Exists(Guid userId, Guid subscriptionId);
        IAsyncEnumerable<PaypalSubscriptionRecord> GetAll();
        IAsyncEnumerable<PaypalSubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<PaypalSubscriptionRecord?> GetById(Guid userId, Guid subscriptionId);
        Task Save(PaypalSubscriptionRecord record);
    }
}
