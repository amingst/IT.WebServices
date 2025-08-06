using FortisAPI.Standard.Models;
using Google.Protobuf.WellKnownTypes;
using IT.WebServices.Fragments.Authorization.Payment;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    internal static class ITPaymentHelper
    {
        public static GenericPaymentRecord ToPaymentRecord(this Data14 fRec)
        {
            var createdOn = DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs);
            var paidThru = createdOn.AddMonths(1).AddDays(2);
            return new()
            {
                ProcessorPaymentID = fRec.Id,
                Status = ConvertStatus(fRec.StatusCode),
                AmountCents = (uint)(fRec.TransactionAmount),
                TaxCents = 0,
                TaxRateThousandPercents = 0,
                TotalCents = (uint)(fRec.TransactionAmount),
                PaidOnUTC = Timestamp.FromDateTimeOffset(createdOn.UtcDateTime),
                PaidThruUTC = Timestamp.FromDateTimeOffset(paidThru.UtcDateTime),
            };
        }

        public static GenericPaymentRecord ToPaymentRecord(this List11 fRec)
        {
            var paidOn = DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs);
            var paidThru = paidOn.AddMonths(1).AddDays(2);
            return new()
            {
                ProcessorPaymentID = fRec.Id,
                Status = ConvertStatus(fRec.StatusId),
                AmountCents = (uint)(fRec.TransactionAmountInt),
                TaxCents = 0,
                TaxRateThousandPercents = 0,
                TotalCents = (uint)(fRec.TransactionAmountInt),
                PaidOnUTC = Timestamp.FromDateTimeOffset(paidOn.UtcDateTime),
                PaidThruUTC = Timestamp.FromDateTimeOffset(paidThru.UtcDateTime),
            };
        }

        public static GenericPaymentRecord ToPaymentRecord(this ResponseTransaction fRec) => fRec.Data.ToPaymentRecord();

        public static List<GenericPaymentRecord> ToPaymentRecords(this ResponseTransactionsCollection fRec)
        {
            return fRec?.List
                        .Select(r => r?.ToPaymentRecord())
                        .Where(r => r is not null)
                        .Select(r => r!)
                        .ToList() ?? new List<GenericPaymentRecord>();
        }

        private static PaymentStatus ConvertStatus(StatusId2Enum? statusId)
        {
            switch (statusId)
            {
                case StatusId2Enum.Enum101:
                    return PaymentStatus.PaymentComplete;
            }

            return PaymentStatus.PaymentFailed;
        }
    }
}
