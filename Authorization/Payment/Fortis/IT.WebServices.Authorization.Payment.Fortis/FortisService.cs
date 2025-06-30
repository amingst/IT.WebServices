using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.Fortis.Clients;
using IT.WebServices.Authorization.Payment.Fortis.Data;
using IT.WebServices.Authorization.Payment.Fortis.Helpers;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Generic;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;

namespace IT.WebServices.Authorization.Payment.Fortis
{
    public class FortisService : FortisInterface.FortisInterfaceBase
    {
        private readonly ILogger<FortisService> logger;
        private readonly ISubscriptionRecordProvider subscriptionProvider;
        private readonly BulkHelper bulkHelper;
        private readonly FortisSubscriptionHelper fortisSubscriptionHelper;
        private readonly FortisTransactionHelper fortisTransactionHelper;
        private readonly SettingsClient settingsClient;

        public FortisService(ILogger<FortisService> logger, ISubscriptionRecordProvider subscriptionProvider, BulkHelper bulkHelper, FortisSubscriptionHelper fortisSubscriptionHelper, FortisTransactionHelper fortisTransactionHelper, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.subscriptionProvider = subscriptionProvider;
            this.bulkHelper = bulkHelper;
            this.fortisSubscriptionHelper = fortisSubscriptionHelper;
            this.fortisTransactionHelper = fortisTransactionHelper;
            this.settingsClient = settingsClient;
        }

        #region Bulk
        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<FortisBulkActionCancelResponse> FortisBulkActionCancel(FortisBulkActionCancelRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new FortisBulkActionCancelResponse());

                var res = new FortisBulkActionCancelResponse();
                res.RunningActions.AddRange(bulkHelper.CancelAction(request.Action, userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new FortisBulkActionCancelResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<FortisBulkActionStartResponse> FortisBulkActionStart(FortisBulkActionStartRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new FortisBulkActionStartResponse());

                var res = new FortisBulkActionStartResponse();
                res.RunningActions.AddRange(bulkHelper.StartAction(request.Action, userToken));
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new FortisBulkActionStartResponse());
            }
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override Task<FortisBulkActionStatusResponse> FortisBulkActionStatus(FortisBulkActionStatusRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return Task.FromResult(new FortisBulkActionStatusResponse());

                var res = new FortisBulkActionStatusResponse();
                res.RunningActions.AddRange(bulkHelper.GetRunningActions());
                return Task.FromResult(res);
            }
            catch
            {
                return Task.FromResult(new FortisBulkActionStatusResponse());
            }
        }
        #endregion

        public override async Task<FortisCancelOwnSubscriptionResponse> FortisCancelOwnSubscription(FortisCancelOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new() { Error = "No user token specified" };

                var subId = request.SubscriptionID.ToGuid();
                if (subId == Guid.Empty)
                    return new() { Error = "No SubscriptionID specified" };

                var record = await subscriptionProvider.GetById(userToken.Id, subId);
                if (record == null)
                    return new() { Error = "Record not found" };

                var res = await fortisSubscriptionHelper.Get(record.SubscriptionID);
                if (res == null)
                    return new() { Error = "SubscriptionId not valid" };

                if (res.Status == FortisAPI.Standard.Models.StatusEnum.Active)
                {
                    var cancelRes = await fortisSubscriptionHelper.Cancel(record.SubscriptionID);
                    if (cancelRes?.Data?.Active != FortisAPI.Standard.Models.ActiveEnum.Enum0)
                        return new() { Error = "Unable to cancel subscription" };
                }

                record.CanceledBy = userToken.Id.ToString();
                record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

                await subscriptionProvider.Save(record);

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

        public override Task<FortisGetAccountDetailsResponse> FortisGetAccountDetails(FortisGetAccountDetailsRequest request, ServerCallContext context)
        {
            var res = new FortisGetAccountDetailsResponse();
            res.Plans = null;
            res.IsTest = settingsClient.PublicData?.Subscription?.Fortis?.IsTest ?? false;
            return Task.FromResult(res);
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<FortisGetOtherSubscriptionRecordsResponse> FortisGetOtherSubscriptionRecords(FortisGetOtherSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var ret = new FortisGetOtherSubscriptionRecordsResponse();
                ret.Records.AddRange(await subscriptionProvider.GetAllByUserId(userId).ToList());

                return ret;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }

            return new();
        }

        public override async Task<FortisGetOwnSubscriptionRecordsResponse> FortisGetOwnSubscriptionRecords(FortisGetOwnSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var ret = new FortisGetOwnSubscriptionRecordsResponse();
                ret.Records.AddRange(await subscriptionProvider.GetAllByUserId(userToken.Id).ToList());

                return ret;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }

            return new();
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
