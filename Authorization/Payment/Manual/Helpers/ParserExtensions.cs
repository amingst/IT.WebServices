using Google.Protobuf;
using IT.WebServices.Fragments.Authorization.Payment.Manual;
using IT.WebServices.Fragments.Content;
using IT.WebServices.Fragments.Generic;
using System;
using System.Data.Common;

namespace IT.WebServices.Authorization.Payment.Manual.Helpers
{
    public static class ParserExtensions
    {
        public static ManualSubscriptionRecord? ParseManualSubscriptionRecord(this DbDataReader rdr)
        {
            var record = new ManualSubscriptionRecord()
            {
                SubscriptionID = rdr["ManualSubscriptionID"] as string,
                UserID = rdr["UserID"] as string,
                AmountCents = (uint)rdr["AmountCents"],
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
    }
}
