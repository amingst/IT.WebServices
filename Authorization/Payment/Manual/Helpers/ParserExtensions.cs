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
                CancelledBy = rdr["CancelledBy"] as string ?? "",
                ModifiedBy = rdr["ModifiedBy"] as string ?? "",
            };

            DateTime d;
            if (!(rdr["CreatedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CreatedOnUTC"], DateTimeKind.Utc);
                record.CreatedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["CancelledOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["CancelledOnUTC"], DateTimeKind.Utc);
                record.CancelledOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            if (!(rdr["ChangedOnUTC"] is DBNull))
            {
                d = DateTime.SpecifyKind((DateTime)rdr["ModifiedOnUTC"], DateTimeKind.Utc);
                record.ModifiedOnUTC = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(d);
            }

            return record;
        }
    }
}
