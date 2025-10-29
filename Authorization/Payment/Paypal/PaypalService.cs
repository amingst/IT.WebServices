using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Helpers;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Payment.Paypal
{
    public class PaypalService : PaypalInterface.PaypalInterfaceBase
    {
        private readonly ILogger<PaypalService> logger;
        private readonly IGenericSubscriptionFullRecordProvider fullProvider;
        private readonly IGenericSubscriptionRecordProvider subProvider;
        private readonly IGenericPaymentRecordProvider paymentProvider;
        private readonly PaypalClient client;
        private readonly ReconcileHelper reconcileHelper;
        private readonly SettingsClient settingsClient;

        public PaypalService(ILogger<PaypalService> logger, IGenericSubscriptionFullRecordProvider fullProvider, IGenericSubscriptionRecordProvider subProvider, IGenericPaymentRecordProvider paymentProvider, PaypalClient client, ReconcileHelper reconcileHelper, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.fullProvider = fullProvider;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.client = client;
            this.reconcileHelper = reconcileHelper;
            this.settingsClient = settingsClient;
        }

        #region New
        public override async Task<PaypalNewOwnSubscriptionResponse> PaypalNewOwnSubscription(PaypalNewOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("create subscription") };

                if (request?.PaypalSubscriptionID == null)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("PaypalSubscriptionID is required") };

                var sub = await client.GetSubscription(request.PaypalSubscriptionID);
                if (sub == null)
                    return new() { Error = PaymentErrorExtensions.CreateSubscriptionNotFoundError(request.PaypalSubscriptionID) };

                var billing_info = sub.billing_info;
                if (billing_info == null)
                    return new() { Error = PaymentErrorExtensions.CreateProviderError("Paypal", "Invalid billing information") };

                decimal value = 0;
                if (!decimal.TryParse(sub.billing_info?.last_payment?.amount?.value ?? "0", out value))
                    return new() { Error = PaymentErrorExtensions.CreateProviderError("Paypal", "Invalid subscription value") };

                var record = new GenericSubscriptionRecord()
                {
                    UserID = userToken.Id.ToString(),
                    InternalSubscriptionID = Guid.NewGuid().ToString(),
                    ProcessorSubscriptionID = request.PaypalSubscriptionID,
                    AmountCents = (uint)(value * 100),
                    Status = Fragments.Authorization.Payment.SubscriptionStatus.SubscriptionActive,
                    CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                    ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                };

                await subProvider.Save(record);

                var payment = new GenericPaymentRecord()
                {
                    UserID = userToken.Id.ToString(),
                    InternalSubscriptionID = record.InternalSubscriptionID,
                    InternalPaymentID = Guid.NewGuid().ToString(),
                    ProcessorPaymentID = sub.id,
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
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
            }
        }
        #endregion
    }
}
