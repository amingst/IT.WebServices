using System;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using IT.WebServices.Fragments.Authorization;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Generic;
using ManualD = IT.WebServices.Authorization.Payment.Manual.Data;
using FortisD = IT.WebServices.Authorization.Payment.Fortis.Data;
using PaypalD = IT.WebServices.Authorization.Payment.Paypal.Data;
using StripeD = IT.WebServices.Authorization.Payment.Stripe.Data;
using IT.WebServices.Helpers;

namespace IT.WebServices.Authorization.Payment.Service
{
    [Authorize]
    public class PaymentService : PaymentInterface.PaymentInterfaceBase
    {
        private readonly ILogger logger;
        private readonly Paypal.Clients.PaypalClient paypalClient;
        private readonly Stripe.Clients.StripeClient stripeClient;
        private readonly ManualD.ISubscriptionRecordProvider manualProvider;
        private readonly PaypalD.ISubscriptionFullRecordProvider paypalProvider;
        private readonly FortisD.ISubscriptionFullRecordProvider peProvider;
        private readonly StripeD.ISubscriptionFullRecordProvider stripeProvider;
        private readonly StripeD.IOneTimeRecordProvider stripeOneTimeProvider;

        public PaymentService(
            ILogger<PaymentService> logger,
            Paypal.Clients.PaypalClient paypalClient,
            Stripe.Clients.StripeClient stripeClient,
            ManualD.ISubscriptionRecordProvider manualProvider,
            PaypalD.ISubscriptionFullRecordProvider paypalProvider,
            FortisD.ISubscriptionFullRecordProvider peProvider,
            StripeD.ISubscriptionFullRecordProvider stripeProvider,
            StripeD.IOneTimeRecordProvider stripeOneTimeProvider
        )
        {
            this.logger = logger;
            this.paypalClient = paypalClient;
            this.stripeClient = stripeClient;
            this.manualProvider = manualProvider;
            this.paypalProvider = paypalProvider;
            this.peProvider = peProvider;
            this.stripeProvider = stripeProvider;
            this.stripeOneTimeProvider = stripeOneTimeProvider;
        }

        public override async Task<GetNewDetailsResponse> GetNewDetails(
            GetNewDetailsRequest request,
            ServerCallContext context
        )
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var level = request?.Level ?? 0;
            if (level == 0)
                return new();

            return new()
            {
                //Paypal = await paypalClient.GetNewDetails(level),
                Stripe = await stripeClient.GetNewDetails(level, userToken, request.DomainName),
            };
        }

        [Authorize(Roles = ONUser.ROLE_IS_ADMIN_OR_OWNER_OR_SERVICE_OR_BOT)]
        public override async Task<GetOtherSubscriptionRecordsResponse> GetOtherSubscriptionRecords(
            GetOtherSubscriptionRecordsRequest request,
            ServerCallContext context
        )
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var fortisT = peProvider.GetAllByUserId(request.UserID.ToGuid()).ToList();
            var manualT = manualProvider.GetAllByUserId(request.UserID.ToGuid()).ToList();
            var paypalT = paypalProvider.GetAllByUserId(request.UserID.ToGuid()).ToList();
            var stripeT = stripeProvider.GetAllByUserId(request.UserID.ToGuid()).ToList();

            await Task.WhenAll(manualT, paypalT, fortisT, stripeT);

            var res = new GetOtherSubscriptionRecordsResponse();

            if (fortisT.Result != null)
                res.Fortis.AddRange(fortisT.Result);

            if (manualT.Result != null)
                res.Manual.AddRange(manualT.Result);

            if (paypalT.Result != null)
                res.Paypal.AddRange(paypalT.Result);

            if (stripeT.Result != null)
                res.Stripe.AddRange(stripeT.Result);

            return res;
        }

        public override async Task<GetOwnSubscriptionRecordsResponse> GetOwnSubscriptionRecords(
            GetOwnSubscriptionRecordsRequest request,
            ServerCallContext context
        )
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var fortisT = peProvider.GetAllByUserId(userToken.Id).ToList();
            var manualT = manualProvider.GetAllByUserId(userToken.Id).ToList();
            var paypalT = paypalProvider.GetAllByUserId(userToken.Id).ToList();
            var stripeT = stripeProvider.GetAllByUserId(userToken.Id).ToList();

            await Task.WhenAll(manualT, paypalT, fortisT, stripeT);

            var res = new GetOwnSubscriptionRecordsResponse();

            if (fortisT.Result != null)
                res.Fortis.AddRange(fortisT.Result);

            if (manualT.Result != null)
                res.Manual.AddRange(manualT.Result);

            if (paypalT.Result != null)
                res.Paypal.AddRange(paypalT.Result);

            if (stripeT.Result != null)
                res.Stripe.AddRange(stripeT.Result);

            return res;
        }

        public override async Task<GetNewOneTimeDetailsResponse> GetNewOneTimeDetails(GetNewOneTimeDetailsRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            if (string.IsNullOrEmpty(request.InternalID))
            {
                return new();
            }

            var details = await stripeClient.GetNewOneTimeDetails(request.InternalID, userToken, request.DomainName, request.DifferentPresetPriceCents);

            return new() { Stripe = details };
        }

        // TODO: Implement
        public override async Task<GetOwnOneTimeRecordsResponse> GetOwnOneTimeRecords(GetOwnOneTimeRecordsRequest request, ServerCallContext context)
        {
            var userToken = ONUserHelper.ParseUser(context.GetHttpContext());
            if (userToken == null)
                return new();

            var records = await stripeOneTimeProvider.GetAllByUserId(userToken.Id);

            var res = new GetOwnOneTimeRecordsResponse();
            res.Stripe.AddRange(records);

            return res;
        }
    }
}
