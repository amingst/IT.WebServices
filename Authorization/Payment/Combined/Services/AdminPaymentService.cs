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
    [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER_OR_SERVICE_OR_BOT)]
    public class AdminPaymentService : AdminPaymentInterface.AdminPaymentInterfaceBase
    {
        private readonly ILogger logger;
        private readonly BulkHelper bulkHelper;
        private readonly IGenericOneTimePaymentRecordProvider genericOneTimeProvider;
        private readonly IGenericSubscriptionRecordProvider genericSubProvider;
        private readonly IGenericSubscriptionFullRecordProvider genericFullProvider;
        private readonly ManualD.ISubscriptionRecordProvider manualProvider;
        private readonly GenericPaymentProcessorProvider genericProcessorProvider;
        private readonly ReconcileHelper reconcileHelper;

        public AdminPaymentService(
            ILogger<PaymentService> logger,
            BulkHelper bulkHelper,
            IGenericOneTimePaymentRecordProvider genericOneTimeProvider,
            IGenericSubscriptionRecordProvider genericSubProvider,
            IGenericSubscriptionFullRecordProvider genericFullProvider,
            ManualD.ISubscriptionRecordProvider manualProvider,
            GenericPaymentProcessorProvider genericProcessorProvider,
            ReconcileHelper reconcileHelper
        )
        {
            this.logger = logger;
            this.bulkHelper = bulkHelper;
            this.genericOneTimeProvider = genericOneTimeProvider;
            this.genericSubProvider = genericSubProvider;
            this.genericFullProvider = genericFullProvider;
            this.manualProvider = manualProvider;
            this.genericProcessorProvider = genericProcessorProvider;
            this.reconcileHelper = reconcileHelper;
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<BulkActionCancelResponse> BulkActionCancel(BulkActionCancelRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new BulkActionCancelResponse());

                var res = new BulkActionCancelResponse();
                res.RunningActions.AddRange(bulkHelper.CancelAction(request.Action, userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new BulkActionCancelResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<BulkActionStartResponse> BulkActionStart(BulkActionStartRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new BulkActionStartResponse());

                var res = new BulkActionStartResponse();
                res.RunningActions.AddRange(bulkHelper.StartAction(request.Action, userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new BulkActionStartResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<BulkActionStatusResponse> BulkActionStatus(BulkActionStatusRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new BulkActionStatusResponse());

                var res = new BulkActionStatusResponse();
                res.RunningActions.AddRange(bulkHelper.GetRunningActions());
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new BulkActionStatusResponse());
            }
        }

        public override async Task<CancelSubscriptionResponse> CancelOtherSubscription(CancelOtherSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("cancel subscription") };

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No UserID specified") };

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No InternalSubscriptionID specified") };

                var record = await genericSubProvider.GetById(userId, intSubId);
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

        public override async Task<GetOneTimeRecordResponse> GetOtherOneTimeRecord(GetOtherOneTimeRecordRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var intPayId = request.InternalPaymentID.ToGuid();
                if (intPayId == Guid.Empty)
                    return new();

                var record = await genericOneTimeProvider.GetById(userId, intPayId);

                var res = new GetOneTimeRecordResponse();
                res.Generic = record;

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new();
            }
        }

        public override async Task<GetOneTimeRecordsResponse> GetOtherOneTimeRecords(GetOtherOneTimeRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var records = await genericOneTimeProvider.GetAllByUserId(userId).ToList();

                var res = new GetOneTimeRecordsResponse();
                res.Generic.AddRange(records);

                return res;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unknown Error");
                return new();
            }
        }

        public override async Task<GetSubscriptionRecordResponse> GetOtherSubscriptionRecord(GetOtherSubscriptionRecordRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new();

                var baseT = genericFullProvider.GetBySubscriptionId(userId, intSubId);
                var manualT = manualProvider.GetBySubscriptionId(userId, intSubId);

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
                return new();
            }
        }


        public override async Task<GetSubscriptionRecordsResponse> GetOtherSubscriptionRecords(GetOtherSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var baseT = genericFullProvider.GetAllByUserId(userId).ToList();
                var manualT = manualProvider.GetAllByUserId(userId).ToList();

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
                return new();
            }
        }

        public override async Task<ReconcileSubscriptionResponse> ReconcileOtherSubscription(ReconcileOtherSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = PaymentErrorExtensions.CreateUnauthorizedError("reconcile subscription") };

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No UserID specified") };

                var intSubId = request.InternalSubscriptionID.ToGuid();
                if (intSubId == Guid.Empty)
                    return new() { Error = PaymentErrorExtensions.CreateValidationError("No InternalSubscriptionID specified") };

                var record = await genericFullProvider.GetBySubscriptionId(userId, intSubId);
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
