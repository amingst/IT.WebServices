using IT.WebServices.Fragments.Authorization.Payment.Manual;

namespace IT.WebServices.Authorization.Payment.Manual.Data
{
    public interface ISubscriptionRecordProvider
    {
        Task Delete(Guid userId, Guid subId);
        IAsyncEnumerable<ManualSubscriptionRecord> GetAll();
        IAsyncEnumerable<(Guid userId, Guid subId)> GetAllSubscriptionIds();
        Task<bool> Exists(Guid userId, Guid subId);
        Task<ManualSubscriptionRecord?> GetBySubscriptionId(Guid userId, Guid subId);
        IAsyncEnumerable<ManualSubscriptionRecord> GetByUserId(Guid userId);
        Task Save(ManualSubscriptionRecord record);
    }
}
