using IT.WebServices.Fragments.Authorization.Payment.Paypal;

namespace IT.WebServices.Authorization.Payment.Paypal.Data
{
    public interface IPaymentRecordProvider
    {
        Task Delete(Guid userId, Guid subscriptionId, Guid paymentId);
        Task DeleteAll(Guid userId, Guid subscriptionId);
        Task<bool> Exists(Guid userId, Guid subscriptionId, Guid paymentId);
        IAsyncEnumerable<PaypalPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subscriptionId);
        IAsyncEnumerable<PaypalPaymentRecord> GetAllByUserId(Guid userId);
        Task<PaypalPaymentRecord?> GetById(Guid userId, Guid subscriptionId, Guid paymentId);
        Task Save(PaypalPaymentRecord record);
        Task SaveAll(IEnumerable<PaypalPaymentRecord> payments);
    }
}
