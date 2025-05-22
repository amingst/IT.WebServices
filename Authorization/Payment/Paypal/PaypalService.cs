using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Data;
using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;

namespace IT.WebServices.Authorization.Payment.Paypal
{
    public class PaypalService : PaypalInterface.PaypalInterfaceBase
    {
        private readonly ILogger<PaypalService> logger;
        private readonly ISubscriptionFullRecordProvider fullProvider;
        private readonly ISubscriptionRecordProvider subProvider;
        private readonly IPaymentRecordProvider paymentProvider;
        private readonly PaypalClient client;
        private readonly SettingsClient settingsClient;

        public PaypalService(ILogger<PaypalService> logger, ISubscriptionFullRecordProvider fullProvider, ISubscriptionRecordProvider subProvider, IPaymentRecordProvider paymentProvider, PaypalClient client, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.fullProvider = fullProvider;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.client = client;
            this.settingsClient = settingsClient;
        }

        public override async Task<PaypalCancelOwnSubscriptionResponse> PaypalCancelOwnSubscription(PaypalCancelOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                Guid subscriptionId;
                if (!Guid.TryParse(request.SubscriptionID, out subscriptionId))
                    return new() { Error = "No SubscriptionID specified" };

                var record = await subProvider.GetById(userToken.Id, subscriptionId);
                if (record == null)
                    return new() { Error = "Record not found" };

                var sub = await client.GetSubscription(record.PaypalSubscriptionID);
                if (sub == null)
                    return new() { Error = "SubscriptionId not valid" };

                if (sub.status == "ACTIVE")
                {
                    var canceled = await client.CancelSubscription(record.PaypalSubscriptionID, request.Reason ?? "None");
                    if (!canceled)
                        return new() { Error = "Unable to cancel subscription" };
                }

                record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
                record.ModifiedBy = userToken.Id.ToString();
                record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
                record.CanceledBy = userToken.Id.ToString();
                record.Status = Fragments.Authorization.Payment.SubscriptionStatus.SubscriptionStopped;

                await subProvider.Save(record);

                return new()
                {
                    Record = record
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
        }

        public override async Task<PaypalGetOwnSubscriptionRecordsResponse> PaypalGetOwnSubscriptionRecords(PaypalGetOwnSubscriptionRecordsRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var res = new PaypalGetOwnSubscriptionRecordsResponse();
            res.Records.AddRange(await fullProvider.GetAllByUserId(userToken.Id).ToList());

            return res;
        }

        public override async Task<PaypalNewOwnSubscriptionResponse> PaypalNewOwnSubscription(PaypalNewOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                if (request?.PaypalSubscriptionID == null)
                    return new() { Error = "SubscriptionId not valid" };

                var sub = await client.GetSubscription(request.PaypalSubscriptionID);
                if (sub == null)
                    return new() { Error = "SubscriptionId not valid" };

                var billing_info = sub.billing_info;
                if (billing_info == null)
                    return new() { Error = "SubscriptionId not valid" };

                decimal value = 0;
                if (!decimal.TryParse(sub.billing_info?.last_payment?.amount?.value ?? "0", out value))
                    return new() { Error = "Subscription Value not valid" };

                var record = new PaypalSubscriptionRecord()
                {
                    UserID = userToken.Id.ToString(),
                    SubscriptionID = Guid.NewGuid().ToString(),
                    PaypalSubscriptionID = request.PaypalSubscriptionID,
                    AmountCents = (uint)(value * 100),
                    Status = Fragments.Authorization.Payment.SubscriptionStatus.SubscriptionActive,
                    CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                };

                await subProvider.Save(record);

                var payment = new PaypalPaymentRecord()
                {
                    UserID = userToken.Id.ToString(),
                    SubscriptionID = record.SubscriptionID,
                    PaymentID = Guid.NewGuid().ToString(),
                    PaypalPaymentID = sub.id,
                    AmountCents = (uint)(value * 100),
                    Status = Fragments.Authorization.Payment.PaymentStatus.PaymentComplete,
                    CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.create_time),
                    PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(billing_info.next_billing_time),
                };

                await paymentProvider.Save(payment);

                return new()
                {
                    Record = record
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
        }
    }
}
