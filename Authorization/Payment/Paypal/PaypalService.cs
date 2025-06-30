using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Paypal.Clients;
using IT.WebServices.Authorization.Payment.Paypal.Data;
using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;
using IT.WebServices.Fragments.Generic;
using Microsoft.AspNetCore.Authorization;
using IT.WebServices.Authorization.Payment.Paypal.Helpers;

namespace IT.WebServices.Authorization.Payment.Paypal
{
    public class PaypalService : PaypalInterface.PaypalInterfaceBase
    {
        private readonly ILogger<PaypalService> logger;
        private readonly ISubscriptionFullRecordProvider fullProvider;
        private readonly ISubscriptionRecordProvider subProvider;
        private readonly IPaymentRecordProvider paymentProvider;
        private readonly BulkHelper bulkHelper;
        private readonly PaypalClient client;
        private readonly ReconcileHelper reconcileHelper;
        private readonly SettingsClient settingsClient;

        public PaypalService(ILogger<PaypalService> logger, ISubscriptionFullRecordProvider fullProvider, ISubscriptionRecordProvider subProvider, IPaymentRecordProvider paymentProvider, BulkHelper bulkHelper, PaypalClient client, ReconcileHelper reconcileHelper, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.fullProvider = fullProvider;
            this.subProvider = subProvider;
            this.paymentProvider = paymentProvider;
            this.bulkHelper = bulkHelper;
            this.client = client;
            this.reconcileHelper = reconcileHelper;
            this.settingsClient = settingsClient;
        }

        #region Bulk
        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<PaypalBulkActionCancelResponse> PaypalBulkActionCancel(PaypalBulkActionCancelRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new PaypalBulkActionCancelResponse());

                var res = new PaypalBulkActionCancelResponse();
                res.RunningActions.AddRange(bulkHelper.CancelAction(request.Action, userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new PaypalBulkActionCancelResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<PaypalBulkActionStatusResponse> PaypalBulkActionStatus(PaypalBulkActionStatusRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new PaypalBulkActionStatusResponse());

                var res = new PaypalBulkActionStatusResponse();
                res.RunningActions.AddRange(bulkHelper.GetRunningActions());
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new PaypalBulkActionStatusResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<PaypalBulkActionStartResponse> PaypalBulkActionStart(PaypalBulkActionStartRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new PaypalBulkActionStartResponse());

                var res = new PaypalBulkActionStartResponse();
                res.RunningActions.AddRange(bulkHelper.StartAction(request.Action ,userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new PaypalBulkActionStartResponse());
            }
        }
        #endregion

        #region Cancel Subscription
        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<PaypalCancelOtherSubscriptionResponse> PaypalCancelOtherSubscription(PaypalCancelOtherSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                if (request?.UserID == null)
                    return new() { Error = "No UserId specified" };

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new() { Error = "No UserId specified" };

                Guid subscriptionId;
                if (!Guid.TryParse(request.SubscriptionID, out subscriptionId))
                    return new() { Error = "No SubscriptionID specified" };

                var response = await CancelSubscription(userId, subscriptionId, userToken, request.Reason);

                return new()
                {
                    Record = response.record ?? new(),
                    Error = response.error ?? "",
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
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

                var response = await CancelSubscription(userToken.Id, subscriptionId, userToken, request.Reason);

                return new()
                {
                    Record = response.record ?? new(),
                    Error = response.error ?? "",
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
        }

        private async Task<(PaypalSubscriptionRecord? record, string? error)> CancelSubscription(Guid userId, Guid subscriptionId, ONUser userToken, string reason)
        {
            var record = await subProvider.GetById(userId, subscriptionId);
            if (record == null)
                return (record: null, error: "Record not found");

            var sub = await client.GetSubscription(record.PaypalSubscriptionID);
            if (sub == null)
                return (record: null, error: "SubscriptionId not valid");

            if (sub.status == "ACTIVE")
            {
                var canceled = await client.CancelSubscription(record.PaypalSubscriptionID, reason ?? "None");
                if (!canceled)
                    return (record: null, error: "Unable to cancel subscription");
            }

            record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            record.ModifiedBy = userToken.Id.ToString();
            record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);
            record.CanceledBy = userToken.Id.ToString();
            record.Status = Fragments.Authorization.Payment.SubscriptionStatus.SubscriptionStopped;

            await subProvider.Save(record);

            return (record, error: null);
        }
        #endregion

        #region Get Subscription Records
        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<PaypalGetOtherSubscriptionRecordsResponse> PaypalGetOtherSubscriptionRecords(PaypalGetOtherSubscriptionRecordsRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            if (request?.UserID == null)
                return new();

            var userId = request.UserID.ToGuid();
            if (userId == Guid.Empty)
                return new();

            var res = new PaypalGetOtherSubscriptionRecordsResponse();
            res.Records.AddRange(await fullProvider.GetAllByUserId(userId).ToList());

            return res;
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
        #endregion

        #region New
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
        #endregion

        #region Reconcile
        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<PaypalReconcileOtherSubscriptionResponse> PaypalReconcileOtherSubscription(PaypalReconcileOtherSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                var userId = (request.UserID ?? "").ToGuid();
                if (userId == Guid.Empty)
                    return new() { Error = "SubscriptionId not valid" };

                var subscriptionId = (request.SubscriptionID ?? "").ToGuid();
                if (subscriptionId == Guid.Empty)
                    return new() { Error = "SubscriptionId not valid" };

                var error = await reconcileHelper.ReconcileSubscription(userId, subscriptionId, userToken);
                if (error != null)
                    return new() { Error = error };


                var record = await fullProvider.GetBySubscriptionId(userToken.Id, subscriptionId);

                return new()
                {
                    Record = record,
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
        }

        public override async Task<PaypalReconcileOwnSubscriptionResponse> PaypalReconcileOwnSubscription(PaypalReconcileOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                var subscriptionId = (request.SubscriptionID ?? "").ToGuid();
                if (subscriptionId == Guid.Empty)
                    return new() { Error = "SubscriptionId not valid" };

                var error = await reconcileHelper.ReconcileSubscription(userToken.Id, subscriptionId, userToken);
                if (error != null)
                    return new() { Error = error };


                var record = await fullProvider.GetBySubscriptionId(userToken.Id, subscriptionId);

                return new()
                {
                    Record = record,
                };
            }
            catch
            {
                return new() { Error = "Unknown error" };
            }
        }
        #endregion
    }
}
