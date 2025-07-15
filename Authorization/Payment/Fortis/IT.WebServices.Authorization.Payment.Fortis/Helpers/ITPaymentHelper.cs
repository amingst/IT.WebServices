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
            return new()
            {
                ProcessorPaymentID = fRec.Id,
                AmountCents = (uint)(fRec.SubtotalAmount ?? 0),
                PaidOnUTC = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs).UtcDateTime),
                Status = ConvertStatus(fRec.StatusCode),
            };
        }

        public static GenericPaymentRecord ToPaymentRecord(this List11 fRec)
        {
            return new()
            {
                ProcessorPaymentID = fRec.Id,
                AmountCents = (uint)(fRec.SubtotalAmount ?? 0),
                PaidOnUTC = Timestamp.FromDateTimeOffset(DateTimeOffset.FromUnixTimeSeconds(fRec.CreatedTs).UtcDateTime),
                Status = ConvertStatus(fRec.StatusId),
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
