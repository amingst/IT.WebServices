using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Fragments.Authorization.Payment;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public class ReconcileHelper
    {
        private readonly ILogger logger;
        private readonly IGenericSubscriptionRecordProvider subProvider;
        private readonly IGenericPaymentRecordProvider paymentProvider;
        private readonly FortisSubscriptionHelper fortisSubscriptionHelper;
        private readonly FortisTransactionHelper fortisTransactionHelper;

        private const int YEARS_TO_GO_BACK_FOR_RECONCILE_ALL = 10;

        public ReconcileHelper(ILogger<ReconcileHelper> logger, IGenericSubscriptionRecordProvider subProvider, IGenericPaymentRecordProvider paymentProvider, FortisSubscriptionHelper fortisSubscriptionHelper, FortisTransactionHelper fortisTransactionHelper)
        {
            this.logger = logger;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.fortisSubscriptionHelper = fortisSubscriptionHelper;
            this.fortisTransactionHelper = fortisTransactionHelper;
        }

        public Task ReconcileAll(ONUser user, PaymentBulkActionProgress progress, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //try
            //{
            //    var subs = await fortisSubscriptionHelper.GetAll();
            //    if (subs == null)
            //        throw new Exception("Error pulling subscriptions");

            //    var numSubs = subs.Count();

            //    progress.Progress = 0.01F;

            //    var i = 0;
            //    foreach (var sub in subs)
            //    {
            //        i++;
            //        cancellationToken.ThrowIfCancellationRequested();

            //        await ReconcileSubscription(sub);

            //        progress.Progress = 0.99F * i / numSubs + 0.01F;
            //    }

            //    progress.StatusMessage = "Completed Successfully";
            //    progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            //    progress.Progress = 1;
            //}
            //catch (Exception ex)
            //{
            //    progress.StatusMessage = ex.Message;
            //    progress.CompletedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            //}

        }

        //public async Task<string?> ReconcileSubscription(Guid userId, Guid subscriptionId, ONUser user)
        //{
        //    try
        //    {
        //        var localSub = await subProvider.GetById(userId, subscriptionId);
        //        if (localSub == null)
        //            return "SubscriptionId not valid";

        //        List<FortisPaymentRecord> localPayments = new();
        //        var paymentEnumerable = paymentProvider.GetAllBySubscriptionId(userId, subscriptionId);
        //        await foreach (var payment in paymentEnumerable)
        //            localPayments.Add(payment);

        //        var fortisSub = await fortisSubscriptionHelper.Get(localSub.FortisSubscriptionID, true);
        //        if (fortisSub == null)
        //            return "SubscriptionId not valid";

        //        var fortisPayments = fortisSub.Transactions;

        //        await EnsureSubscription(localSub, fortisSub, user);

        //        return null;
        //    }
        //    catch
        //    {
        //        return "Unknown error";
        //    }
        //}

        //public async Task DoOne(Subscription dbSub)
        //{
        //    var dbTrans = await Transaction.GetAllBySubscription(mysql, dbSub);
        //    var dbTransIds = dbTrans.Select(t => t.TransNum).ToList();

        //    var fSub = await subHelper.Get(dbSub.SubscriptionId, true);
        //    if (fSub == null)
        //        return;

        //    var missingTrans = fSub.Transactions.Where(t => !dbTransIds.Contains(t.Id)).ToList();
        //    if (missingTrans.Count == 0)
        //        return;

        //    foreach (var t in missingTrans)
        //    {
        //        var newTran = new Transaction(t, dbSub);
        //        await newTran.Insert(mysql);

        //        Console.WriteLine($"Subscription: {dbSub.Id} - Trans: {t.Id} Fixed");
        //    }

        //    await memFixer.FixUser(dbSub.UserId);
        //}

        //private async Task EnsureSubscription(FortisSubscriptionRecord localSub, SubscriptionModel fortisSub, ONUser user)
        //{
        //    bool changed = false;

        //    if (fortisSub.StatusEnum == SubscriptionStatus.SubscriptionUnknown)
        //        return;

        //    if (localSub.Status != fortisSub.StatusEnum)
        //    {
        //        localSub.Status = fortisSub.StatusEnum;
        //        changed = true;
        //    }

        //    var amountStr = fortisSub.billing_info?.last_payment?.amount?.value;
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
