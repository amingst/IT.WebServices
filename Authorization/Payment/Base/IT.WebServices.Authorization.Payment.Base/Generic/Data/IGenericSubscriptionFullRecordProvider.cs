using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Generic;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public interface IGenericSubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<GenericSubscriptionFullRecord> GetAll();
        IAsyncEnumerable<GenericSubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<GenericSubscriptionFullRecord?> GetBySubscription(GenericSubscriptionRecord record) => GetBySubscriptionId(record.UserID.ToGuid(), record.InternalSubscriptionID.ToGuid());
        Task<GenericSubscriptionFullRecord?> GetBySubscription(GenericSubscriptionFullRecord record) => GetBySubscription(record.SubscriptionRecord);
        Task<GenericSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(GenericSubscriptionFullRecord record);
    }
}
