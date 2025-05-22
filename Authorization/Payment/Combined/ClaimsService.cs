using Grpc.Core;
using Microsoft.Extensions.Logging;
using IT.WebServices.Authentication;
using ManualD = IT.WebServices.Authorization.Payment.Manual.Data;
using PED = IT.WebServices.Authorization.Payment.ParallelEconomy.Data;
using PaypalD = IT.WebServices.Authorization.Payment.Paypal.Data;
using StripeD = IT.WebServices.Authorization.Payment.Stripe.Data;
using IT.WebServices.Fragments.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using IT.WebServices.Fragments.Authorization.Payment.Stripe;
using IT.WebServices.Helpers;
using IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy;
using IT.WebServices.Fragments.Authorization.Payment.Manual;

namespace IT.WebServices.Authorization.Payment.Service
{
    public class ClaimsService : ClaimsInterface.ClaimsInterfaceBase
    {
        private readonly ILogger<ClaimsService> logger;
        private readonly ManualD.ISubscriptionRecordProvider manualProvider;
        private readonly PaypalD.ISubscriptionFullRecordProvider paypalProvider;
        private readonly PED.ISubscriptionFullRecordProvider peProvider;
        private readonly StripeD.ISubscriptionFullRecordProvider stripeProvider;

        public ClaimsService(ILogger<ClaimsService> logger, ManualD.ISubscriptionRecordProvider manualProvider, PaypalD.ISubscriptionFullRecordProvider paypalProvider, PED.ISubscriptionFullRecordProvider peProvider, StripeD.ISubscriptionFullRecordProvider stripeProvider)
        {
            this.logger = logger;
            this.manualProvider = manualProvider;
            this.paypalProvider = paypalProvider;
            this.peProvider = peProvider;
            this.stripeProvider = stripeProvider;
        }

        public override async Task<GetClaimsResponse> GetClaims(GetClaimsRequest request, ServerCallContext context)
        {
            if (request.UserID == null)
                return new GetClaimsResponse();

            Guid userId;
            if (!Guid.TryParse(request.UserID, out userId))
                return new GetClaimsResponse();

            var res = new GetClaimsResponse();

            var claims = await GetPaymentClaims(userId);

            res.Claims.AddRange(claims);

            return res;
        }
        private async Task<ClaimRecord[]> GetPaymentClaims(Guid userId)
        {
            var bestRecord = await GetBestSubscription(userId);

            if (bestRecord == null || bestRecord.AmountCents < 1)
                return Array.Empty<ClaimRecord>();

            if (bestRecord.PaidThruUTC.ToDateTime() < DateTime.UtcNow)
                return Array.Empty<ClaimRecord>();

            return bestRecord.ToClaimRecords();
        }

        private async Task<UnifiedSubscriptionRecord> GetBestSubscription(Guid userId)
        {
            var manualRecs = await manualProvider.GetAllByUserId(userId).ToList();
            var paypalRecs = await paypalProvider.GetAllByUserId(userId).ToList();
            var peRecs = await peProvider.GetAllByUserId(userId).ToList();
            var stripeRecs = await stripeProvider.GetAllByUserId(userId).ToList();

            var recs = new List<UnifiedSubscriptionRecord>();
            recs.AddRange(manualRecs.Where(r => r.CanceledOnUTC == null).Select(r => new UnifiedSubscriptionRecord(r)));
            recs.AddRange(paypalRecs.Where(r => r.SubscriptionRecord.CanceledOnUTC == null).Select(r => new UnifiedSubscriptionRecord(r)));
            recs.AddRange(peRecs.Where(r => r.SubscriptionRecord.CanceledOnUTC == null).Select(r => new UnifiedSubscriptionRecord(r)));
            recs.AddRange(stripeRecs.Where(r => r.SubscriptionRecord.CanceledOnUTC == null).Select(r => new UnifiedSubscriptionRecord(r)));

            return recs.Where(r => r.PaidThruUTC.ToDateTime() > DateTime.UtcNow).OrderByDescending(r => r.PaidThruUTC).OrderByDescending(r => r.AmountCents).FirstOrDefault();
        }

        public class UnifiedSubscriptionRecord
        {
            public UnifiedSubscriptionRecord(ManualSubscriptionRecord r)
            {
                PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(DateTime.UtcNow.AddMonths(1));
                AmountCents = r.AmountCents;
                Service = "manual";
            }

            public UnifiedSubscriptionRecord(ParallelEconomySubscriptionFullRecord r)
            {
                PaidThruUTC = r.PaidThruUTC;
                AmountCents = r.SubscriptionRecord.AmountCents;
                Service = "pe";
            }

            public UnifiedSubscriptionRecord(PaypalSubscriptionFullRecord r)
            {
                PaidThruUTC = r.PaidThruUTC;
                AmountCents = r.SubscriptionRecord.AmountCents;
                Service = "paypal";
            }

            public UnifiedSubscriptionRecord(StripeSubscriptionFullRecord r)
            {
                PaidThruUTC = r.PaidThruUTC;
                AmountCents = r.SubscriptionRecord.AmountCents;
                Service = "stripe";
            }

            public Google.Protobuf.WellKnownTypes.Timestamp PaidThruUTC { get; set; }
            public uint AmountCents { get; set; }
            public string Service { get; set; }

            public ClaimRecord[] ToClaimRecords()
            {
                return
                [
                    new ClaimRecord()
                    {
                        Name = ONUser.SubscriptionLevelType,
                        Value = AmountCents.ToString(),
                        ExpiresOnUTC = PaidThruUTC
                    },
                    new ClaimRecord()
                    {
                        Name = ONUser.SubscriptionProviderType,
                        Value = Service,
                        ExpiresOnUTC = PaidThruUTC
                    }
                ];
            }
        }
    }
}
