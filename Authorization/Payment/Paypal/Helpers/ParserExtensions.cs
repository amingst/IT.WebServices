using IT.WebServices.Fragments.Authorization.Payment.Paypal;
using System.Data.Common;

namespace IT.WebServices.Authorization.Payment.Paypal.Helpers
{
    public static class ParserExtensions
    {
        public static PaypalSubscriptionRecord? ParsePaypalSubscriptionRecord(this DbDataReader rdr)
        {
            var record = new PaypalSubscriptionRecord()
            {
                SubscriptionID = rdr["PaypalInternalSubscriptionID"] as string ?? "",
                UserID = rdr["UserID"] as string ?? "",
                PaypalCustomerID = rdr["PaypalCustomerID"] as string ?? "",
                PaypalSubscriptionID = rdr["PaypalSubscriptionID"] as string ?? "",
                Status = (Fragments.Authorization.Payment.SubscriptionStatus)(byte)rdr["Status"],
                AmountCents = (uint)rdr["AmountCents"],
                TaxCents = (uint)rdr["TaxCents"],
                TaxRateThousandPercents = (uint)rdr["TaxRateThousandPercents"],
                TotalCents = (uint)rdr["TotalCents"],
                CreatedBy = rdr["CreatedBy"] as string ?? "",
                ModifiedBy = rdr["ModifiedBy"] as string ?? "",
                CanceledBy = rdr["CanceledBy"] as string ?? "",
            };

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                record.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["CanceledOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CanceledOnUTC"], DateTimeKind.Utc);
                record.CanceledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return record;
        }

        public static PaypalPaymentRecord? ParsePaypalPaymentRecord(this DbDataReader rdr)
        {
            var record = new PaypalPaymentRecord()
            {
                PaymentID = rdr["PaypalInternalPaymentID"] as string,
                SubscriptionID = rdr["PaypalInternalSubscriptionID"] as string,
                UserID = rdr["UserID"] as string,
                PaypalPaymentID = rdr["PaypalPaymentID"] as string,
                Status = (Fragments.Authorization.Payment.PaymentStatus)(byte)rdr["Status"],
                AmountCents = (uint)rdr["AmountCents"],
                TaxCents = (uint)rdr["TaxCents"],
                TaxRateThousandPercents = (uint)rdr["TaxRateThousandPercents"],
                TotalCents = (uint)rdr["TotalCents"],
                CreatedBy = rdr["CreatedBy"] as string ?? "",
                ModifiedBy = rdr["ModifiedBy"] as string ?? "",
            };

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                record.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ModifiedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PaidOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PaidOnUTC"], DateTimeKind.Utc);
                record.PaidOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["PaidThruUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["PaidThruUTC"], DateTimeKind.Utc);
                record.PaidThruUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return record;
        }
    }
}
