using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public interface IGenericPaymentRecordProvider
    {
        Task Delete(Guid userId, Guid subId, Guid paymentId);
        Task DeleteAll(Guid userId, Guid subId);
        Task<bool> Exists(Guid userId, Guid subId, Guid paymentId);
        IAsyncEnumerable<GenericPaymentRecord> GetAll();
        IAsyncEnumerable<GenericPaymentRecord> GetAllBySubscriptionId(Guid userId, Guid subId);
        IAsyncEnumerable<GenericPaymentRecord> GetAllByUserId(Guid userId);
        Task<GenericPaymentRecord?> GetById(Guid userId, Guid subId, Guid paymentId);
        Task<GenericPaymentRecord?> GetByProcessorId(string processorPaymentId);
        Task Save(GenericPaymentRecord record);
    }
}
