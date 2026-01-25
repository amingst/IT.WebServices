using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Authorization.Payment;
using Stripe;
using System;
using System.Collections.Generic;
using System.Text;

namespace IT.WebServices.Authorization.Payment.Stripe.Helpers
{
    internal static class ITPaymentHelper
    {
        public static GenericPaymentRecord ToPaymentRecord(this Invoice pRec)
        {
            var createdOn = pRec.Created;
            var paidThru = createdOn.AddMonths(1).AddDays(2);
            return new()
            {
                ProcessorPaymentID = pRec.Id,
                Status = ConvertStatus(pRec.Status),
                AmountCents = (uint)(pRec.AmountPaid),
                TaxCents = 0,
                TaxRateThousandPercents = 0,
                TotalCents = (uint)(pRec.AmountPaid),
                CreatedOnUTC = Timestamp.FromDateTime(pRec.Created),
                ModifiedOnUTC = Timestamp.FromDateTime(pRec.Created),
                PaidOnUTC = Timestamp.FromDateTime(pRec.Created),
                PaidThruUTC = Timestamp.FromDateTime(paidThru),
            };
        }

        private static PaymentStatus ConvertStatus(string status)
        {
            switch (status)
            {
                case "draft":
                case "open":
                case "requires_payment_method":
                case "requires_confirmation":
                case "requires_capture":
                case "requires_action":
                case "processing":
                    return PaymentStatus.PaymentPending;
                case "paid":
                case "succeeded":
                    return PaymentStatus.PaymentComplete;
                case "canceled":
                case "uncollectible":
                case "void":
                default:
                    return PaymentStatus.PaymentFailed;
            }
        }
    }
}
