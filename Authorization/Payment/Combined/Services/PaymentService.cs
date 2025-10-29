using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Generic.Data;
using IT.WebServices.Authorization.Payment.Generic;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using ManualD = IT.WebServices.Authorization.Payment.Manual.Data;
using StripeD = IT.WebServices.Authorization.Payment.Stripe.Data;
using IT.WebServices.Authorization.Payment.Helpers;

namespace IT.WebServices.Authorization.Payment.Combined.Services
{
    [Authorize]
    public class PaymentService : PaymentInterface.PaymentInterfaceBase
    {
        private readonly ILogger logger;
        private readonly Paypal.Clients.PaypalClient paypalClient;
        private readonly Stripe.Clients.StripeClient stripeClient;
        private readonly ManualD.ISubscriptionRecordProvider manualProvider;
        private readonly IGenericOneTimePaymentRecordProvider genericOneTimeProvider;
        private readonly IGenericSubscriptionRecordProvider genericSubProvider;
        private readonly IGenericSubscriptionFullRecordProvider genericFullProvider;
        private readonly GenericPaymentProcessorProvider genericProcessorProvider;
        private readonly ReconcileHelper reconcileHelper;

        public PaymentService(
            ILogger<PaymentService> logger,
            Paypal.Clients.PaypalClient paypalClient,
            Stripe.Clients.StripeClient stripeClient,
            IGenericOneTimePaymentRecordProvider genericOneTimeProvider,
            IGenericSubscriptionRecordProvider genericSubProvider,
            IGenericSubscriptionFullRecordProvider genericFullProvider,
            ManualD.ISubscriptionRecordProvider manualProvider,
            GenericPaymentProcessorProvider genericProcessorProvider,
            ReconcileHelper reconcileHelper
        )
        {
            this.logger = logger;
            this.paypalClient = paypalClient;
            this.stripeClient = stripeClient;
            this.genericOneTimeProvider = genericOneTimeProvider;
            this.genericSubProvider = genericSubProvider;
            this.genericFullProvider = genericFullProvider;
            this.manualProvider = manualProvider;
            this.genericProcessorProvider = genericProcessorProvider;
            this.reconcileHelper = reconcileHelper;
        }

        public override async Task<CancelSubscriptionResponse> CancelOwnSubscription(CancelOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("cancel subscription") };

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No InternalSubscriptionID specified") };

                var record = await genericSubProvider.GetById(userToken.Id, intSubId);
                if (record == null)
                    return new() { Error = PaymentErrorExtensions.CreateSubscriptionNotFoundError(intSubId.ToString()) };

                var provider = genericProcessorProvider.GetProcessor(record);
                return await provider.CancelSubscription(record, userToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.CancelSubscriptionErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetNewDetailsResponse> GetNewDetails(GetNewDetailsRequest request, ServerCallContext context)
        {
            try
            {
                if (request?.DomainName == null)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("Domain name is required") };

                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get payment details") };

                var level = request?.Level ?? 0;
                if (level == 0)
                    return new() { Error = PaymentErrorExtensions.CreateInvalidLevelError("0") };

                return new()
                {
                    //Paypal = await paypalClient.GetNewDetails(level),
                    Stripe = await stripeClient.GetNewDetails(level, userToken, request!.DomainName),
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.GetNewDetailsErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetNewOneTimeDetailsResponse> GetNewOneTimeDetails(GetNewOneTimeDetailsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get one-time payment details") };

                if (string.IsNullOrEmpty(request.InternalID))
                {
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("Internal ID is required") };
                }

                var details = await stripeClient.GetNewOneTimeDetails(request.InternalID, userToken, request.DomainName, request.DifferentPresetPriceCents);

                return new() { Stripe = details };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.PaymentErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetOneTimeRecordResponse> GetOwnOneTimeRecord(GetOwnOneTimeRecordRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get payment record") };

                var intPayId = request.InternalPaymentID.ToGuid();
                if (intPayId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("Invalid payment ID") };

                var record = await genericOneTimeProvider.GetById(userToken.Id, intPayId);

                var res = new GetOneTimeRecordResponse();
                res.Generic = record;

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.GetPaymentErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetOneTimeRecordsResponse> GetOwnOneTimeRecords(GetOwnOneTimeRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get payment records") };

                var records = await genericOneTimeProvider.GetAllByUserId(userToken.Id).ToList();

                var res = new GetOneTimeRecordsResponse();
                res.Generic.AddRange(records);

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.GetPaymentErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetSubscriptionRecordResponse> GetOwnSubscriptionRecord(GetOwnSubscriptionRecordRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get subscription record") };

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("Invalid subscription ID") };

                var baseT = genericFullProvider.GetBySubscriptionId(userToken.Id, intSubId);
                var manualT = manualProvider.GetBySubscriptionId(userToken.Id, intSubId);

                await Task.WhenAll(baseT, manualT);

                var res = new GetSubscriptionRecordResponse();

                if (baseT.Result != null)
                    res.Generic = baseT.Result;

                if (manualT.Result != null)
                    res.Manual = manualT.Result;

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.GetSubscriptionErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<GetSubscriptionRecordsResponse> GetOwnSubscriptionRecords(GetOwnSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("get subscription records") };

                var baseT = genericFullProvider.GetAllByUserId(userToken.Id).ToList();
                var manualT = manualProvider.GetAllByUserId(userToken.Id).ToList();

                await Task.WhenAll(baseT, manualT);

                var res = new GetSubscriptionRecordsResponse();

                if (manualT.Result != null)
                    res.Manual.AddRange(manualT.Result);

                if (baseT.Result != null)
                    res.Generic.AddRange(baseT.Result);

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.GetSubscriptionErrorUnknown, "Unknown error occurred") };
            }
        }

        public override async Task<ReconcileSubscriptionResponse> ReconcileOwnSubscription(ReconcileOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("reconcile subscription") };

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No InternalSubscriptionID specified") };

                var record = await genericFullProvider.GetBySubscriptionId(userToken.Id, intSubId);
                if (record == null)
                    return new() { Error = PaymentErrorExtensions.CreateSubscriptionNotFoundError(intSubId.ToString()) };

                var provider = genericProcessorProvider.GetProcessor(record);
                return await reconcileHelper.ReconcileSubscription(record, userToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new() { Error = PaymentErrorExtensions.CreateError(PaymentErrorReason.ReconcileSubscriptionErrorUnknown, "Unknown error occurred") };
            }
        }
    }
}
