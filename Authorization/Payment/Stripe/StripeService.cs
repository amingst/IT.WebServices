using Grpc.Core;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Authorization.Payment.Stripe.Clients;
using IT.WebServices.Authorization.Payment.Stripe.Data;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace IT.WebServices.Authorization.Payment.Stripe
{
    public class StripeService : StripeInterface.StripeInterfaceBase
    {
        private readonly ILogger<StripeService> logger;
        private readonly IGenericSubscriptionFullRecordProvider fullProvider;
        private readonly IGenericSubscriptionRecordProvider subscriptionProvider;
        private readonly IGenericPaymentRecordProvider paymentProvider;
        private readonly StripeClient client;
        private readonly SettingsClient settingsClient;

        public StripeService(
            ILogger<StripeService> logger,
            IGenericSubscriptionFullRecordProvider fullProvider,
            IGenericSubscriptionRecordProvider subscriptionProvider,
            IGenericPaymentRecordProvider paymentProvider,
            StripeClient client,
            SettingsClient settingsClient
        )
        {
            this.logger = logger;
            this.fullProvider = fullProvider;
            this.subscriptionProvider = subscriptionProvider;
            this.paymentProvider = paymentProvider;
            this.client = client;
            this.settingsClient = settingsClient;
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<StripeCheckOtherSubscriptionResponse> StripeCheckOtherSubscription(
            StripeCheckOtherSubscriptionRequest request,
            ServerCallContext context
        )
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("stripe operation") };

                var userId = request.UserID.ToGuid();

                var customer = await client.GetCustomerByUserId(userId);
                if (customer == null)
                    return new() { };

                var stripeSubs = await client.GetSubscriptionsByCustomerId(customer.Id);

                var dbSubs = await subscriptionProvider.GetAllByUserId(userId).ToList();

                foreach (var stripeSub in stripeSubs)
                {
                    var dbSub = dbSubs.FirstOrDefault(s => s.ProcessorSubscriptionID == stripeSub.Id);
                    if (dbSub == null)
                    {
                        dbSub = new()
                        {
                            UserID = userId.ToString(),
                            InternalSubscriptionID = Guid.NewGuid().ToString(),
                            ProcessorSubscriptionID = stripeSub.Id.ToString(),
                            ProcessorCustomerID = customer.Id,
                            CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.Created),
                            ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                            Status = ConvertStatus(stripeSub.Status),
                            AmountCents = (uint)(
                                stripeSub.Items.FirstOrDefault()?.Plan?.Amount ?? 0
                            ),
                        };

                        await subscriptionProvider.Save(dbSub);

                        var dbPayment = new GenericPaymentRecord()
                        {
                            UserID = userId.ToString(),
                            InternalSubscriptionID = dbSub.InternalSubscriptionID,
                            InternalPaymentID = Guid.NewGuid().ToString(),
                            ProcessorPaymentID = stripeSub.LatestInvoiceId,
                            AmountCents = dbSub.AmountCents,
                            Status =
                                dbSub.Status == SubscriptionStatus.SubscriptionActive
                                    ? PaymentStatus.PaymentComplete
                                    : PaymentStatus.PaymentFailed,
                            CreatedOnUTC = dbSub.CreatedOnUTC,
                            ModifiedOnUTC = dbSub.ModifiedOnUTC,
                            PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.CurrentPeriodStart),
                            PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.CurrentPeriodEnd.AddDays(5)),
                        };

                        await paymentProvider.Save(dbPayment);
                    }
                }

                var ret = new StripeCheckOtherSubscriptionResponse();
                ret.Records.AddRange(await fullProvider.GetAllByUserId(userId).ToList());

                return ret;
            }
            catch
            {
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<StripeCheckOwnSubscriptionResponse> StripeCheckOwnSubscription(StripeCheckOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("stripe operation") };

                var customer = await client.GetCustomerByUserId(userToken.Id);
                if (customer == null)
                    return new() { };

                var stripeSubs = await client.GetSubscriptionsByCustomerId(customer.Id);

                var dbSubs = await subscriptionProvider.GetAllByUserId(userToken.Id).ToList();

                foreach (var stripeSub in stripeSubs)
                {
                    var dbSub = dbSubs.FirstOrDefault(s => s.ProcessorSubscriptionID == stripeSub.Id);
                    if (dbSub == null)
                    {
                        dbSub = new()
                        {
                            UserID = userToken.Id.ToString(),
                            InternalSubscriptionID = Guid.NewGuid().ToString(),
                            ProcessorSubscriptionID = stripeSub.Id.ToString(),
                            ProcessorCustomerID = customer.Id,
                            CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.Created),
                            ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
                            Status = ConvertStatus(stripeSub.Status),
                            AmountCents = (uint)(stripeSub.Items.FirstOrDefault()?.Plan?.Amount ?? 0),
                        };

                        await subscriptionProvider.Save(dbSub);

                        var dbPayment = new GenericPaymentRecord()
                        {
                            UserID = userToken.Id.ToString(),
                            InternalSubscriptionID = dbSub.InternalSubscriptionID,
                            InternalPaymentID = Guid.NewGuid().ToString(),
                            ProcessorPaymentID = stripeSub.LatestInvoiceId,
                            AmountCents = dbSub.AmountCents,
                            Status =
                                dbSub.Status == SubscriptionStatus.SubscriptionActive
                                    ? PaymentStatus.PaymentComplete
                                    : PaymentStatus.PaymentFailed,
                            CreatedOnUTC = dbSub.CreatedOnUTC,
                            ModifiedOnUTC = dbSub.ModifiedOnUTC,
                            PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.CurrentPeriodStart),
                            PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripeSub.CurrentPeriodEnd.AddDays(5)),
                        };

                        await paymentProvider.Save(dbPayment);
                    }
                }

                //dbSubs = await subscriptionProvider.GetAllByUserId(userToken.Id);
                //foreach (var dbSub in dbSubs)
                //    await EnsureAllPayments(dbSub);

                var ret = new StripeCheckOwnSubscriptionResponse();
                ret.Records.AddRange(await fullProvider.GetAllByUserId(userToken.Id).ToList());

                return ret;
            }
            catch
            {
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
            }
        }

        //public override async Task<StripeCheckOwnOneTimeResponse> StripeCheckOwnOneTime(StripeCheckOwnOneTimeRequest request, ServerCallContext context)
        //{
        //    try
        //    {
        //        var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
        //        if (userToken == null)
        //            return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("stripe operation") };

        //        var customer = await client.GetCustomerByUserId(userToken.Id);
        //        if (customer == null)
        //            return new() { };

        //        var payments = await client.GetOneTimePaymentsByCustomerId(customer.Id);

        //        var dbPayments = await oneTimeProvider.GetAllByUserId(userToken.Id);

        //        foreach (var stripePayment in payments)
        //        {
        //            var dbPayment = dbPayments.FirstOrDefault(s => s.StripePaymentID == stripePayment.Id);
        //            if (dbPayment == null)
        //            {
        //                var checkout = await client.GetCheckoutSessionByPaymentIntentId(stripePayment.Id);
        //                if (checkout == null)
        //                    continue;

        //                var lineItem = checkout.LineItems.FirstOrDefault();
        //                if (lineItem == null)
        //                    continue;

        //                dbPayment = new()
        //                {
        //                    UserID = userToken.Id.ToString(),
        //                    PaymentID = Guid.NewGuid().ToString(),
        //                    StripePaymentID = stripePayment.Id.ToString(),
        //                    InternalID = lineItem.Price.ProductId.Replace(StripeClient.PRODUCT_ONETIME_PREFIX, ""),
        //                    Status = ConvertPaymentStatus(stripePayment.Status),
        //                    AmountCents = (uint)stripePayment.Amount,
        //                    TaxCents = 0,
        //                    TaxRateThousandPercents = 0,
        //                    TotalCents = (uint)stripePayment.Amount,
        //                    CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripePayment.Created),
        //                    CreatedBy = userToken.Id.ToString(),
        //                    ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //                    ModifiedBy = userToken.Id.ToString(),
        //                    PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(stripePayment.Created),
        //                };

        //                await oneTimeProvider.Save(dbPayment);
        //            }
        //            else
        //            {
        //                var newStatus = ConvertPaymentStatus(stripePayment.Status);
        //                if (dbPayment.Status != newStatus)
        //                {
        //                    dbPayment.Status = newStatus;
        //                    dbPayment.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
        //                    dbPayment.ModifiedBy = userToken.Id.ToString();

        //                    await oneTimeProvider.Save(dbPayment);
        //                }
        //            }
        //        }

        //        var ret = new StripeCheckOwnOneTimeResponse();
        //        ret.Records.AddRange(await oneTimeProvider.GetAllByUserId(userToken.Id));

        //        return ret;
        //    }
        //    catch
        //    {
        //        return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
        //    }
        //}

        //private async Task EnsureAllPayments(ONUser userToken, StripeSubscriptionRecord dbSub)
        //{
        //    var dbPayments = paymentProvider.GetAllBySubscriptionId(userToken.Id, dbSub.SubscriptionID.ToGuid());
        //    var stripePayments = client.GetPaymentsBySubscriptionId(dbSub.StripeSubscriptionID);
        //}

        private SubscriptionStatus ConvertStatus(string status)
        {
            switch (status)
            {
                case "incomplete":
                case "unpaid":
                    return SubscriptionStatus.SubscriptionPending;
                case "incomplete_expired":
                case "canceled":
                    return SubscriptionStatus.SubscriptionStopped;
                case "active":
                    return SubscriptionStatus.SubscriptionActive;
                case "paused":
                case "past_due":
                default:
                    return SubscriptionStatus.SubscriptionPaused;
            }
        }

        private PaymentStatus ConvertPaymentStatus(string status)
        {
            switch (status)
            {
                case "requires_payment_method":
                case "requires_confirmation":
                case "requires_capture":
                case "requires_action":
                case "processing":
                    return PaymentStatus.PaymentPending;
                case "succeeded":
                    return PaymentStatus.PaymentComplete;
                case "canceled":
                default:
                    return PaymentStatus.PaymentFailed;
            }
        }

        [Authorize(Roles = ONUser.ROLE_CAN_CREATE_CONTENT)]
        public override async Task<StripeEnsureOneTimeProductResponse> StripeEnsureOneTimeProduct(StripeEnsureOneTimeProductRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("stripe operation") };

                var product = await client.EnsureOneTimeProduct(request);
                if (product == null)
                    return new() { Error = PaymentErrorExtensions.CreateProviderError("Stripe", "Failed to get response from Stripe client") };

                var price = await client.EnsureOneTimePrice(request, product);
                if (price == null)
                    return new() { Error = PaymentErrorExtensions.CreateProviderError("Stripe", "Failed to get response from Stripe client") };

                await client.EnsureOneTimeProductDefaultPrice(product, price);

                return new();
            }
            catch (Exception e)
            {
                return new() { Error = PaymentErrorExtensions.CreateProviderError("Stripe", e.Message) };
            }
        }

        //public override async Task<StripeGetOwnSubscriptionRecordsResponse> StripeGetOwnSubscriptionRecords(StripeGetOwnSubscriptionRecordsRequest request, ServerCallContext context)
        //{
        //    var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
        //    if (userToken == null)
        //        return new ();

        //    var res = new StripeGetOwnSubscriptionRecordsResponse();
        //    res.Records.AddRange(await dataMerger.GetAllByUserId(userToken.Id));

        //    return res;
        //}

        //public override async Task<StripeNewOwnSubscriptionResponse> StripeNewOwnSubscription(StripeNewOwnSubscriptionRequest request, ServerCallContext context)
        //{
        //    try
        //    {
        //        var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
        //        if (userToken == null)
        //            return new () { Error = PaymentErrorExtensions.CreateUnauthorizedError("stripe operation") };

        //        if (request?.SubscriptionId == null)
        //            return new () { Error = "SubscriptionId not valid" };

        //        var sub = await client.GetSubscription(request.SubscriptionId);
        //        if (sub == null)
        //            return new () { Error = "SubscriptionId not valid" };

        //        var billing_info = sub.billing_info;
        //        if (billing_info == null)
        //            return new () { Error = "SubscriptionId not valid" };

        //        decimal value = 0;
        //        if (!decimal.TryParse(sub.billing_info?.last_payment?.amount?.value ?? "0", out value))
        //            return new () { Error = "Subscription Value not valid" };

        //        var record = new StripeSubscriptionRecord()
        //        {
        //            UserID = userToken.Id.ToString(),
        //            SubscriptionID = Guid.NewGuid().ToString(),
        //            StripeSubscriptionID = request.StripeSubscriptionId,
        //            AmountCents = (uint)(value * 100),
        //            Status = Fragments.Authorization.Payment.SubscriptionStatus.SubscriptionActive,
        //            CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //            LastPaidUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.create_time),
        //            ChangedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //            PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(billing_info.next_billing_time),
        //            RenewsOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(billing_info.next_billing_time),
        //        };

        //        await subscriptionProvider.Save(record);

        //        var payment = new StripePaymentRecord()
        //        {
        //            UserID = userToken.Id.ToString(),
        //            SubscriptionID = record.SubscriptionID,
        //            PaymentID = Guid.NewGuid().ToString(),
        //            StripePaymentID = sub.id,
        //            AmountCents = (uint)(value * 100),
        //            Status = Fragments.Authorization.Payment.PaymentStatus.PaymentComplete,
        //            CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //            PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(sub.create_time),
        //            ChangedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow),
        //            PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(billing_info.next_billing_time),
        //        };

        //        await paymentProvider.Save(payment);

        //        return new ()
        //        {
        //            Record = record
        //        };
        //    }
        //    catch
        //    {
        //        return new () { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
        //    }
        //}
    }
}
