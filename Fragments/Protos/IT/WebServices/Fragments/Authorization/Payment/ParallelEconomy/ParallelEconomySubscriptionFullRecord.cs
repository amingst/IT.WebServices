using IT.WebServices.Fragments.Generic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using pb = global::Google.Protobuf;

namespace IT.WebServices.Fragments.Authorization.Payment.ParallelEconomy
{
    public sealed partial class ParallelEconomySubscriptionFullRecord : pb::IMessage<ParallelEconomySubscriptionFullRecord>
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
