using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Authorization.Payment.Generic;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Fragments.Authorization.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis
{
    public class FortisGenericPaymentProvider : IGenericPaymentProvider
    {
        private readonly FortisSubscriptionHelper fortisSubscriptionHelper;
        private readonly IGenericSubscriptionRecordProvider genericSubProvider;

        public FortisGenericPaymentProvider(FortisSubscriptionHelper fortisSubscriptionHelper, IGenericSubscriptionRecordProvider genericSubProvider)
        {
            this.fortisSubscriptionHelper = fortisSubscriptionHelper;
            this.genericSubProvider = genericSubProvider;
        }

        public string ProcessorName => PaymentConstants.PROCESSOR_NAME_FORTIS;

        public async Task<CancelSubscriptionResponse> CancelSubscription(GenericSubscriptionRecord record, ONUser userToken)
        {
            var res = await fortisSubscriptionHelper.Get(record.InternalSubscriptionID);
            if (res == null)
                return new() { Error = "SubscriptionId not valid" };

            if (res.Status == SubscriptionStatus.SubscriptionActive)
            {
                var cancelRes = await fortisSubscriptionHelper.Cancel(record.InternalSubscriptionID);
                if (cancelRes?.Status != SubscriptionStatus.SubscriptionStopped)
                    return new() { Error = "Unable to cancel subscription" };
            }

            record.CanceledBy = userToken.Id.ToString();
            record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

            await genericSubProvider.Save(record);

            return new()
            {
                Record = record
            };
        }

        public Task<ReconcileSubscriptionResponse> ReconcileSubscription(GenericSubscriptionRecord record, ONUser userToken)
        {
            throw new NotImplementedException();
        }
    }
}
