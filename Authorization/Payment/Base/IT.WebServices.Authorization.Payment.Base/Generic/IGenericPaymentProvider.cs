using IT.WebServices.Authentication;
using IT.WebServices.Fragments.Authorization.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Generic
{
    public interface IGenericPaymentProvider
    {
        string ProcessorName { get; }

        Task<CancelSubscriptionResponse> CancelSubscription(GenericSubscriptionRecord record, ONUser userToken);
        Task<ReconcileSubscriptionResponse> ReconcileSubscription(GenericSubscriptionRecord record, ONUser userToken);
    }
}
