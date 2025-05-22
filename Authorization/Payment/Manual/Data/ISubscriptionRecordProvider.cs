using IT.WebServices.Fragments.Authorization.Payment.Manual;

namespace IT.WebServices.Authorization.Payment.Manual.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId);
        IAsyncEnumerable<ManualSubscriptionRecord> GetAll();
        IAsyncEnumerable<ManualSubscriptionRecord> GetAllByUserId(Guid userId);
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<ManualSubscriptionRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        Task Save(ManualSubscriptionRecord record);
    }
}
