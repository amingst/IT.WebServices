using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;

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

        //public override async Task<FortisFinishOwnSubscriptionResponse> FortisFinishOwnSubscription(FortisFinishOwnSubscriptionRequest request, ServerCallContext context)
        //{
        //    try
        //    {
        //        var utcNow = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
        //        var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
        //        if (userToken == null)
        //            return new() { Error = "No user token specified" };

        //        if (request == null)
        //            return new() { Error = "Request not valid" };

        //        if (string.IsNullOrWhiteSpace(request.TransactionID))
        //            return new() { Error = "TransactionID not valid" };

        //        var transaction = await fortisTransactionHelper.Get(request.TransactionID);
        //        if (transaction == null)
        //            return new() { Error = "TransactionID not valid" };
        //        if (transaction.Status != PaymentStatus.PaymentComplete)
        //            return new() { Error = "Transaction status not valid" };

        //        var sub = await fortisSubscriptionHelper.CreateFromTransaction(request.TransactionID, userToken, 1);
        //        if (sub == null)
        //            return new() { Error = "SessionId not valid" };

        //        var newSubId = Guid.NewGuid().ToString();

        //        var curSubRecord = await subscriptionProvider.GetByProcessorId(sub.Id);
        //        if (curSubRecord != null)
        //            newSubId = curSubRecord.InternalSubscriptionID;

        //        decimal value = sub.Items?.FirstOrDefault()?.Price?.UnitAmountDecimal ?? 0;
        //        if (value <= 0)
        //            return new() { Error = "Subscription Value not valid" };

        //        var newSubRecord = new GenericSubscriptionRecord()
        //        {
        //            UserID = userToken.Id.ToString(),
        //            InternalSubscriptionID = newSubId,
        //            ProcessorName = PaymentConstants.PROCESSOR_NAME_STRIPE,
        //            ProcessorCustomerID = sub.CustomerId,
        //            ProcessorSubscriptionID = sub.Id,
        //            Status = SubscriptionStatus.SubscriptionActive,
        //            AmountCents = (uint)(value),
        //            TaxCents = 0,
        //            TaxRateThousandPercents = 0,
        //            TotalCents = (uint)(value),
        //            CreatedOnUTC = utcNow,
        //            ModifiedOnUTC = utcNow,
        //            CreatedBy = userToken.Id.ToString(),
        //            ModifiedBy = userToken.Id.ToString(),
        //        };

        //        var newPaymentRecord = new GenericPaymentRecord()
        //        {
        //            UserID = userToken.Id.ToString(),
        //            InternalSubscriptionID = newSubRecord.InternalSubscriptionID,
        //            InternalPaymentID = Guid.NewGuid().ToString(),
        //            ProcessorPaymentID = sub.LatestInvoiceId,
        //            Status = PaymentStatus.PaymentComplete,
        //            AmountCents = (uint)(value),
        //            TaxCents = 0,
        //            TaxRateThousandPercents = 0,
        //            TotalCents = (uint)(value),
        //            CreatedOnUTC = utcNow,
        //            ModifiedOnUTC = utcNow,
        //            PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.Created),
        //            PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.Created.AddDays(1)),
        //            CreatedBy = userToken.Id.ToString(),
        //            ModifiedBy = userToken.Id.ToString(),
        //        };

        //        var fullRecord = new GenericSubscriptionFullRecord()
        //        {
        //            SubscriptionRecord = newSubRecord,
        //        };
        //        fullRecord.Payments.Add(newPaymentRecord);

        //        //await fullProvider.Save(fullRecord);

        //        return new()
        //        {
        //            Record = newSubRecord
        //        };
        //    }
        //    catch
        //    {
        //        return new() { Error = "Unknown error" };
        //    }
        //}
    }
}
