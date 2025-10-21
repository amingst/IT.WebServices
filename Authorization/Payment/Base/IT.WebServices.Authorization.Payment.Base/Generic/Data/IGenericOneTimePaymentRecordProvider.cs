using IT.WebServices.Fragments.Authorization.Payment;

namespace IT.WebServices.Authorization.Payment.Generic.Data
{
    public interface IGenericOneTimePaymentRecordProvider
    {
        Task Delete(Guid userId, Guid internalPaymentId);
        Task DeleteAll(Guid userId);
        Task<bool> Exists(Guid userId, Guid internalPaymentId);
        IAsyncEnumerable<GenericOneTimePaymentRecord> GetAll();
        IAsyncEnumerable<GenericOneTimePaymentRecord> GetAllByUserId(Guid userId);
        Task<GenericOneTimePaymentRecord?> GetById(Guid userId, Guid internalPaymentId);
        Task Save(GenericOneTimePaymentRecord record);
    }
}
