﻿using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Authorization.Payment.Fortis.Data;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using IT.WebServices.Fragments.Generic;
using Microsoft.Extensions.Logging;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public class ReconcileHelper
    {
        private readonly ILogger logger;
        private readonly ISubscriptionRecordProvider subProvider;
        private readonly IPaymentRecordProvider paymentProvider;
        private readonly FortisSubscriptionHelper fortisSubscriptionHelper;
        private readonly FortisTransactionHelper fortisTransactionHelper;

        private const int YEARS_TO_GO_BACK_FOR_RECONCILE_ALL = 10;

        public ReconcileHelper(ILogger<ReconcileHelper> logger, ISubscriptionRecordProvider subProvider, IPaymentRecordProvider paymentProvider, FortisSubscriptionHelper fortisSubscriptionHelper, FortisTransactionHelper fortisTransactionHelper)
        {
            this.logger = logger;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.fortisSubscriptionHelper = fortisSubscriptionHelper;
            this.fortisTransactionHelper = fortisTransactionHelper;
        }

        public async Task ReconcileAll(ONUser user, PaymentBulkActionProgress progress, CancellationToken cancellationToken)
        {
            try
            {
                //float stepsToComplete = 12 * YEARS_TO_GO_BACK_FOR_RECONCILE_ALL;
                //var stepsCompleted = 0;

                //var subs = await fortisSubscriptionHelper.GetAll();

                //progress.Progress = 0.01F;

                //while (monthFrom < to)
                //{
                //    cancellationToken.ThrowIfCancellationRequested();

                //    progress.Progress = stepsCompleted / stepsToComplete;

                //    monthFrom = monthTo;
                //    stepsCompleted += 1;
                //}


                //progress.StatusMessage = "Completed Successfully";
                //progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
                //progress.Progress = 1;
            }
            catch (Exception ex)
            {
                progress.StatusMessage = ex.Message;
                progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            }

        }

        //public async Task<string?> ReconcileSubscription(Guid userId, Guid subscriptionId, ONUser user)
        //{
        //    try
        //    {
        //        var localSub = await subProvider.GetById(userId, subscriptionId);
        //        if (localSub == null)
        //            return "SubscriptionId not valid";

        //        List<PaypalPaymentRecord> localPayments = new();
        //        var paymentEnumerable = paymentProvider.GetAllBySubscriptionId(userId, subscriptionId);
        //        await foreach (var payment in paymentEnumerable)
        //            localPayments.Add(payment);

        //        var paypalSub = await client.GetSubscription(localSub.PaypalSubscriptionID);
        //        if (paypalSub == null)
        //            return "SubscriptionId not valid";

        //        var paypalPayments = await client.GetTransactionsForSubscription(localSub.PaypalSubscriptionID);

        //        await EnsureSubscription(localSub, paypalSub, user);

        //        return null;
        //    }
        //    catch
        //    {
        //        return "Unknown error";
        //    }
        //}

        //private async Task EnsureSubscription(PaypalSubscriptionRecord localSub, SubscriptionModel paypalSub, ONUser user)
        //{
        //    bool changed = false;

        //    if (paypalSub.StatusEnum == SubscriptionStatus.SubscriptionUnknown)
        //        return;

        //    if (localSub.Status != paypalSub.StatusEnum)
        //    {
        //        localSub.Status = paypalSub.StatusEnum;
        //        changed = true;
        //    }

        //    var amountStr = paypalSub.billing_info?.last_payment?.amount?.value;
        //    if (!double.TryParse(amountStr, out var amount))
        //        return;

        //    var amountCents = (uint)(amount * 100);

        //    if (localSub.TotalCents != amountCents)
        //    {
        //        localSub.TotalCents = amountCents;
        //        localSub.AmountCents = amountCents;
        //        localSub.TaxCents = 0;
        //        changed = true;
        //    }

        //    if (changed)
        //    {
        //        localSub.ModifiedBy = user.Id.ToString();
        //        localSub.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

        //        await subProvider.Save(localSub);
        //    }
        //}

        //private async Task EnsurePaymentAndSubscription(TransactionInfoModel paypalPayment, List<PaypalSubscriptionRecord> processedSubs, ONUser user)
        //{
        //    if (paypalPayment.paypal_reference_id == null)  //if it's not tied to a subscription... abort...
        //        return;

        //    var localSub = await subProvider.GetByPaypalId(paypalPayment.paypal_reference_id);
        //    if (localSub == null) //if can't find subscription... abort...
        //        return;

        //    if (!processedSubs.Any(s => s.PaypalSubscriptionID.ToLower() == paypalPayment.paypal_reference_id.ToLower()))
        //    {
        //        var paypalSub = await client.GetSubscription(paypalPayment.paypal_reference_id);
        //        if (paypalSub == null) //if can't find subscription... abort...
        //            return;

        //        await EnsureSubscription(localSub, paypalSub, user);
        //        processedSubs.Add(localSub);
        //    }

        //    await EnsurePayment(paypalPayment, localSub, user);
        //}

        //private async Task EnsurePayment(TransactionInfoModel paypalPayment, PaypalSubscriptionRecord localSub, ONUser user)
        //{
        //    var localPayments = paymentProvider.GetAllBySubscriptionId(localSub.UserID.ToGuid(), localSub.SubscriptionID.ToGuid());
        //    var localPayment = localPayments.ToBlockingEnumerable().FirstOrDefault(p => p.PaypalPaymentID.ToLower() == paypalPayment.transaction_id?.ToLower());

        //    if (localPayment == null)
        //    {
        //        await CreateMissingPayment(paypalPayment, localSub, user);
        //        return;
        //    }

        //    bool changed = false;

        //    if (paypalPayment.StatusEnum == PaymentStatus.PaymentUnknown)
        //        return;

        //    if (localPayment.Status != paypalPayment.StatusEnum)
        //    {
        //        localPayment.Status = paypalPayment.StatusEnum;
        //        changed = true;
        //    }

        //    var amountCents = paypalPayment.transaction_amount?.AmountInCents;
        //    if (amountCents == null)
        //        return;

        //    if (localPayment.TotalCents != amountCents)
        //    {
        //        localPayment.TotalCents = amountCents.Value;
        //        localPayment.AmountCents = amountCents.Value;
        //        localPayment.TaxCents = 0;
        //        changed = true;
        //    }

        //    if (changed)
        //    {
        //        localPayment.ModifiedBy = user.Id.ToString();
        //        localPayment.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

        //        await paymentProvider.Save(localPayment);
        //    }
        //}

        //private async Task CreateMissingPayment(TransactionInfoModel paypalPayment, PaypalSubscriptionRecord localSub, ONUser user)
        //{
        //    var amountCents = paypalPayment.transaction_amount?.AmountInCents;
        //    if (amountCents == null)
        //        return;

        //    var record = new PaypalPaymentRecord
        //    {
        //        PaymentID = Guid.NewGuid().ToString(),
        //        UserID = localSub.UserID,
        //        SubscriptionID = localSub.SubscriptionID,
        //        PaypalPaymentID = paypalPayment.transaction_id,
        //        Status = paypalPayment.StatusEnum,
        //        AmountCents = amountCents.Value,
        //        TaxCents = 0,
        //        TaxRateThousandPercents = 0,
        //        TotalCents = amountCents.Value,
        //        CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //        CreatedBy = user.Id.ToString(),
        //    };

        //    var paidOnUtc = paypalPayment.transaction_initiation_date_UTC;
        //    if (paidOnUtc != null)
        //    {
        //        record.PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset((DateTimeOffset)paidOnUtc);
        //        record.PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTimeOffset(((DateTimeOffset)paidOnUtc).AddMonths(1));
        //    }

        //    await paymentProvider.Save(record);
        //}
    }
}
