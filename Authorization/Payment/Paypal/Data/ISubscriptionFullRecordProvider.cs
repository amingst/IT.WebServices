using IT.WebServices.Fragments.Authorization.Payment.Paypal;

namespace IT.WebServices.Authorization.Payment.Paypal.Data
{
    public interface ISubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<PaypalSubscriptionFullRecord> GetAll();
        IAsyncEnumerable<PaypalSubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<PaypalSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(PaypalSubscriptionFullRecord record);
    }
}
