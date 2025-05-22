using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Authorization.Payment.ParallelEconomy.Clients;
using IT.WebServices.Authorization.Payment.ParallelEconomy.Data;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using IT.WebServices.Fragments.Generic;
using Microsoft.AspNetCore.Authorization;
using IT.WebServices.Helpers;
using IT.WebServices.Settings;

namespace IT.WebServices.Authorization.Payment.ParallelEconomy
{
    public class ParallelEconomyService : ParallelEconomyInterface.ParallelEconomyInterfaceBase
    {
        private readonly ILogger<ParallelEconomyService> logger;
        private readonly ISubscriptionRecordProvider subscriptionProvider;
        private readonly ParallelEconomyClient client;
        private readonly SettingsClient settingsClient;

        public ParallelEconomyService(ILogger<ParallelEconomyService> logger, ISubscriptionRecordProvider subscriptionProvider, ParallelEconomyClient client, SettingsClient settingsClient)
        {
            this.logger = logger;
            this.subscriptionProvider = subscriptionProvider;
            this.client = client;
            this.settingsClient = settingsClient;
        }

        public override async Task<CancelOwnSubscriptionResponse> CancelOwnSubscription(CancelOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new CancelOwnSubscriptionResponse() { Error = "No user token specified" };

                var subId = request.SubscriptionID.ToGuid();
                if (subId == Guid.Empty)
                    return new() { Error = "No SubscriptionID specified" };

                var record = await subscriptionProvider.GetById(userToken.Id, subId);
                if (record == null)
                    return new CancelOwnSubscriptionResponse() { Error = "Record not found" };

                var res = await client.GetSubscription(record.SubscriptionID);
                if (res == null)
                    return new CancelOwnSubscriptionResponse() { Error = "SubscriptionId not valid" };

                if (res.Data.Status == FortisAPI.Standard.Models.StatusEnum.Active)
                {
                    var canceled = await client.CancelSubscription(record.SubscriptionID, request.Reason ?? "None");
                    if (!canceled)
                        return new CancelOwnSubscriptionResponse() { Error = "Unable to cancel subscription" };
                }

                record.CanceledBy = userToken.Id.ToString();
                record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow);

                await subscriptionProvider.Save(record);

                return new CancelOwnSubscriptionResponse()
                {
                    Record = record
                };
            }
            catch
            {
                return new CancelOwnSubscriptionResponse() { Error = "Unknown error" };
            }
        }

        public override Task<GetAccountDetailsResponse> GetAccountDetails(GetAccountDetailsRequest request, ServerCallContext context)
        {
            var res = new GetAccountDetailsResponse();
            res.Plans = null;
            res.IsTest = settingsClient.PublicData?.Subscription?.ParallelEconomy?.IsTest ?? false;
            return Task.FromResult(res);
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<GetOtherSubscriptionRecordsResponse> GetOtherSubscriptionRecords(GetOtherSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var userId = request.UserID.ToGuid();
                if (userId == Guid.Empty)
                    return new();

                var ret = new GetOtherSubscriptionRecordsResponse();
                ret.Records.AddRange(await subscriptionProvider.GetAllByUserId(userId).ToList());

                return ret;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }

            return new();
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER)]
        public override async Task<GetOtherSubscriptionRecordResponse> GetOtherSubscriptionRecord(GetOtherSubscriptionRecordRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var userId = request.UserID.ToGuid();
            if (userId == Guid.Empty)
                return new();

            var subId = request.SubscriptionID.ToGuid();
            if (subId == Guid.Empty)
                return new();

            return new()
            {
                Record = await subscriptionProvider.GetById(userId, subId)
            };
        }

        public override async Task<GetOwnSubscriptionRecordsResponse> GetOwnSubscriptionRecords(GetOwnSubscriptionRecordsRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new();

                var ret = new GetOwnSubscriptionRecordsResponse();
                ret.Records.AddRange(await subscriptionProvider.GetAllByUserId(userToken.Id).ToList());

                return ret;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error");
            }

            return new();
        }

        public override async Task<GetOwnSubscriptionRecordResponse> GetOwnSubscriptionRecord(GetOwnSubscriptionRecordRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var subId = request.SubscriptionID.ToGuid();
            if (subId == Guid.Empty)
                return new();

            return new()
            {
                Record = await subscriptionProvider.GetById(userToken.Id, subId)
            };
        }

        public override async Task<NewOwnSubscriptionResponse> NewOwnSubscription(NewOwnSubscriptionRequest request, ServerCallContext context)
        {
            try
            {
                var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
                if (userToken == null)
                    return new NewOwnSubscriptionResponse() { Error = "No user token specified" };

                if (request?.TransactionID == null)
                    return new NewOwnSubscriptionResponse() { Error = "TransactionId not valid" };

                var trans = await client.GetTransaction(request.TransactionID);
                if (trans == null)
                    return new NewOwnSubscriptionResponse() { Error = "TransactionId not valid" };

                //decimal value = 0;
                //if (!decimal.TryParse(sub.billing_info?.last_payment?.amount?.value ?? "0", out value))
                //    return new NewOwnSubscriptionResponse() { Error = "Subscription Value not valid" };

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

                //return new NewOwnSubscriptionResponse()
                //{
                //    Record = record
                //};
            }
            catch
            {
            }

            return new NewOwnSubscriptionResponse() { Error = "Unknown error" };
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
