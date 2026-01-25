using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Authorization.Payment;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace IT.WebServices.Authorization.Payment.Stripe.Helpers
{
    internal static class ITSubscriptionHelper
    {
        public static GenericSubscriptionRecord ToSubscriptionRecord(this Subscription pRec)
        {
            var amount = (uint)(pRec.Items.FirstOrDefault()?.Plan?.Amount ?? 0);
            var status = ConvertStatus(pRec.Status);

            return new()
            {
                ProcessorName = PaymentConstants.PROCESSOR_NAME_STRIPE,
                ProcessorSubscriptionID = pRec.Id,
                ProcessorCustomerID = pRec.CustomerId,
                CreatedOnUTC = Timestamp.FromDateTime(pRec.Created),
                ModifiedOnUTC = Timestamp.FromDateTime(DateTime.UtcNow),
                CanceledOnUTC = status == SubscriptionStatus.SubscriptionStopped ? Timestamp.FromDateTime(DateTime.UtcNow) : new(),
                Status = status,
                AmountCents = amount,
                TaxCents = 0,
                TaxRateThousandPercents = 0,
                TotalCents = amount,
            };
        }

        private static SubscriptionStatus ConvertStatus(string status)
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
    }
}
