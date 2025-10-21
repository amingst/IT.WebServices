using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment
{
    public sealed partial class PaymentBulkActionProgress : pb::IMessage<PaymentBulkActionProgress>
    {
        public bool IsCanceled => CanceledOnUTC != null;
        public bool IsCompleted => CompletedOnUTC != null;

        public bool IsCompletedOrCanceled => IsCompleted || IsCanceled;
    }
}
