using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public interface IGenericSubscriptionFullRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<GenericSubscriptionFullRecord> GetAll();
        IAsyncEnumerable<GenericSubscriptionFullRecord> GetAllByUserId(Guid userId);
        Task<GenericSubscriptionFullRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(GenericSubscriptionFullRecord record);
    }
}
