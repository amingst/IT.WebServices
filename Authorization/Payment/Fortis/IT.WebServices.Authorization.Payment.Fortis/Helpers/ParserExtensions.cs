using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Authorization.Payment.Fortis;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Generic;
using System;
using System.Data.Common;

namespace IT.WebServices.Authorization.Payment.Fortis.Helpers
{
    public static class ParserExtensions
    {
        public static FortisSubscriptionRecord? ParseFortisSubscriptionRecord(this DbDataReader rdr)
        {
            var record = new FortisSubscriptionRecord()
            {
                SubscriptionID = rdr["FortisInternalSubscriptionID"] as string,
                UserID = rdr["UserID"] as string,
                FortisCustomerID = rdr["FortisCustomerID"] as string,
                FortisSubscriptionID = rdr["FortisSubscriptionID"] as string,
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

        public static FortisPaymentRecord? ParseFortisPaymentRecord(this DbDataReader rdr)
        {
            var record = new FortisPaymentRecord()
            {
                PaymentID = rdr["FortisInternalPaymentID"] as string,
                SubscriptionID = rdr["FortisInternalSubscriptionID"] as string,
                UserID = rdr["UserID"] as string,
                FortisPaymentID = rdr["FortisPaymentID"] as string,
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
