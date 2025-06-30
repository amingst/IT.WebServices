using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment.Fortis
{
    public sealed partial class FortisSubscriptionFullRecord : pb::IMessage<FortisSubscriptionFullRecord>
    {
        public void CalculateRecords()
        {
            var last = Payments.OrderBy(p => p.PaidOnUTC).LastOrDefault();
            if (last == null)
                return;

            LastPaidUTC = last.PaidOnUTC;
            PaidThruUTC = last.PaidThruUTC;
            RenewsOnUTC = pb.WellKnownTypes.Timestamp.FromDateTimeOffset(last.PaidOnUTC.ToDateTimeOffset().AddMonths(1));
        }
    }
}
