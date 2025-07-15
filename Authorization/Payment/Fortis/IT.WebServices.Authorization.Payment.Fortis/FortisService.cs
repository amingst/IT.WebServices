using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using IT.WebServices.Authorization.Payment.Generic.Data;

namespace IT.WebServices.Authorization.Payment.Fortis
{
    public class FortisService : FortisInterface.FortisInterfaceBase
    {
        private readonly ILogger<FortisService> logger;
        private readonly IGenericSubscriptionRecordProvider subscriptionProvider;
        private readonly FortisSubscriptionHelper fortisSubscriptionHelper;
        private readonly FortisTransactionHelper fortisTransactionHelper;
        private readonly SettingsClient settingsClient;

        public FortisService(ILogger<FortisService> logger, IGenericSubscriptionRecordProvider subscriptionProvider, FortisSubscriptionHelper fortisSubscriptionHelper, FortisTransactionHelper fortisTransactionHelper, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.subscriptionProvider = subscriptionProvider;
            this.fortisSubscriptionHelper = fortisSubscriptionHelper;
            this.fortisTransactionHelper = fortisTransactionHelper;
            this.settingsClient = settingsClient;
        }

        public override async Task<FortisNewOwnSubscriptionResponse> FortisNewOwnSubscription(FortisNewOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                if (request?.TransactionID == null)
                    return new() { Error = "TransactionId not valid" };

                var trans = await fortisTransactionHelper.Get(request.TransactionID);
                if (trans == null)
                    return new() { Error = "TransactionId not valid" };

                //decimal value = 0;
                //if (!decimal.TryParse(sub.billing_info?.last_payment?.amount?.value ?? "0", out value))
                //    return new() { Error = "Subscription Value not valid" };

                //var record = new SubscriptionRecord()
                //{
                //    UserID = Google.Protobuf.ByteString.CopyFrom(userToken.Id.ToByteArray()),
                //    Level = (uint)value,
                //    ChangedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                //    LastPaidUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                //    SubscriptionId = request.SubscriptionId,
                //    PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.billing_info.next_billing_time),
                //    RenewsOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.billing_info.next_billing_time),
                //};

                //await subscriptionProvider.Save(record);

                //return new()
                //{
                //    Record = record
                //};
            }
            catch
            {
            }

            return new() { Error = "Unknown error" };
        }

        //public override async Task<StartNewSubscriptionResponse> StartNewSubscription(StartNewSubscriptionRequest request, ServerCallContext context)
        //{
        //    try
        //    {
        //        var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
        //        if (userToken == null)
        //            return new StartNewSubscriptionResponse() { Error = "No user token specified" };

        //        if ((request?.Level ?? 0) < 1)
        //            return new StartNewSubscriptionResponse() { Error = "Level not valid" };

        //        var intentToken = await client.GetNewPaymentIntent(request.Level);

        //        return new StartNewSubscriptionResponse()
        //        {
        //            ClientToken = intentToken
        //        };
        //    }
        //    catch
        //    {
        //        return new StartNewSubscriptionResponse() { Error = "Unknown error" };
        //    }
        //}
    }
}
